using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Http.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Components;
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.ShoppingCart;
using Nop.Plugin.InstantOnePageCheckout.Components;

namespace Nop.Plugin.InstantOnePageCheckout.Controllers
{
    public partial class OnePageCheckoutController : BasePluginController
    {
        #region Fields 

        private readonly OrderSettings _orderSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerService _customerService;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressService _addressService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly AddressSettings _addressSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly IShippingService _shippingService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly PaymentSettings _paymentSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IPermissionService _permissionService;
        private readonly IDiscountService _discountService;

        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IDownloadService _downloadService;
        private readonly IGiftCardService _giftCardService;
        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly IProductService _productService;
        private readonly CustomerSettings _customerSettings;
        private readonly InstantOnePageCheckOutSetting _instantOnePageCheckOutSetting;

        #endregion

        #region Ctor

        public OnePageCheckoutController(OrderSettings orderSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            ICheckoutModelFactory checkoutModelFactory,
            ILocalizationService localizationService,
            ILogger logger,
            IAddressAttributeParser addressAttributeParser,
            IAddressService addressService,
            IGenericAttributeService genericAttributeService,
            AddressSettings addressSettings,
            ShippingSettings shippingSettings,
            IShippingService shippingService,
            IOrderProcessingService orderProcessingService,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IPermissionService permissionService,
            IDiscountService discountService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            IDownloadService downloadService,
            IGiftCardService giftCardService,
            IOrderService orderService,
            IWebHelper webHelper,
            IProductService productService,
            CustomerSettings customerSettings,
            InstantOnePageCheckOutSetting instantOnePageCheckOutSetting
            )
        {
            _orderSettings = orderSettings;
            _workContext = workContext;
            _storeContext = storeContext;
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _checkoutModelFactory = checkoutModelFactory;
            _localizationService = localizationService;
            _logger = logger;
            _addressAttributeParser = addressAttributeParser;
            _addressService = addressService;
            _genericAttributeService = genericAttributeService;
            _addressSettings = addressSettings;
            _shippingSettings = shippingSettings;
            _shippingService = shippingService;
            _orderProcessingService = orderProcessingService;
            _paymentSettings = paymentSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _permissionService = permissionService;
            _discountService = discountService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
            _downloadService = downloadService;
            _giftCardService = giftCardService;
            _orderService = orderService;
            _webHelper = webHelper;
            _productService = productService;
            _customerSettings = customerSettings;
            _instantOnePageCheckOutSetting = instantOnePageCheckOutSetting;
        }


        #endregion

        #region Utilities

        /// <summary>
        /// Parses the value indicating whether the "pickup in store" is allowed
        /// </summary>
        /// <param name="form">The form</param>
        /// <returns>The value indicating whether the "pickup in store" is allowed</returns>
        protected virtual bool ParsePickupInStore(IFormCollection form)
        {
            var pickupInStore = false;

            var pickupInStoreParameter = form["PickupInStore"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(pickupInStoreParameter))
                _ = bool.TryParse(pickupInStoreParameter, out pickupInStore);

            return pickupInStore;
        }

        /// <summary>
        /// Parses the pickup option
        /// </summary>
        /// <param name="form">The form</param>
        /// <returns>
        /// The task result contains the pickup option
        /// </returns>
        protected virtual async Task<PickupPoint> ParsePickupOptionAsync(IFormCollection form)
        {
            
            var pickupPoint = form["pickup-points-id"].ToString().Split(new[] { "___" }, StringSplitOptions.None);

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var address = await _addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0);
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId: store.Id);

            var selectedPoint = (await _shippingService.GetPickupPointsAsync(cart, address,
                customer, pickupPoint[1], store.Id)).PickupPoints.FirstOrDefault(x => x.Id.Equals(pickupPoint[0]));

            if (selectedPoint == null)
                throw new Exception("Pickup point is not allowed");

            return selectedPoint;
        }

        /// <summary>
        /// Saves the pickup option
        /// </summary>
        /// <param name="pickupPoint">The pickup option</param>
        protected virtual async Task SavePickupOptionAsync(PickupPoint pickupPoint)
        {
            var name = !string.IsNullOrEmpty(pickupPoint.Name) ?
                string.Format(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.PickupPoints.Name"), pickupPoint.Name) :
                await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.PickupPoints.NullName");
            var pickUpInStoreShippingOption = new ShippingOption
            {
                Name = name,
                Rate = pickupPoint.PickupFee,
                Description = pickupPoint.Description,
                ShippingRateComputationMethodSystemName = pickupPoint.ProviderSystemName,
                IsPickupInStore = true
            };

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, pickUpInStoreShippingOption, store.Id);
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedPickupPointAttribute, pickupPoint, store.Id);
        }

        protected virtual async Task<JsonResult> OpcLoadStepAfterShippingMethod(IList<ShoppingCartItem> cart)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
            if (isPaymentWorkflowRequired)
            {
                //filter by country
                var filterByCountryId = 0;
                if (_addressSettings.CountryEnabled)
                {
                    filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId ?? 0;
                }

                //payment is required
                var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);

                //Check whether payment workflow is required
                //we ignore reward points during cart total calculation
                var paymentMethodHtml = await RenderPartialViewToStringAsync("_OpcPaymentMethods", paymentMethodModel);
                return Json(new
                {
                    success = true,
                    paymentMethod = true,
                    html = paymentMethodHtml
                });
            }

            //payment is not required
            await _genericAttributeService.SaveAttributeAsync<string>(customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, null, store.Id);

            return Json(new
            {
                success = true,
                paymentMethod = false,
                html = string.Empty
            });
        }

        protected virtual async Task<JsonResult> OpcLoadStepAfterPaymentMethod(IPaymentMethod paymentMethod, IList<ShoppingCartItem> cart)
        {
            if (paymentMethod.SkipPaymentInfo ||
                (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection && _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
            {
                //skip payment info page
                var paymentInfo = new ProcessPaymentRequest();

                //session save
                HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);

                return Json(new
                {
                    success = true,
                    paymentInfo = false,
                    html = string.Empty
                });
            }

            //return payment info page
            var paymenInfoModel = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);

            var paymentInfoHtml = await RenderPartialViewToStringAsync("_OpcPaymentInfo", paymenInfoModel);
            return Json(new
            {
                success = true,
                paymentInfo = true,
                html = paymentInfoHtml
            });
        }

        protected virtual async Task ParseAndSaveCheckoutAttributesAsync(IList<ShoppingCartItem> cart, IFormCollection form)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;
            var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
            var store = await _storeContext.GetCurrentStoreAsync();
            var checkoutAttributes = await _checkoutAttributeService.GetAllCheckoutAttributesAsync(store.Id, excludeShippableAttributes);
            foreach (var attribute in checkoutAttributes)
            {
                var controlId = $"checkout_attribute_{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _checkoutAttributeService.GetCheckoutAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }

                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date));
                            }
                            catch
                            {
                                // ignored
                            }

                            if (selectedDate.HasValue)
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                        }

                        break;
                    case AttributeControlType.FileUpload:
                        {
                            _ = Guid.TryParse(form[controlId], out var downloadGuid);
                            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                           attribute, download.DownloadGuid.ToString());
                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            //validate conditional attributes (if specified)
            foreach (var attribute in checkoutAttributes)
            {
                var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                    attributesXml = _checkoutAttributeParser.RemoveCheckoutAttribute(attributesXml, attribute);
            }

            //save checkout attributes
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.CheckoutAttributes, attributesXml, store.Id);
        }

        protected virtual async Task<bool> IsMinimumOrderPlacementIntervalValidAsync(Customer customer)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

            var store = await _storeContext.GetCurrentStoreAsync();

            var lastOrder = (await _orderService.SearchOrdersAsync(storeId: store.Id,
                customerId: customer.Id, pageSize: 1))
                .FirstOrDefault();
            if (lastOrder == null)
                return true;

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }

        #endregion

        #region Methods (One Page Checkout)
        public virtual async Task<IActionResult> Index()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();
            var downloadableProductsRequireRegistration =
                _customerSettings.RequireRegistrationForDownloadableProducts && await _productService.HasAnyDownloadableProductAsync(cartProductIds);

            if (await _customerService.IsGuestAsync(customer) && (!_orderSettings.AnonymousCheckoutAllowed || downloadableProductsRequireRegistration))
                return Challenge();

            //if we have only "button" payment methods available (displayed on the shopping cart page, not during checkout),
            //then we should allow standard checkout
            //all payment methods (do not filter by country here as it could be not specified yet)
            var paymentMethods = await (await _paymentPluginManager
                .LoadActivePluginsAsync(customer, store.Id))
                .WhereAwait(async pm => !await pm.HidePaymentMethodAsync(cart)).ToListAsync();
            //payment methods displayed during checkout (not with "Button" type)
            var nonButtonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType != PaymentMethodType.Button)
                .ToList();
            //"button" payment methods(*displayed on the shopping cart page)
            var buttonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .ToList();
            if (!nonButtonPaymentMethods.Any() && buttonPaymentMethods.Any())
                return RedirectToRoute("ShoppingCart");

            //reset checkout data
            await _customerService.ResetCheckoutDataAsync(customer, store.Id);

            //validation (cart)
            var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.CheckoutAttributes, store.Id);
            var scWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, checkoutAttributesXml, true);
            if (scWarnings.Any())
                return RedirectToRoute("ShoppingCart");
            //validation (each shopping cart item)
            foreach (var sci in cart)
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                var sciWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(customer,
                    sci.ShoppingCartType,
                    product,
                    sci.StoreId,
                    sci.AttributesXml,
                    sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc,
                    sci.RentalEndDateUtc,
                    sci.Quantity,
                    false,
                    sci.Id);
                if (sciWarnings.Any())
                    return RedirectToRoute("ShoppingCart");
            }

            if (!_instantOnePageCheckOutSetting.InstantOnePageCheckoutDisabled)
            {
                if (_orderSettings.OnePageCheckoutEnabled)
                    return RedirectToRoute("CheckoutOnePage");

                return RedirectToRoute("CheckoutBillingAddress");
            }

            return RedirectToRoute("SSOnePageCheckout");
        }

        public virtual async Task<IActionResult> SSOnePageCheckout()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            var model = await _checkoutModelFactory.PrepareOnePageCheckoutModelAsync(cart);

            return View(model);
        }

        public virtual async Task<IActionResult> GetBillingAddressList()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            var billingAddressModel = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart);

            var billinghtml = await RenderPartialViewToStringAsync("PartialBillingAddressList", billingAddressModel);
            return Json(new
            {
                success = true,
                html = billinghtml
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> NewBillingAddress()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            var model = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart, prePopulateNewAddressWithCustomerFields: true);
            var billinghtml = string.Empty;
            if (model != null)
            {
                billinghtml = await RenderPartialViewToStringAsync("PartialBillingAddress", model);
            }
            return Json(new
            {
                success = true,
                html = billinghtml
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> OpcSaveBilling(CheckoutBillingAddressModel model, IFormCollection form)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.Disabled"));

                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
                var billinghtml = string.Empty;

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                _ = int.TryParse(form["billing_address_id"], out var billingAddressId);

                if (billingAddressId > 0)
                {
                    //existing address
                    var address = await _customerService.GetCustomerAddressAsync(customer.Id, billingAddressId)
                        ?? throw new Exception(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.Address.NotFound"));

                    customer.BillingAddressId = address.Id;
                    await _customerService.UpdateCustomerAsync(customer);
                }
                else
                {
                    //new address
                    var newAddress = model.BillingNewAddress;

                    //custom address attributes
                    var customAttributes = await _addressAttributeParser.ParseCustomAddressAttributesAsync(form);
                    var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var billingAddressModel = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart,
                            selectedCountryId: newAddress.CountryId,
                            overrideAttributesXml: customAttributes);
                        billingAddressModel.NewAddressPreselected = true;
                        billinghtml = await RenderPartialViewToStringAsync("PartialBillingAddressList", billingAddressModel);
                        var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                        return Json(new
                        {
                            success = ModelState.ErrorCount > 0 ? false : true,
                            billingAddressId = newAddress.Id,
                            html = billinghtml,                            
                            message = message

                        });
                    }

                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes);

                    if (address == null)
                    {
                        //address is not found. let's create a new one
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        //some validation
                        if (address.CountryId == 0)
                            address.CountryId = null;

                        if (address.StateProvinceId == 0)
                            address.StateProvinceId = null;

                        await _addressService.InsertAddressAsync(address);

                        await _customerService.InsertCustomerAddressAsync(customer, address);
                    }

                    customer.BillingAddressId = address.Id;

                    await _customerService.UpdateCustomerAsync(customer);
                }

                if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                {
                    var shippingMethodModel = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer));
                    if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
                        shippingMethodModel.ShippingMethods.Count == 1)
                    {
                        //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                        await _genericAttributeService.SaveAttributeAsync(customer,
                            NopCustomerDefaults.SelectedShippingOptionAttribute,
                            shippingMethodModel.ShippingMethods.First().ShippingOption,
                            store.Id);
                    }
                }

                var billingModel = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart, prePopulateNewAddressWithCustomerFields: true);

                if (model != null)
                {
                    billinghtml = await RenderPartialViewToStringAsync("PartialBillingAddressList", billingModel);
                }
                return Json(new
                {
                    success = true,
                    billingAddressId = customer.BillingAddressId,
                    html = billinghtml
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> UpdateCustomerBillingAddress(int billingAddressId)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                throw new Exception(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.Disabled"));

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");


            if (billingAddressId > 0)
            {
                //existing address
                var address = await _customerService.GetCustomerAddressAsync(customer.Id, billingAddressId)
                    ?? throw new Exception(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.Address.NotFound"));

                customer.BillingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(customer);
            }
            return Json(new
            {
                success = true,
                billingAddressId = customer.BillingAddressId
            });
        }

        /// <summary>
        /// Delete edited address
        /// </summary>
        /// <param name="addressId"></param>
        public virtual async Task<IActionResult> DeleteEditAddress(int addressId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address != null)
            {
                await _customerService.RemoveCustomerAddressAsync(customer, address);
                await _customerService.UpdateCustomerAsync(customer);
                await _addressService.DeleteAddressAsync(address);
            }

            var billingAddressModel = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart);

            var billinghtml = await RenderPartialViewToStringAsync("PartialBillingAddressList", billingAddressModel);
            return Json(new
            {
                success = true,
                billingAddressId = 0,
                html = billinghtml
            });
        }

        /// <summary>
        /// Get specified Address by addresId
        /// </summary>
        /// <param name="addressId"></param>
        public virtual async Task<IActionResult> GetAddressById(int addressId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            var json = JsonConvert.SerializeObject(address, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            return Content(json, "application/json");
        }

        /// <summary>
        /// Save edited address
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<IActionResult> SaveEditAddress(CheckoutBillingAddressModel model)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
                var billinghtml = string.Empty;

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                //find address (ensure that it belongs to the current customer)
                var address = await _customerService.GetCustomerAddressAsync(customer.Id, model.BillingNewAddress.Id);
                if (address == null)
                    throw new Exception("Address can't be loaded");

                address = model.BillingNewAddress.ToEntity(address);
                await _addressService.UpdateAddressAsync(address);

                customer.BillingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(customer);

                var billingAddressModel = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart, address.CountryId);

                billinghtml = await RenderPartialViewToStringAsync("PartialBillingAddressList", billingAddressModel);
                return Json(new
                {
                    success = true,
                    selected_id = model.BillingNewAddress.Id,
                    html = billinghtml
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> NewShippingAddress()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            var shippinghtml = string.Empty;

            var shippingAddressModel = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart, prePopulateNewAddressWithCustomerFields: true);

            if (shippingAddressModel != null)
            {
                shippinghtml = await RenderPartialViewToStringAsync("PartialShippingAddress", shippingAddressModel);
            }
            return Json(new
            {
                success = true,
                html = shippinghtml
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> OpcSaveShipping(CheckoutShippingAddressModel model, IFormCollection form)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.Disabled"));

                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
                var shippinghtml = string.Empty;

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                    throw new Exception("Shipping is not required");

                //pickup point
                if (_shippingSettings.AllowPickupInStore && !_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
                {
                    var pickupInStore = ParsePickupInStore(form);
                    if (pickupInStore)
                    {
                        var pickupOption = await ParsePickupOptionAsync(form);
                        await SavePickupOptionAsync(pickupOption);

                        return Json(new
                        {
                            success = false
                        });
                    }

                    //set value indicating that "pick up in store" option has not been chosen
                    await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, store.Id);
                }

                _ = int.TryParse(form["shipping_address_id"], out var shippingAddressId);

                if (shippingAddressId > 0)
                {
                    //existing address
                    var address = await _customerService.GetCustomerAddressAsync(customer.Id, shippingAddressId)
                        ?? throw new Exception(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.Address.NotFound"));

                    customer.ShippingAddressId = address.Id;
                    await _customerService.UpdateCustomerAsync(customer);
                }
                else
                {
                    //new address
                    var newAddress = model.ShippingNewAddress;

                    //custom address attributes
                    var customAttributes = await _addressAttributeParser.ParseCustomAddressAttributesAsync(form);
                    var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes);

                    if (address == null)
                    {
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        await _addressService.InsertAddressAsync(address);

                        await _customerService.InsertCustomerAddressAsync(customer, address);
                    }

                    customer.ShippingAddressId = address.Id;

                    await _customerService.UpdateCustomerAsync(customer);
                }

                if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                {
                    var shippingMethodModel = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer));
                    if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
                        shippingMethodModel.ShippingMethods.Count == 1)
                    {
                        //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                        await _genericAttributeService.SaveAttributeAsync(customer,
                            NopCustomerDefaults.SelectedShippingOptionAttribute,
                            shippingMethodModel.ShippingMethods.First().ShippingOption,
                            store.Id);
                    }
                }

                //return await OpcLoadStepAfterShippingAddress(cart);
                var shippingModel = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart, prePopulateNewAddressWithCustomerFields: true);
                if (shippingModel != null)
                {
                    shippinghtml = await RenderPartialViewToStringAsync("PartialShippingAddressList", shippingModel);
                }
                return Json(new
                {
                    success = true,
                    shippingAddressId = customer.ShippingAddressId,
                    html = shippinghtml
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> UpdateCustomerShippingAddress(int shippingAddressId)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                throw new Exception(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.Disabled"));

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");


            if (shippingAddressId > 0)
            {
                //existing address
                var address = await _customerService.GetCustomerAddressAsync(customer.Id, shippingAddressId)
                    ?? throw new Exception(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.Address.NotFound"));

                customer.ShippingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(customer);
            }

            return Json(new
            {
                success = true,
                shippingAddressId = customer.ShippingAddressId
            });
        }

        public virtual async Task<IActionResult> GetShippingAddressList()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            var shippinghtml = string.Empty;

            var shippingModel = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart, prePopulateNewAddressWithCustomerFields: true);
            if (shippingModel != null)
            {
                shippinghtml = await RenderPartialViewToStringAsync("PartialShippingAddressList", shippingModel);
            }
            return Json(new
            {
                success = true,
                html = shippinghtml
            });
        }


        public virtual async Task<IActionResult> GetShippingMethod()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            var shippingMethodModel = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer));
            var shippingMethodHtml = string.Empty;

            if (shippingMethodModel != null)
            {
                shippingMethodHtml = await RenderPartialViewToStringAsync("_OpcShippingMethods", shippingMethodModel);
            }
            return Json(new
            {
                success = true,
                html = shippingMethodHtml
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> OpcSaveShippingMethod(string shippingoption, IFormCollection form)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.Disabled"));

                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                    throw new Exception("Shipping is not required");

                //pickup point
                if (_shippingSettings.AllowPickupInStore && _orderSettings.DisplayPickupInStoreOnShippingMethodPage)
                {
                    var pickupInStore = ParsePickupInStore(form);
                    if (pickupInStore)
                    {
                        var pickupOption = await ParsePickupOptionAsync(form);
                        await SavePickupOptionAsync(pickupOption);

                        return await OpcLoadStepAfterShippingMethod(cart);
                    }

                    //set value indicating that "pick up in store" option has not been chosen
                    await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, store.Id);
                }

                //parse selected method 
                if (string.IsNullOrEmpty(shippingoption))
                    throw new Exception("Selected shipping method can't be parsed");
                var splittedOption = shippingoption.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
                if (splittedOption.Length != 2)
                    throw new Exception("Selected shipping method can't be parsed");
                var selectedName = splittedOption[0];
                var shippingRateComputationMethodSystemName = splittedOption[1];

                //find it
                //performance optimization. try cache first
                var shippingOptions = await _genericAttributeService.GetAttributeAsync<List<ShippingOption>>(customer,
                    NopCustomerDefaults.OfferedShippingOptionsAttribute, store.Id);
                if (shippingOptions == null || !shippingOptions.Any())
                {
                    //not found? let's load them using shipping service
                    shippingOptions = (await _shippingService.GetShippingOptionsAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer),
                        customer, shippingRateComputationMethodSystemName, store.Id)).ShippingOptions.ToList();
                }
                else
                {
                    //loaded cached results. let's filter result by a chosen shipping rate computation method
                    shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                }

                var shippingOption = shippingOptions
                    .Find(so => !string.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));
                if (shippingOption == null)
                    throw new Exception("Selected shipping method can't be loaded");

                //save
                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOption, store.Id);

                //load next step
                return await OpcLoadStepAfterShippingMethod(cart);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual async Task<IActionResult> GetPaymentMethod()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
            if (isPaymentWorkflowRequired)
            {
                //filter by country
                var filterByCountryId = 0;
                if (_addressSettings.CountryEnabled)
                {
                    filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId ?? 0;
                }

                //payment is required
                var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);

                var paymentMethodHtml = await RenderPartialViewToStringAsync("_OpcPaymentMethods", paymentMethodModel);
                return Json(new
                {
                    success = true,
                    html = paymentMethodHtml
                });
            }
            return Json(new
            {
                success = false,
                html = string.Empty
            });
        }


        [HttpPost]
        public virtual async Task<IActionResult> OpcSavePaymentMethod(string paymentmethod, CheckoutPaymentMethodModel model)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.Disabled"));

                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                //payment method 
                if (string.IsNullOrEmpty(paymentmethod))
                    throw new Exception("Selected payment method can't be parsed");

                //reward points
                if (_rewardPointsSettings.Enabled)
                {
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute, model.UseRewardPoints,
                        store.Id);
                }

                //Check whether payment workflow is required
                var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
                if (!isPaymentWorkflowRequired)
                {
                    //payment is not required
                    await _genericAttributeService.SaveAttributeAsync<string>(customer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute, null, store.Id);

                    return Json(new
                    {
                        success = true,
                        paymentInfo = false,
                        html = string.Empty
                    });
                }

                var paymentMethodInst = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(paymentmethod, customer, store.Id);
                if (!_paymentPluginManager.IsPluginActive(paymentMethodInst))
                    throw new Exception("Selected payment method can't be parsed");

                //save
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, paymentmethod, store.Id);

                return await OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public async Task<IActionResult> OpcInstantCartView()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, store.Id);

            var model = new ShoppingCartModel();
            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart,
                isEditable: true,
                prepareAndDisplayOrderReviewData: false);

            var instantCartViewHtml = await RenderPartialViewToStringAsync("_InstantCartView", model);
            return Json(new
            {
                success = true,
                html = instantCartViewHtml
            });
        }

        public async Task<IActionResult> OpcInstantConfirmOrder()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, store.Id);

            var model = new ShoppingCartModel();
            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart,
                isEditable: true,
                prepareAndDisplayOrderReviewData: false);

            var instantConfirmOrderHtml = await RenderPartialViewToStringAsync("_InstantConfirmOrder", model);
            return Json(new
            {
                success = true,
                html = instantConfirmOrderHtml
            });
        }

        public async Task<IActionResult> UpdateCart(int id, int selectedQty, bool isUpdateQty)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("Homepage");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            var cartItem = cart.FirstOrDefault(x => x.Id == id);
            if (cartItem != null)
            {
                if (isUpdateQty)
                {
                    cartItem.Quantity = selectedQty > 0 ? selectedQty : 1;

                    var warnings = await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                               cartItem.Id, cartItem.AttributesXml, cartItem.CustomerEnteredPrice,
                               cartItem.RentalStartDateUtc, cartItem.RentalEndDateUtc, cartItem.Quantity, false);

                    if (warnings.Count == 0)
                    {
                        return Json(new
                        {
                            success = true
                        });
                    }
                }
                else
                {
                    var redirectUrl = string.Empty;

                    await _shoppingCartService.DeleteShoppingCartItemAsync(cartItem, false, true);

                    var existCart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                    if (!existCart.Any())
                        redirectUrl = $"{_webHelper.GetStoreLocation().TrimEnd('/')}{Url.RouteUrl("ShoppingCart")}";

                    return Json(new
                    {
                        success = true,
                        redirect = redirectUrl
                    });
                }
            }

            return Json(new
            {
                success = false,
                redirect = ""
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> ApplyDiscountCoupon(string discountcouponcode)
        {
            //trim
            if (discountcouponcode != null)
                discountcouponcode = discountcouponcode.Trim();

            //cart
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            var model = new ShoppingCartModel();
            if (!string.IsNullOrWhiteSpace(discountcouponcode))
            {
                //we find even hidden records here. this way we can display a user-friendly message if it's expired
                var discounts = (await _discountService.GetAllDiscountsAsync(couponCode: discountcouponcode, showHidden: true))
                    .Where(d => d.RequiresCouponCode)
                    .ToList();
                if (discounts.Any())
                {
                    var userErrors = new List<string>();
                    var anyValidDiscount = await discounts.AnyAwaitAsync(async discount =>
                    {
                        var validationResult = await _discountService.ValidateDiscountAsync(discount, customer, new[] { discountcouponcode });
                        userErrors.AddRange(validationResult.Errors);

                        return validationResult.IsValid;
                    });

                    if (anyValidDiscount)
                    {
                        //valid
                        await _customerService.ApplyDiscountCouponCodeAsync(customer, discountcouponcode);
                        model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.ShoppingCart.DiscountCouponCode.Applied"));
                        model.DiscountBox.IsApplied = true;
                    }
                    else
                    {
                        if (userErrors.Any())
                            //some user errors
                            model.DiscountBox.Messages = userErrors;
                        else
                            //general error text
                            model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.ShoppingCart.DiscountCouponCode.WrongDiscount"));
                    }
                }
                else
                    //discount cannot be found
                    model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.ShoppingCart.DiscountCouponCode.CannotBeFound"));
            }
            else
                //empty coupon code
                model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.ShoppingCart.DiscountCouponCode.Empty"));

            return Json(new
            {
                success = true,
                discountBoxModel = model.DiscountBox
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> RemoveDiscountCoupon(int discountId)
        {
            var model = new ShoppingCartModel();

            //get discount identifier

            var discount = await _discountService.GetDiscountByIdAsync(discountId);
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (discount != null)
                await _customerService.RemoveDiscountCouponCodeAsync(customer, discount.CouponCode);

            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> CheckoutAttributeChange(IFormCollection form, bool isEditable)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            //save selected attributes
            await ParseAndSaveCheckoutAttributesAsync(cart, form);
            var attributeXml = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.CheckoutAttributes, store.Id);

            //conditions
            var enabledAttributeIds = new List<int>();
            var disabledAttributeIds = new List<int>();
            var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
            var attributes = await _checkoutAttributeService.GetAllCheckoutAttributesAsync(store.Id, excludeShippableAttributes);
            foreach (var attribute in attributes)
            {
                var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute, attributeXml);
                if (conditionMet.HasValue)
                {
                    if (conditionMet.Value)
                        enabledAttributeIds.Add(attribute.Id);
                    else
                        disabledAttributeIds.Add(attribute.Id);
                }
            }

            //update blocks
            var ordetotalssectionhtml = await RenderViewComponentToStringAsync(typeof(CustomOrderTotalsComponent), new { isEditable });
            var selectedcheckoutattributesssectionhtml = await RenderViewComponentToStringAsync(typeof(SelectedCheckoutAttributesViewComponent));

            return Json(new
            {
                ordetotalssectionhtml,
                selectedcheckoutattributesssectionhtml,
                enabledattributeids = enabledAttributeIds.ToArray(),
                disabledattributeids = disabledAttributeIds.ToArray()
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> ApplyGiftCard(string giftcardcouponcode)
        {
            //trim
            if (giftcardcouponcode != null)
                giftcardcouponcode = giftcardcouponcode.Trim();

            //cart
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            var model = new ShoppingCartModel();
            if (!await _shoppingCartService.ShoppingCartIsRecurringAsync(cart))
            {
                if (!string.IsNullOrWhiteSpace(giftcardcouponcode))
                {
                    var giftCard = (await _giftCardService.GetAllGiftCardsAsync(giftCardCouponCode: giftcardcouponcode)).FirstOrDefault();
                    var isGiftCardValid = giftCard != null && await _giftCardService.IsGiftCardValidAsync(giftCard);
                    if (isGiftCardValid)
                    {
                        await _customerService.ApplyGiftCardCouponCodeAsync(customer, giftcardcouponcode);
                        model.GiftCardBox.Message = await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.ShoppingCart.GiftCardCouponCode.Applied");
                        model.GiftCardBox.IsApplied = true;
                    }
                    else
                    {
                        model.GiftCardBox.Message = await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                        model.GiftCardBox.IsApplied = false;
                    }
                }
                else
                {
                    model.GiftCardBox.Message = await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                    model.GiftCardBox.IsApplied = false;
                }
            }
            else
            {
                model.GiftCardBox.Message = await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");
                model.GiftCardBox.IsApplied = false;
            }

            return Json(new
            {
                success = true,
                giftCartBoxModel = model.GiftCardBox
            });
        }


        public virtual async Task<IActionResult> RemoveGiftCardCode(int giftCardId)
        {
            var model = new ShoppingCartModel();

            var gc = await _giftCardService.GetGiftCardByIdAsync(giftCardId);
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (gc != null)
                await _customerService.RemoveGiftCardCouponCodeAsync(customer, gc.GiftCardCouponCode);

            return Json(new
            {
                success = true
            });
        }



        [HttpPost]
        public virtual async Task<IActionResult> OpcConfirmOrder(IFormCollection form)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.Disabled"));

                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                // save Payment Info
                var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
                var paymentMethod = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(paymentMethodSystemName, customer, store.Id)
                    ?? throw new Exception("Payment method is not selected");

                var warnings = await paymentMethod.ValidatePaymentFormAsync(form);
                foreach (var warning in warnings)
                    ModelState.AddModelError("", warning);
                if (ModelState.IsValid)
                {
                    //get payment info
                    var paymentInfo = await paymentMethod.GetPaymentInfoAsync(form);
                    //set previous order GUID (if exists)
                    _paymentService.GenerateOrderGuid(paymentInfo);

                    //session save
                    HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);
                }
                else
                {
                    var paymentWarningMessage = await RenderPartialViewToStringAsync("_InstantPartialShowWarningMessage", warnings);
                    return Json(new
                    {
                        success = true,
                        paymentWarningMessage = true,
                        html = paymentWarningMessage
                    });
                }


                //prevent 2 orders being placed within an X seconds time frame
                if (!await IsMinimumOrderPlacementIntervalValidAsync(customer))
                    throw new Exception(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.MinOrderPlacementInterval"));

                //place order
                var processPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
                    if (await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart))
                    {
                        throw new Exception("Payment information is not entered");
                    }

                    processPaymentRequest = new ProcessPaymentRequest();
                }
                _paymentService.GenerateOrderGuid(processPaymentRequest);
                processPaymentRequest.StoreId = store.Id;
                processPaymentRequest.CustomerId = customer.Id;
                processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);
                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };

                    var paymentMethodOrderSucess = await _paymentPluginManager
                        .LoadPluginBySystemNameAsync(placeOrderResult.PlacedOrder.PaymentMethodSystemName, customer, store.Id);
                    if (paymentMethodOrderSucess == null)
                        //payment method could be null if order total is 0
                        //success
                        return Json(new { success = 1 });

                    if (paymentMethodOrderSucess.PaymentMethodType == PaymentMethodType.Redirection)
                    {
                        //Redirection will not work because it's AJAX request.
                        //That's why we don't process it here (we redirect a user to another page where he'll be redirected)

                        //redirect
                        return Json(new
                        {
                            redirect = $"{_webHelper.GetStoreLocation()}checkout/OpcCompleteRedirectionPayment"
                        });
                    }

                    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);
                    //success
                    return Json(new { success = 1 });
                }

                //error
                var confirmOrderModel = new CheckoutConfirmModel();
                foreach (var error in placeOrderResult.Errors)
                    confirmOrderModel.Warnings.Add(error);
              
                    return Json(new
                    {
                        success = true,
                        error = confirmOrderModel,
                        message = confirmOrderModel.Warnings
                    });
              
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual async Task<IActionResult> OpcCompleteRedirectionPayment()
        {
            try
            {
                //validation
                if (!_orderSettings.OnePageCheckoutEnabled)
                    return RedirectToRoute("Homepage");

                var customer = await _workContext.GetCurrentCustomerAsync();
                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    return Challenge();

                //get the order
                var store = await _storeContext.GetCurrentStoreAsync();
                var order = (await _orderService.SearchOrdersAsync(storeId: store.Id,
                customerId: customer.Id, pageSize: 1)).FirstOrDefault();
                if (order == null)
                    return RedirectToRoute("Homepage");

                var paymentMethod = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(order.PaymentMethodSystemName, customer, store.Id);
                if (paymentMethod == null)
                    return RedirectToRoute("Homepage");
                if (paymentMethod.PaymentMethodType != PaymentMethodType.Redirection)
                    return RedirectToRoute("Homepage");

                //ensure that order has been just placed
                if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes > 3)
                    return RedirectToRoute("Homepage");

                //Redirection will not work on one page checkout page because it's AJAX request.
                //That's why we process it here
                var postProcessPaymentRequest = new PostProcessPaymentRequest
                {
                    Order = order
                };

                await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

                if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                {
                    //redirection or POST has been done in PostProcessPayment
                    return Content(await _localizationService.GetResourceAsync("Nop.Plugin.InstantOnePageCheckout.Checkout.RedirectMessage"));
                }

                //if no redirection has been done (to a third-party payment page)
                //theoretically it's not possible
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Content(exc.Message);
            }
        }


        #endregion
    }
}
