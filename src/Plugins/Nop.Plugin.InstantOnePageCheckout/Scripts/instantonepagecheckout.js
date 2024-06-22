var OnepageCheckout = {
  loadWaiting: false,
  failureUrl: false,

  init: function (failureUrl) {
    this.loadWaiting = false;
    this.failureUrl = failureUrl;
  },

  ajaxFailure: function () {
    location.href = Checkout.failureUrl;
  },

  setLoadWaiting: function (step, keepDisabled) {
    var container;
    container = $('#' + step);
    var isDisabled = keepDisabled ? true : false;
    if (isDisabled) {
      $(container).LoadingOverlay("show", {
        zIndex: 1010,
        maxSize: 60
      });
    } else {
      $(container).LoadingOverlay("hide");
    }
  },

  ScrollToTop: function () {
    $('html,body').animate({
      scrollTop: $(".validation").offset().top
    },
      'slow');
  },

  showInstantCartView: function () {

    OnepageCheckout.setLoadWaiting("checkout-confirm-cartview-load", true);
    var postData = {
    };
    $.ajax({
      cache: false,
      type: "GET",
      url: "/OnePageCheckout/OpcInstantCartView",
      data: postData,
      dataType: 'json',
      success: function (response) {
        if (response.success == true) {
          $("#checkout-confirm-cartview-load").html('');
          $("#checkout-confirm-cartview-load").html(response.html);
        }
      },
      complete: function () {
        OnepageCheckout.setLoadWaiting("checkout-confirm-cartview-load", false);
      },
      error: OnepageCheckout.ajaxFailure
    });
  },

  showInstantConfirmOrder: function (showMessage = false, message = null, isValid = false, source = null) {

    OnepageCheckout.setLoadWaiting("checkout-confirm-order-load", true);
    var postData = {
    };

    $.ajax({

      cache: false,
      type: "GET",
      url: "/OnePageCheckout/OpcInstantConfirmOrder",
      data: postData,
      dataType: 'json',
      success: function (response) {
        if (response.success == true) {
          $("#checkout-confirm-order-load").html('');
          $("#checkout-confirm-order-load").html(response.html);
        }
      },
      complete: function () {
        if (source == "giftcard" && showMessage == true && message != null) {
          var addclass = isValid ? "message-success" : "message-failure";
          $("#showgiftcardmessage").html("");
          $("#showgiftcardmessage").html("<div class=" + addclass + ">" + message + "</div>");
        }
        if (source == "discount" && showMessage == true && message != null) {
          var addclass = isValid ? "message-success" : "message-failure";
          $("#showdiscountmessage").html("");
          $("#showdiscountmessage").html("<div class=" + addclass + ">" + message + "</div>");
        }
        OnepageCheckout.setLoadWaiting("checkout-confirm-order-load", false);
      },
      error: OnepageCheckout.ajaxFailure
    });

  },

  updateCart: function (id, selectedQty, isUpdateQty) {
    var postData = {
      id: id,
      selectedQty: selectedQty,
      isUpdateQty: isUpdateQty
    };

    $.ajax({
      cache: false,
      type: "POST",
      url: "/OnePageCheckout/UpdateCart",
      data: postData,
      dataType: 'json',
      success: function (response) {
        if (response.success == true) {
          if (response.redirect != "" && response.redirect != null) {
            location.href = response.redirect;
          }
          OnepageCheckout.showInstantCartView();
          OnepageCheckout.showInstantConfirmOrder();
        }
      },
      complete: function () {
      },
      error: OnepageCheckout.ajaxFailure
    });

  },

  applyDiscountCouponCode: function (discountcouponcode) {

    var postData = {
      discountcouponcode: discountcouponcode
    };

    $.ajax({
      cache: false,
      type: "POST",
      url: "/OnePageCheckout/ApplyDiscountCoupon",
      data: postData,
      dataType: 'json',
      success: function (response) {
        if (response.success == true) {
          console.log(response.discountBoxModel.Messages[0]);
          console.log(response.discountBoxModel.IsApplied);
          console.log(response.discountBoxModel.Messages != undefined && response.discountBoxModel.Messages != null && response.discountBoxModel.Messages.length > 0);
          if (response.discountBoxModel.Messages != undefined && response.discountBoxModel.Messages != null && response.discountBoxModel.Messages.length > 0) {
            var message = response.discountBoxModel.Messages[0];
            var isApplied = response.discountBoxModel.IsApplied;
            OnepageCheckout.showInstantConfirmOrder(true, message, isApplied, "discount");
          } else {
            OnepageCheckout.showInstantConfirmOrder();
          }
        }
      },
      complete: function () {
      },
      error: OnepageCheckout.ajaxFailure
    });

  },

  removeDiscount: function (discountId) {

    var postData = {
      discountId: discountId
    };

    $.ajax({
      cache: false,
      type: "POST",
      url: "/OnePageCheckout/RemoveDiscountCoupon",
      data: postData,
      dataType: 'json',
      success: function (response) {
        if (response.success == true) {
          OnepageCheckout.showInstantConfirmOrder();
        }
      },
      complete: function () {
      },
      error: OnepageCheckout.ajaxFailure
    });

  },

  applyGiftCardCouponCode: function (giftcardcouponcode) {

    var postData = {
      giftcardcouponcode: giftcardcouponcode
    };

    $.ajax({
      cache: false,
      type: "POST",
      url: "/OnePageCheckout/ApplyGiftCard",
      data: postData,
      dataType: 'json',
      success: function (response) {
        if (response.success == true) {
          if (response.giftCartBoxModel.Message != undefined && response.giftCartBoxModel.Message != null) {
            var message = response.giftCartBoxModel.Message;
            var isApplied = response.giftCartBoxModel.IsApplied;
            OnepageCheckout.showInstantConfirmOrder(true, message, isApplied, "giftcard");
          } else {
            OnepageCheckout.showInstantConfirmOrder();
          }
        }
      },
      complete: function () {
      },
      error: OnepageCheckout.ajaxFailure
    });

  },

  removeGiftCart: function (giftCardId) {

    var postData = {
      giftCardId: giftCardId
    };

    $.ajax({
      cache: false,
      type: "POST",
      url: "/OnePageCheckout/RemoveGiftCardCode",
      data: postData,
      dataType: 'json',
      success: function (response) {
        if (response.success == true) {
          OnepageCheckout.showInstantConfirmOrder();
        }
      },
      complete: function () {
      },
      error: OnepageCheckout.ajaxFailure
    });

  },

};

var Billing = {
  form: false,
  saveUrl: false,
  disableBillingAddressCheckoutStep: false,
  guest: false,
  selectedStateId: 0,
  updateBillingAddressUrl: false,

  init: function (form, saveUrl, disableBillingAddressCheckoutStep, guest, updateBillingAddressUrl) {
    this.form = form;
    this.saveUrl = saveUrl;
    this.disableBillingAddressCheckoutStep = disableBillingAddressCheckoutStep;
    this.guest = guest;
    this.updateBillingAddressUrl = updateBillingAddressUrl;
  },

  getBillingAddress: function () {

    var postData = {
    };

    $.ajax({
      cache: false,
      type: "GET",
      url: "/OnePageCheckout/GetBillingAddressList",
      data: postData,
      dataType: 'json',
      success: function (response) {
        if (response.success == true) {
          $("#divpartialbillingaddresslist").html('');
          $("#divpartialbillingaddresslist").html(response.html);
          //Shipping.initializeShippingAddressRadioChange();
          Billing.initializeBillingAddressRadioChange();
        }
      },
      complete: function () {        
      },
      error: OnepageCheckout.ajaxFailure
    });

  },

  showNewAddress: function () {

    var postData = {};

    $.ajax({
      cache: false,
      type: "POST",
      url: "/OnePageCheckout/NewBillingAddress",
      data: postData,
      dataType: 'json',
      success: function (response) {
        if (response.success == true) {
          $("#checkout-step-billing").html('');
          $("#checkout-step-billing").html(response.html);
        }
      },
      complete: function () {
        //$('#billingaddressdialog').dialog('open');
        $('#save-edit-address-button').hide();
        Billing.initializeCountrySelect();
        commonOnePageCheckOut.billingFormControlvalidation();
      },
      error: OnepageCheckout.ajaxFailure
    });

  },

  initializeCountrySelect: function () {
    if ($('#checkout-billing-load').has('select[data-trigger="country-select"]')) {
      $('#checkout-billing-load select[data-trigger="country-select"]').countrySelect();
    }
  },

  save: function () {

    var flag = false;
    if (this.disableBillingAddressCheckoutStep == false) {
      $('div.billingvalid').remove();
      var flag = commonOnePageCheckOut.checkBillingFormValidation();
    } else {
      flag = true;
    }

    //$('div.billingvalid').remove();
    //var flag = commonOnePageCheckOut.checkBillingFormValidation();

    if (flag == true) {
      OnepageCheckout.setLoadWaiting("checkout-billing-load", true);

      $.ajax({
        cache: false,
        url: this.saveUrl,
        data: $(this.form).serialize(),
        type: "POST",
        success: function (response) {
          if (response.success == true) {
            $("#billingaddressdialog").dialog('close');
            OnepageCheckout.setLoadWaiting("checkout-billing-load", false);
            $("#divpartialbillingaddresslist").html('');
            $("#divpartialbillingaddresslist").html(response.html);
            if (response.billingAddressId > 0) {
              $('.hli').toggleClass('hli');
              var divid = 'div#' + response.billingAddressId + '.big'.trim();
              if ($("#divpartialbillingaddresslist").find(divid)) {
                $(divid).toggleClass('hli');
              } else {
                if ($('.big').eq(0).length == 1) {
                  $('.big').eq(0).toggleClass('hli');
                }
              }
            } else {
              if ($('.big').eq(0).length == 1) {
                $('.big').eq(0).toggleClass('hli');
              }
            }
            Billing.initializeBillingAddressRadioChange();

          }
          else {
            $(".enter-address").prepend("<div class='billingvalid'>" + response.message + "</div>");
            OnepageCheckout.setLoadWaiting("checkout-billing-load", false);
          }
        },
        complete: function () {
          //if (this.disableBillingAddressCheckoutStep == false) {
          Shipping.getShippingAddress();
          //}
        },
        error: OnepageCheckout.ajaxFailure
      });
    }
  },

  updateCustomerBillingAddress: function (billingAddressId) {

    $.ajax({
      cache: false,
      url: this.updateBillingAddressUrl,
      data: {
        "billingAddressId": billingAddressId
      },
      type: "POST",
      success: function (response) {
        if (response.success == true) {
          if (response.billingAddressId > 0) {
            $('.hli').toggleClass('hli');
            var divid = 'div#' + response.billingAddressId + '.big'.trim();
            if ($("#divpartialbillingaddresslist").find(divid)) {
              $(divid).toggleClass('hli');
            } else {
              if ($('.big').eq(0).length == 1) {
                $('.big').eq(0).toggleClass('hli');
              }
            }
          } else {
            if ($('.big').eq(0).length == 1) {
              $('.big').eq(0).toggleClass('hli');
            }
          }
        }
      },
      complete: function () {
        //OnepageCheckout.showInstantConfirmOrder();
        Shipping.getShippingAddress();
      },
      error: OnepageCheckout.ajaxFailure
    });
  },

  deleteAddress: function (url, selectedAddress) {

    $.ajax({
      cache: false,
      type: "GET",
      url: url,
      data: {
        "addressId": selectedAddress
      },
      success: function (response) {
        if (response.success == true) {
          $("#divpartialbillingaddresslist").html('');
          $("#divpartialbillingaddresslist").html(response.html);
          if (response.billingAddressId > 0) {
            $('.hli').toggleClass('hli');
            var divid = 'div#' + response.billingAddressId + '.big'.trim();
            if ($("#divpartialbillingaddresslist").find(divid)) {
              $(divid).toggleClass('hli');
            } else {
              if ($('.big').eq(0).length == 1) {
                $('.big').eq(0).toggleClass('hli');
              }
            }
          } else {
            if ($('.big').eq(0).length == 1) {
              $('.big').eq(0).toggleClass('hli');
            }
          }
        }
      },
      complete: function () {
        $("#dialog-confirm").dialog('close');
        Shipping.getShippingAddress();
      },
      error: OnepageCheckout.ajaxFailure
    });
  },

  editAddress: function (url, selectedAddressId) {
    Billing.resetBillingForm();

    var prefix = 'BillingNewAddress_';
    $.ajax({
      cache: false,
      type: "GET",
      url: url,
      data: {
        "addressId": selectedAddressId
      },
      success: function (data, textStatus, jqXHR) {
        $.each(data,
          function (id, value) {
            if (value !== null) {
              var val = $(`#${prefix}${id}`).val(value);
              if (id.indexOf('CountryId') >= 0) {
                val.trigger('change');
              }
              if (id.indexOf('StateProvinceId') >= 0) {
                Billing.setSelectedStateId(value);
              }
            }
          });
      },
      complete: function (jqXHR, textStatus) {
        $('#billing-new-address-form').show();
        $('#save-address-button').hide();
        $('#save-edit-address-button').show();
        $('#billingaddressdialog').dialog('open');
      },
      error: OnepageCheckout.ajaxFailure
    });
  },

  setSelectedStateId: function (id) {
    this.selectedStateId = id;
  },

  ShowAddressDialog: function () {
    Billing.showNewAddress();
    $('#save-address-button').show();
    $('#save-edit-address-button').hide();
    $('#billingaddressdialog').dialog('open');
    $('#BillingNewAddress_FirstName').blur();
    $('div.billingvalid').remove();
  },

  saveEditAddress: function (url) {
    var selectedId;
    OnepageCheckout.setLoadWaiting("checkout-billing-load", true);
    $.ajax({
      cache: false,
      url: url,
      data: $(this.form).serialize(),
      type: "POST",
      success: function (response) {
        selectedId = response.selected_id;
        if (response.success == true) {
          $("#billingaddressdialog").dialog('close');
          OnepageCheckout.setLoadWaiting("checkout-billing-load", false);
          $("#divpartialbillingaddresslist").html('');
          $("#divpartialbillingaddresslist").html(response.html);
        }
        Billing.resetBillingForm();
      },
      complete: function () {
        if (selectedId > 0) {
          $('.hli').toggleClass('hli');
          var divid = 'div#' + selectedId + '.big'.trim();
          if ($("#divpartialbillingaddresslist").find(divid)) {
            $(divid).toggleClass('hli');
          } else {
            if ($('.big').eq(0).length == 1) {
              $('.big').eq(0).toggleClass('hli');
            }
          }
        } else {
          if ($('.big').eq(0).length == 1) {
            $('.big').eq(0).toggleClass('hli');
          }
        }
        OnepageCheckout.showInstantConfirmOrder();
        Shipping.getShippingAddress();
      },
      error: OnepageCheckout.ajaxFailure
    });
  },

  resetBillingForm: function () {
    $(':input', '#billing-new-address-form')
      .not(':button, :submit, :reset, :hidden')
      .removeAttr('checked').removeAttr('selected')
    $(':input', '#billing-new-address-form')
      .not(':checkbox, :radio, select')
      .val('');

    $('.address-id', '#billing-new-address-form').val('0');
    $('select option[value="0"]', '#billing-new-address-form').prop('selected', true).trigger('change');
  },

  initializeBillingAddressRadioChange: function () {

    $('.big').click(function () {
      $('.hli').toggleClass('hli');
      $(this).toggleClass('hli');
      var billingAddressId = $(this).attr('id');
      Billing.updateCustomerBillingAddress(billingAddressId);
    });
  }
};

var Shipping = {
  form: false,
  pickupform: false,
  saveUrl: false,
  updateShippingAddressUrl: false,

  init: function (form, pickupform, saveUrl, updateShippingAddressUrl) {
    this.form = form;
    this.pickupform = pickupform;
    this.saveUrl = saveUrl;
    this.updateShippingAddressUrl = updateShippingAddressUrl
  },

  showNewAddress: function () {

    var postData = {
    };

    $.ajax({
      cache: false,
      type: "POST",
      url: "/OnePageCheckout/NewShippingAddress",
      data: postData,
      dataType: 'json',
      success: function (response) {
        if (response.success == true) {
          $("#checkout-step-shippingaddress").html('');
          $("#checkout-step-shippingaddress").html(response.html);
          Shipping.initializeCountrySelect();
          $('#shippingaddressdialog').dialog('open');
          $('#ShippingNewAddress_FirstName').blur();
          $('div.shippingvalid').remove();
        }
      },
      complete: function () {
        commonOnePageCheckOut.shippingFormControlvalidation();
      },
      error: OnepageCheckout.ajaxFailure
    });

  },

  newAddress: function (isNew) {
    if (isNew) {
      this.resetSelectedAddress();
      $('#shipping-new-address-form').show();
    } else {
      $('#shipping-new-address-form').hide();
    }
    $(document).trigger({ type: "onepagecheckout_shipping_address_new" });
    Shipping.initializeCountrySelect();
  },

  resetSelectedAddress: function () {
    var selectElement = $('#shipping-address-select');
    if (selectElement) {
      selectElement.val('');
    }
    $(document).trigger({ type: "onepagecheckout_shipping_address_reset" });
  },

  save: function (isValid) {

    $('div.shippingvalid').remove();

    var flag = true;
    if (isValid)
      flag = commonOnePageCheckOut.checkShippingFormValidation();

    if (flag == true) {
      OnepageCheckout.setLoadWaiting("checkout-step-shippingaddress", true);

      $.ajax({
        cache: false,
        url: this.saveUrl,
        data: isValid == true ? $(this.form).serialize() : $(this.pickupform).serialize(),
        type: "POST",
        success: function (response) {
          if (response.success == true) {
            $("#shippingaddressdialog").dialog('close');
            OnepageCheckout.setLoadWaiting("checkout-step-shippingaddress", false);
            $("#divpartialshippingaddresslist").html('');
            $("#divpartialshippingaddresslist").html(response.html);
            Shipping.initializeShippingAddressRadioChange();
          }
        },
        complete: function () {          
          OnepageCheckout.showInstantConfirmOrder();
        },
        error: OnepageCheckout.ajaxFailure
      });
    }
  },

  updateCustomerShippingAddress: function (shippingAddressId) {
    $.ajax({
      cache: false,
      url: this.updateShippingAddressUrl,
      data: {
        "shippingAddressId": shippingAddressId
      },
      type: "POST",
      success: function (response) {
        if (response.success == true) {
          //if (response.billingAddressId > 0) {
          //  $('.hli').toggleClass('hli');
          //  var divid = 'div#' + response.billingAddressId + '.big'.trim();
          //  if ($("#divpartialbillingaddresslist").find(divid)) {
          //    $(divid).toggleClass('hli');
          //  } else {
          //    if ($('.big').eq(0).length == 1) {
          //      $('.big').eq(0).toggleClass('hli');
          //    }
          //  }
          //} else {
          //  if ($('.big').eq(0).length == 1) {
          //    $('.big').eq(0).toggleClass('hli');
          //  }
          //}
        }
      },
      complete: function () {
        OnepageCheckout.showInstantConfirmOrder();
      },
      error: OnepageCheckout.ajaxFailure
    });
  },

  getShippingAddress: function () {

    var postData = {
    };

    $.ajax({
      cache: false,
      type: "GET",
      url: "/OnePageCheckout/GetShippingAddressList",
      data: postData,
      dataType: 'json',
      success: function (response) {
        if (response.success == true) {
          $("#divpartialshippingaddresslist").html('');
          $("#divpartialshippingaddresslist").html(response.html);
          Shipping.initializeShippingAddressRadioChange();
        }
      },
      complete: function () {        
      },
      error: OnepageCheckout.ajaxFailure
    });

  },

  shippingAddresschange: function (shippingchange) {
    if (shippingchange.checked == true) {
      $("#divShippingAddress").hide();
      $("#divpartialshippingaddresslist").html('');
      OnepageCheckout.showInstantConfirmOrder();
      var billingId = $("#divpartialbillingaddresslist").find('.hli').attr('id');
      if (billingId != null && billingId > 0 && billingId != undefined) {
        Shipping.updateCustomerShippingAddress(billingId);
      }
    } else {
      $("#divShippingAddress").show();
      Shipping.getShippingAddress();
    }
  },

  initializeCountrySelect: function () {
    if ($('#checkout-shipping-load').has('select[data-trigger="country-select"]')) {
      $('#checkout-shipping-load select[data-trigger="country-select"]').countrySelect();
    }
  },

  initializeShippingAddressRadioChange: function () {

    $('.shippingradio').click(function () {
      $('.shippingborder').toggleClass('shippingborder');
      $(this).toggleClass('shippingborder');
      var shippingAddressId = $(this).attr('id');
      Shipping.updateCustomerShippingAddress(shippingAddressId);
    });
  }

};

var ShippingMethod = {
  form: false,
  saveUrl: false,
  localized_data: false,

  init: function (form, saveUrl, localized_data) {
    this.form = form;
    this.saveUrl = saveUrl;
    this.localized_data = localized_data;
  },

  getShippingMethod: function () {

    OnepageCheckout.setLoadWaiting("checkout-shipping-method-load", true);
    var postData = {
    };

    $.ajax({
      cache: false,
      type: "GET",
      url: "/OnePageCheckout/GetShippingMethod",
      data: postData,
      dataType: 'json',
      success: function (response) {
        if (response.success == true) {
          $("#checkout-shipping-method-load").html('');
          $("#checkout-shipping-method-load").html(response.html);
          $("#checkout-step-shipping-method").show();
        } else {
          $("#checkout-shipping-method-load").html('');
          $("#checkout-step-shipping-method").hide();
        }
      },
      complete: function () {
        var id = $('input[type=radio][name=shippingoption]:checked').attr('id');
        if (id != null && id != undefined && id != '') {
          $("#" + id).prop("checked", true).trigger("change");
          ShippingMethod.save();
        }
        OnepageCheckout.setLoadWaiting("checkout-shipping-method-load", false);
      },
      error: OnepageCheckout.ajaxFailure
    });

  },

  validate: function () {
    var methods = document.getElementsByName('shippingoption');
    if (methods.length === 0) {
      $(".page-body.checkout-data").prepend("<div class='validation'>" + this.localized_data.NotAvailableMethodsError + "</div>");
      OnepageCheckout.ScrollToTop();
      return false;
    }

    for (var i = 0; i < methods.length; i++) {
      if (methods[i].checked) {
        return true;
      }
    }
    $(".page-body.checkout-data").prepend("<div class='validation'>" + this.localized_data.SpecifyMethodError + "</div>");
    OnepageCheckout.ScrollToTop();
    return false;
  },

  save: function () {

    if (this.validate()) {

      $.ajax({
        cache: false,
        url: this.saveUrl,
        data: $(this.form).serialize(),
        type: "POST",
        success: function (response) {
          if (response.success == true) {
            if (response.paymentMethod == true) {
              $("#divPaymentMethod").show();
              $("#checkout-step-payment-method").show();
              $("#checkout-payment-method-load").html('');
              $("#checkout-payment-method-load").html(response.html);
            } else {
              $("#divPaymentMethod").hide();
              $("#checkout-step-payment-method").hide();
              $("#checkout-payment-method-load").html('');
              OnepageCheckout.showInstantConfirmOrder();
            }
          }
        },
        complete: function () {
          PaymentMethod.getPaymentMethod();
          Billing.getBillingAddress();
        },
        error: OnepageCheckout.ajaxFailure
      });
    }
  },
};
var PaymentMethod = {
  form: false,
  saveUrl: false,
  localized_data: false,

  init: function (form, saveUrl, localized_data) {
    this.form = form;
    this.saveUrl = saveUrl;
    this.localized_data = localized_data;
  },

  toggleUseRewardPoints: function (useRewardPointsInput) {
    if (useRewardPointsInput.checked) {
      $('#payment-method-block').hide();
    }
    else {
      $('#payment-method-block').show();
    }
  },

  getPaymentMethod: function () {

    OnepageCheckout.setLoadWaiting("checkout-payment-method-load", true);
    var postData = {
    };

    $.ajax({
      cache: false,
      type: "GET",
      url: "/OnePageCheckout/GetPaymentMethod",
      data: postData,
      dataType: 'json',
      success: function (response) {
        if (response.success == true) {
          $("#checkout-step-payment-method").show();
          $("#checkout-payment-method-load").html('');
          $("#checkout-payment-method-load").html(response.html);
          OnepageCheckout.showInstantConfirmOrder();
        } else {
          $("#checkout-payment-method-load").html('');
          $("#checkout-step-payment-method").hide();
        }
      },
      complete: function () {
        OnepageCheckout.setLoadWaiting("checkout-payment-method-load", false);
      },
      error: OnepageCheckout.ajaxFailure
    });

  },

  validate: function () {
    var methods = document.getElementsByName('paymentmethod');
    if (methods.length === 0) {
      $(".page-body.checkout-data").prepend("<div class='validation'>" + this.localized_data.NotAvailableMethodsError + "</div>");
      OnepageCheckout.ScrollToTop();
      return false;
    }

    for (var i = 0; i < methods.length; i++) {
      if (methods[i].checked) {
        return true;
      }
    }
    $(".page-body.checkout-data").prepend("<div class='validation'>" + this.localized_data.SpecifyMethodError + "</div>");
    OnepageCheckout.ScrollToTop();
    return false;
  },

  save: function () {

    if (this.validate()) {

      OnepageCheckout.setLoadWaiting("checkout-payment-info-load", true);
      $.ajax({
        cache: false,
        url: this.saveUrl,
        data: $(this.form).serialize(),
        type: "POST",
        success: function (response) {
          if (response.success == true) {
            if (response.paymentInfo == true) {
              $("#divPaymentInfoMethod").show();
              $("#checkout-step-payment-info").show();
              $("#checkout-payment-info-load").html('');
              $("#checkout-payment-info-load").html(response.html);
            } else {
              $("#divPaymentInfoMethod").hide();
              $("#checkout-step-payment-info").hide();
              $("#checkout-payment-info-load").html('');
              OnepageCheckout.showInstantCartView();
            }
          }
        },
        complete: function () {
          OnepageCheckout.showInstantConfirmOrder();
          OnepageCheckout.setLoadWaiting("checkout-payment-info-load", false);
        },
        error: OnepageCheckout.ajaxFailure
      });
    }
  },
};

var ConfirmOrder = {
  form: false,
  saveUrl: false,
  isSuccess: false,

  init: function (saveUrl, successUrl, form) {
    this.saveUrl = saveUrl;
    this.successUrl = successUrl;
    this.form = form
  },

  save: function () {

    $("body").LoadingOverlay("show");

    $.ajax({
      cache: false,
      url: this.saveUrl,
      data: $(this.form).serialize(),
      type: "POST",
      success: function (response) {
        if (response.success == true) {
          if (response.paymentWarningMessage == true) {
            $("#showWarningMessage").html('');
            $("#showWarningMessage").html(response.html);
            return false;
          }
        }
        $("body").LoadingOverlay("hide");
        ConfirmOrder.nextStep(response);
      },
      complete: function () {

      },
      error: OnepageCheckout.ajaxFailure
    });
  },

  nextStep: function (response) {
    var val = $(".validation").text();
    if (response.error) {
      if (val == response.message) {
        $(".validation").remove();
        if (typeof response.message === 'string') {
          $(".page-body.checkout-data").prepend("<div class='validation'>" + response.message + "</div>");
        }
      }
      $(".page-body.checkout-data").prepend("<div class='validation'>" + response.message.join("\n") + "</div>");
      OnepageCheckout.ScrollToTop();
      return false;
    }

    if (response.redirect) {
      ConfirmOrder.isSuccess = true;
      location.href = response.redirect;
      return;
    }
    if (response.success) {
      ConfirmOrder.isSuccess = true;
      window.location = ConfirmOrder.successUrl;
    }
  }
};
