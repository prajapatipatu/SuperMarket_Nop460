using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using SS.Plugin.Widgets.ProductReview.Infrastructure.Cache;
using SS.Plugin.Widgets.ProductReview.Models;
using SS.Plugin.Widgets.ProductReview.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SS.Plugin.Widgets.ProductReview.Factories
{
    public partial class WidgetProductReviewModelFactory : IWidgetProductReviewModelFactory
    {

        #region fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;        
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IReviewTypeService _reviewTypeService;
        private readonly IStoreContext _storeContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;        
        private readonly IWidgetProductReviewService _widgetProductReviewService;
        private readonly IStaticCacheManager _staticCacheManager;
        #endregion

        #region Ctor

        public WidgetProductReviewModelFactory(CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CustomerSettings customerSettings,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,            
            ILocalizationService localizationService,
            IPictureService pictureService,
            IProductService productService,
            IReviewTypeService reviewTypeService,
            IStoreContext storeContext,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,            
            IWidgetProductReviewService widgetProductReviewService,
            IStaticCacheManager staticCacheManager)
        {
            _captchaSettings = captchaSettings;
            _catalogSettings = catalogSettings;
            _customerSettings = customerSettings;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;            
            _localizationService = localizationService;
            _pictureService = pictureService;
            _productService = productService;
            _reviewTypeService = reviewTypeService;
            _storeContext = storeContext;
            _urlRecordService = urlRecordService;
            _workContext = workContext;            
            _widgetProductReviewService = widgetProductReviewService;
            _staticCacheManager = staticCacheManager;
        }
        #endregion


        /// <summary>
        /// Prepare the product reviews model
        /// </summary>
        /// <param name="model">Product reviews model</param>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product reviews model
        /// </returns>
        public virtual async Task<ProductReviewsModel> PrepareWidgetProductReviewsModelAsync(ProductReviewsModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var currentStore = await _storeContext.GetCurrentStoreAsync();

            model.ProductId = product.Id;
            model.ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
            model.ProductSeName = await _urlRecordService.GetSeNameAsync(product);

            var productReviews = await _productService.GetAllProductReviewsAsync(
                approved: true,
                productId: product.Id,
                storeId: _catalogSettings.ShowProductReviewsPerStore ? currentStore.Id : 0);

            //get all review types
            foreach (var reviewType in await _reviewTypeService.GetAllReviewTypesAsync())
            {
                model.ReviewTypeList.Add(new ReviewTypeModel
                {
                    Id = reviewType.Id,
                    Name = await _localizationService.GetLocalizedAsync(reviewType, entity => entity.Name),
                    Description = await _localizationService.GetLocalizedAsync(reviewType, entity => entity.Description),
                    VisibleToAllCustomers = reviewType.VisibleToAllCustomers,
                    DisplayOrder = reviewType.DisplayOrder,
                    IsRequired = reviewType.IsRequired,
                });
            }

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            foreach (var rt in model.ReviewTypeList)
            {
                if (model.ReviewTypeList.Count <= model.AddAdditionalProductReviewList.Count)
                    continue;
                var reviewType = await _reviewTypeService.GetReviewTypeByIdAsync(rt.Id);
                var reviewTypeMappingModel = new AddProductReviewReviewTypeMappingModel
                {
                    ReviewTypeId = rt.Id,
                    Name = await _localizationService.GetLocalizedAsync(reviewType, entity => entity.Name),
                    Description = await _localizationService.GetLocalizedAsync(reviewType, entity => entity.Description),
                    DisplayOrder = rt.DisplayOrder,
                    IsRequired = rt.IsRequired,
                };

                model.AddAdditionalProductReviewList.Add(reviewTypeMappingModel);
            }

            //Average rating
            foreach (var rtm in model.ReviewTypeList)
            {
                var totalRating = 0;
                var totalCount = 0;
                foreach (var item in model.Items)
                {
                    foreach (var q in item.AdditionalProductReviewList.Where(w => w.ReviewTypeId == rtm.Id))
                    {
                        totalRating += q.Rating;
                        totalCount = ++totalCount;
                    }
                }

                rtm.AverageRating = (double)totalRating / (totalCount > 0 ? totalCount : 1);
            }

            model.AddProductReview.CanCurrentCustomerLeaveReview = _catalogSettings.AllowAnonymousUsersToReviewProduct || !await _customerService.IsGuestAsync(currentCustomer);
            model.AddProductReview.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage;
            model.AddProductReview.CanAddNewReview = await _productService.CanAddReviewAsync(product.Id, _catalogSettings.ShowProductReviewsPerStore ? currentStore.Id : 0);

            return model;
        }

        /// <summary>
        /// Prepare the product reviews model
        /// </summary>
        /// <param name="productId">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product reviews model
        /// </returns>
        public virtual async Task<ProductReviewsModel> PrepareWidgetProductReviewsDetailsModelAsync(int productId)
        {
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var language = await _workContext.GetWorkingLanguageAsync();
            var currentcustomer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(currentcustomer);

            var productReviewRatingCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ProductReviewModelEventConsumer.ProductReviewRatingpageKey,
               productId, language, customerRoleIds, currentStore);

            var ratingCacheKey = await _staticCacheManager.GetAsync(productReviewRatingCacheKey, async () =>
            {
                var model = new ProductReviewsModel();

                var productReviews = await _widgetProductReviewService.GetAllRatingAsync(productId);

                model.RatingOne = productReviews.Any(x => x.Rating == 1) ? productReviews.FirstOrDefault(x => x.Rating == 1).Count : 0;
                model.RatingTwo = productReviews.Any(x => x.Rating == 2) ? productReviews.FirstOrDefault(x => x.Rating == 2).Count : 0;
                model.RatingThree = productReviews.Any(x => x.Rating == 3) ? productReviews.FirstOrDefault(x => x.Rating == 3).Count : 0;
                model.RatingFour = productReviews.Any(x => x.Rating == 4) ? productReviews.FirstOrDefault(x => x.Rating == 4).Count : 0;
                model.RatingFive = productReviews.Any(x => x.Rating == 5) ? productReviews.FirstOrDefault(x => x.Rating == 5).Count : 0;

                decimal totalRatingSum = productReviews.Sum(p => p.Count);
                decimal ratingSum = 5 * model.RatingFive + 4 * model.RatingFour + 3 * model.RatingThree + 2 * model.RatingTwo + 1 * model.RatingOne;

                //TotalRating in product
                model.TotalRating = (int)totalRatingSum;
                model.ProductId = productId;

                if (ratingSum > 0 && totalRatingSum > 0)
                {
                    //Total Rating Average
                    model.RatingAverage = Math.Round(ratingSum / totalRatingSum, 2);

                    if (model.RatingFive > 0)
                        model.RatingFivePercent = Convert.ToInt32(model.RatingFive * 100 / totalRatingSum);

                    if (model.RatingFour > 0)
                        model.RatingFourPercent = Convert.ToInt32(model.RatingFour * 100 / totalRatingSum);

                    if (model.RatingThree > 0)
                        model.RatingThreePercent = Convert.ToInt32(model.RatingThree * 100 / totalRatingSum);

                    if (model.RatingTwo > 0)
                        model.RatingTwoPercent = Convert.ToInt32(model.RatingTwo * 100 / totalRatingSum);

                    if (model.RatingOne > 0)
                        model.RatingOnePercent = Convert.ToInt32(model.RatingOne * 100 / totalRatingSum);
                }


                //get all review types
                foreach (var reviewType in await _reviewTypeService.GetAllReviewTypesAsync())
                {
                    model.ReviewTypeList.Add(new ReviewTypeModel
                    {
                        Id = reviewType.Id,
                        Name = await _localizationService.GetLocalizedAsync(reviewType, entity => entity.Name),
                        Description = await _localizationService.GetLocalizedAsync(reviewType, entity => entity.Description),
                        VisibleToAllCustomers = reviewType.VisibleToAllCustomers,
                        DisplayOrder = reviewType.DisplayOrder,
                        IsRequired = reviewType.IsRequired,
                    });
                }
                return model;
            });
            return ratingCacheKey;
        }

        /// <summary>
        /// Prepare the product reviews model
        /// </summary>
        /// <param name="productId">Product</param>
        /// <param name="pageIndex">pageIndex</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product reviews model
        /// </returns>
        public virtual async Task<ProductReviewsModel> PrepareProductReviewsModelAjaxAsync(int productId, int pageIndex)
        {
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var language = await _workContext.GetWorkingLanguageAsync();
            var currentcustomer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(currentcustomer);

            var model = new ProductReviewsModel();

            var productReviews = await _productService.GetAllProductReviewsAsync(
             approved: true,
             productId: productId,
             storeId: _catalogSettings.ShowProductReviewsPerStore ? currentStore.Id : 0,
             pageIndex: pageIndex,
             pageSize: 5);

            var productReviewCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ProductReviewModelEventConsumer.ProductReviewpageKey,
               productReviews, language, customerRoleIds, currentStore);

            var reviewCacheKey = await _staticCacheManager.GetAsync(productReviewCacheKey, async () =>
            {
                //filling data from db
                foreach (var pr in productReviews)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(pr.CustomerId);

                    var productReviewModel = new ProductReviewModel
                    {
                        Id = pr.Id,
                        CustomerId = pr.CustomerId,
                        CustomerName = await _customerService.FormatUsernameAsync(customer),
                        AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !await _customerService.IsGuestAsync(customer),
                        Title = pr.Title,
                        ReviewText = pr.ReviewText,
                        ReplyText = pr.ReplyText,
                        Rating = pr.Rating,
                        Helpfulness = new ProductReviewHelpfulnessModel
                        {
                            ProductReviewId = pr.Id,
                            HelpfulYesTotal = pr.HelpfulYesTotal,
                            HelpfulNoTotal = pr.HelpfulNoTotal,
                        },
                        WrittenOnStr = (await _dateTimeHelper.ConvertToUserTimeAsync(pr.CreatedOnUtc, DateTimeKind.Utc)).ToString("g"),
                    };

                    var productReviewImages = await _widgetProductReviewService.GetProductReviewImagesByProductReviewIdAsync(pr.Id);
                    if (productReviewImages != null)
                    {
                        var picturesUrl = new List<string>();
                        if (!string.IsNullOrEmpty(productReviewImages.PictureId))
                        {
                            var productIds = productReviewImages.PictureId.Split(",");
                            foreach (var item in productIds)
                            {
                                if (!string.IsNullOrEmpty(item))
                                {
                                    var pictureThumbnailUrl = await _pictureService.GetPictureUrlAsync(Convert.ToInt32(item));
                                    if (string.IsNullOrEmpty(pictureThumbnailUrl))
                                        pictureThumbnailUrl = await _pictureService.GetDefaultPictureUrlAsync(targetSize: 1);

                                    picturesUrl.Add(pictureThumbnailUrl);
                                }
                            }
                        }
                        productReviewModel.PictureUrl = picturesUrl;
                        productReviewModel.VideoUrl = productReviewImages.VideoUrl;
                    }
                    model.Items.Add(productReviewModel);
                }
                return model;
            });
            return reviewCacheKey;
        }
    }
}
