using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using SS.Plugin.Widgets.ProductReview.Domains;
using SS.Plugin.Widgets.ProductReview.Factories;
using SS.Plugin.Widgets.ProductReview.Models;
using SS.Plugin.Widgets.ProductReview.Services;

namespace SS.Plugin.Widgets.ProductReview.Controllers
{
    public partial class WidgetProductReviewController : BasePublicController
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IStoreContext _storeContext;
        private readonly CaptchaSettings _captchaSettings;
        private readonly IReviewTypeService _reviewTypeService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWidgetProductReviewModelFactory _widgetProductReviewModelFactory;
        private readonly IWidgetProductReviewService _productReviewService;
        private readonly ProductReviewSettings _productReviewSettings;
        #endregion

        #region Ctor
        public WidgetProductReviewController(IProductService productService,
              CatalogSettings catalogSettings,
              IWorkContext workContext,
              ILocalizationService localizationService,
              ICustomerService customerService,
              IOrderService orderService,
              IPictureService pictureService,
              IStoreContext storeContext,
              CaptchaSettings captchaSettings,
              IReviewTypeService reviewTypeService,
              IWorkflowMessageService workflowMessageService,
              ICustomerActivityService customerActivityService,
              LocalizationSettings localizationSettings,
              IEventPublisher eventPublisher,
              IWidgetProductReviewModelFactory widgetProductReviewModelFactory,
              IWidgetProductReviewService productReviewService,
              ProductReviewSettings productReviewSettings)
        {
            _productService = productService;
            _catalogSettings = catalogSettings;
            _workContext = workContext;
            _localizationService = localizationService;
            _customerService = customerService;
            _orderService = orderService;
            _pictureService = pictureService;
            _storeContext = storeContext;
            _captchaSettings = captchaSettings;
            _reviewTypeService = reviewTypeService;
            _workflowMessageService = workflowMessageService;
            _customerActivityService = customerActivityService;
            _localizationSettings = localizationSettings;
            _eventPublisher = eventPublisher;
            _widgetProductReviewModelFactory = widgetProductReviewModelFactory;
            _productReviewService = productReviewService;
            _productReviewSettings = productReviewSettings;
        }
        #endregion


        #region Utilities

        protected virtual async Task ValidateProductReviewAvailabilityAsync(Product product)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsGuestAsync(customer) && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Reviews.OnlyRegisteredUsersCanWriteReviews"));

            if (!_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing)
                return;

            var hasCompletedOrders = product.ProductType == ProductType.SimpleProduct
                ? await HasCompletedOrdersAsync(product)
                : await (await _productService.GetAssociatedProductsAsync(product.Id)).AnyAwaitAsync(HasCompletedOrdersAsync);

            if (!hasCompletedOrders)
                ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));
        }

        protected virtual async ValueTask<bool> HasCompletedOrdersAsync(Product product)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            return (await _orderService.SearchOrdersAsync(customerId: customer.Id,
                productId: product.Id,
                osIds: new List<int> { (int)OrderStatus.Complete },
                pageSize: 1)).Any();
        }


        #endregion

        #region Method

        [HttpPost, ActionName("ProductReviews")]
        [FormValueRequired("add-product-review")]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> ProductReviewsAdd(int productId, ProductReviewsModel model, bool captchaValid, IFormCollection form)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews ||
                !await _productService.CanAddReviewAsync(product.Id, _catalogSettings.ShowProductReviewsPerStore ? currentStore.Id : 0))
                return RedirectToRoute("Homepage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage && !captchaValid)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            await ValidateProductReviewAvailabilityAsync(product);
            string pictureId = form["hdnProductReviewPictureId"];
            var pId = pictureId.Split(",");
            var pictureCount = pId.Count();
            if(_productReviewSettings.MaxPicture < pictureCount)
            {
                ModelState.AddModelError("ProductReviewPictureValidation", string.Format(await _localizationService.GetResourceAsync("SS.Plugin.Widgets.ProductReview.PictureMaxValidator"), _productReviewSettings.MaxPicture));
            }
            if (ModelState.IsValid)
            {
                //save review
                var rating = model.AddProductReview.Rating;
                if (rating < 1 || rating > 5)
                    rating = _catalogSettings.DefaultProductRatingValue;
                var isApproved = !_catalogSettings.ProductReviewsMustBeApproved;
                var customer = await _workContext.GetCurrentCustomerAsync();

                var productReview = new Nop.Core.Domain.Catalog.ProductReview
                {
                    ProductId = product.Id,
                    CustomerId = customer.Id,
                    Title = model.AddProductReview.Title,
                    ReviewText = model.AddProductReview.ReviewText,
                    Rating = rating,
                    HelpfulYesTotal = 0,
                    HelpfulNoTotal = 0,
                    IsApproved = isApproved,
                    CreatedOnUtc = DateTime.UtcNow,
                    StoreId = currentStore.Id,
                };

                await _productService.InsertProductReviewAsync(productReview);                

                var productReviewImages = new ProductReviewImages
                {
                    ProductReviewId = productReview.Id,
                    PictureId = pictureId,
                    VideoUrl = model.AddProductReview.VideoUrl
                };
                await _productReviewService.InsertProductReviewImages(productReviewImages);


                //add product review and review type mapping                
                foreach (var additionalReview in model.AddAdditionalProductReviewList)
                {
                    var additionalProductReview = new ProductReviewReviewTypeMapping
                    {
                        ProductReviewId = productReview.Id,
                        ReviewTypeId = additionalReview.ReviewTypeId,
                        Rating = additionalReview.Rating
                    };

                    await _reviewTypeService.InsertProductReviewReviewTypeMappingsAsync(additionalProductReview);
                }

                //update product totals
                await _productService.UpdateProductReviewTotalsAsync(product);

                //notify store owner
                if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                    await _workflowMessageService.SendProductReviewStoreOwnerNotificationMessageAsync(productReview, _localizationSettings.DefaultAdminLanguageId);

                //activity log
                await _customerActivityService.InsertActivityAsync("PublicStore.AddProductReview",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddProductReview"), product.Name), product);

                //raise event
                if (productReview.IsApproved)
                    await _eventPublisher.PublishAsync(new ProductReviewApprovedEvent(productReview));

                model = await _widgetProductReviewModelFactory.PrepareWidgetProductReviewsModelAsync(model, product);
                model.AddProductReview.Title = null;
                model.AddProductReview.ReviewText = null;

                model.AddProductReview.SuccessfullyAdded = true;
                if (!isApproved)
                    model.AddProductReview.Result = await _localizationService.GetResourceAsync("Reviews.SeeAfterApproving");
                else
                    model.AddProductReview.Result = await _localizationService.GetResourceAsync("Reviews.SuccessfullyAdded");

                return View(model);
            }

            //if we got this far, something failed, redisplay form
            model = await _widgetProductReviewModelFactory.PrepareWidgetProductReviewsModelAsync(model, product);
            return View(model);
        }

        public virtual async Task<IActionResult> ProductReviews(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("Homepage");

            var model = new ProductReviewsModel();
            model = await _widgetProductReviewModelFactory.PrepareWidgetProductReviewsModelAsync(model, product);

            await ValidateProductReviewAvailabilityAsync(product);

            //default value
            model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;

            //default value for all additional review types
            if (model.ReviewTypeList.Count > 0)
                foreach (var additionalProductReview in model.AddAdditionalProductReviewList)
                {
                    additionalProductReview.Rating = additionalProductReview.IsRequired ? _catalogSettings.DefaultProductRatingValue : 0;
                }

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AsyncPicture(IFormFile file)
        {
            var picturesFormatProvider = Request.Form.Files.ToList();  
            
            if(picturesFormatProvider.Count == 0)
                return Json(new { success = false, message = await _localizationService.GetResourceAsync("SS.Plugin.Widgets.ProductReview.FileNotUpload") });

            if(picturesFormatProvider.Count > _productReviewSettings.MaxPicture)
                return Json(new { success = false, message = string.Format(await _localizationService.GetResourceAsync("SS.Plugin.Widgets.ProductReview.PictureMaxValidator"), _productReviewSettings.MaxPicture) });


            var fileModel = new List<UploadFileModel>();
            
            int invaliedImage = 0;
            int largerImage = 0;
            foreach (var formatProvider in picturesFormatProvider)
            {
                if (formatProvider == null)
                    invaliedImage++;

                var imagesize = formatProvider.Length / 1048576.0;
                if (imagesize > _productReviewSettings.PictureSize)
                    largerImage++;

                var picture = await _pictureService.InsertPictureAsync(formatProvider);
                if (picture != null)
                {
                    fileModel.Add(new UploadFileModel()
                    {
                        FileName = formatProvider.FileName,
                        PictureId = picture.Id
                    });
                }
                else
                {
                    invaliedImage++;
                }
            }
            var message = "";
            if (invaliedImage > 0)
            {
                message += string.Format(await _localizationService.GetResourceAsync("SS.Plugin.Widgets.ProductReview.PictureInvaliedValidator"), invaliedImage);
            }
            if (largerImage >0)
            {
                message += string.Format(await _localizationService.GetResourceAsync("SS.Plugin.Widgets.ProductReview.PictureMaxValidatorInSelected"), largerImage, _productReviewSettings.MaxPicture);
            }
            return Json(new
            {
                success = true,
                pictureId = string.Join(",", fileModel.Select(p => p.PictureId).ToList()),
                fileModel = fileModel,
                message = message
            });
            

        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductReviewAjax(int productId, string pageIndex)
        {
            int.TryParse(pageIndex, out var index);
            var model = await _widgetProductReviewModelFactory.PrepareProductReviewsModelAjaxAsync(productId, index);
            var html = await RenderPartialViewToStringAsync("_ProductReviewsList", model.Items);
            return Json(new
            {
                html = html
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteProductReviewPicture(int pictureId, string picIds)
        {
            var picture = await _pictureService.GetPictureByIdAsync(pictureId);

            var reviewPictureIds = new List<string>();
            if (picture != null)
            {
                var picturesIds = picIds.Split(",");
                foreach (var item in picturesIds)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (Convert.ToInt32(item) != pictureId)
                        {
                            reviewPictureIds.Add(item);
                        }
                    }
                }                
                await _pictureService.DeletePictureAsync(picture);                
                return Json(new
                {
                    success = true,
                    reviewPictureIds = string.Join(",", reviewPictureIds),  
                    reviewCount = reviewPictureIds.Count()
                });
            }
            return Json(new
            {
                success = false,
            });
        }
        #endregion
    }
}
