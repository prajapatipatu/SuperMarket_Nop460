
var commonOnePageCheckOut = {

  firstNameMessage: null,
  lastNameMessage: null,
  emailMessage: null,
  companyMessage: null,
  cityMessage: null,
  address1Message: null,
  zipPostalCodeMessage: null,
  phoneNumberMessage: null,
  countryMessage: null,
  companyRequired: null,
  stateprovince: null,

  init: function (firstNameMessage, lastNameMessage, emailMessage, companyMessage, cityMessage, address1Message, zipPostalCodeMessage, phoneNumberMessage, countryMessage, companyRequiredValid, stateprovince) {
    this.firstNameMessage = firstNameMessage;
    this.lastNameMessage = lastNameMessage;
    this.emailMessage = emailMessage;
    this.companyMessage = companyMessage;
    this.cityMessage = cityMessage;
    this.address1Message = address1Message;
    this.zipPostalCodeMessage = zipPostalCodeMessage;
    this.phoneNumberMessage = phoneNumberMessage;
    this.countryMessage = countryMessage;
    this.companyRequired = companyRequiredValid;
    this.stateprovince = stateprovince;
  },

  billingFormControlvalidation: function () {

    $("#BillingNewAddress_FirstName").blur(function () {
      billingControlValid(this, commonOnePageCheckOut.firstNameMessage);
    });

    $("#BillingNewAddress_LastName").blur(function () {
      billingControlValid(this, commonOnePageCheckOut.lastNameMessage);
    });

    $("#BillingNewAddress_Email").blur(function () {
      billingControlValid(this, commonOnePageCheckOut.emailMessage);
    });

    if (commonOnePageCheckOut.companyRequired == true) {
      $("#BillingNewAddress_Company").blur(function () {
        billingControlValid(this, commonOnePageCheckOut.companyMessage);
      });
    }

    $("#BillingNewAddress_City").blur(function () {
      billingControlValid(this, commonOnePageCheckOut.cityMessage);
    });

    $("#BillingNewAddress_Address1").blur(function () {
      billingControlValid(this, commonOnePageCheckOut.address1Message);
    });

    $("#BillingNewAddress_ZipPostalCode").blur(function () {
      billingControlValid(this, commonOnePageCheckOut.zipPostalCodeMessage);
    });

    $("#BillingNewAddress_PhoneNumber").blur(function () {
      billingControlValid(this, commonOnePageCheckOut.phoneNumberMessage);
    });

  },

  checkBillingFormValidation: function () {

    var flag = true;
    var validCount = 0;
    if (commonOnePageCheckOut.companyRequired == true) {
      var controlList = [
        { "id": "BillingNewAddress_FirstName", "message": commonOnePageCheckOut.firstNameMessage },
        { "id": "BillingNewAddress_LastName", "message": commonOnePageCheckOut.lastNameMessage },
        { "id": "BillingNewAddress_Email", "message": commonOnePageCheckOut.emailMessage },
        { "id": "BillingNewAddress_Company", "message": commonOnePageCheckOut.companyMessage },
        { "id": "BillingNewAddress_City", "message": commonOnePageCheckOut.cityMessage },
        { "id": "BillingNewAddress_Address1", "message": commonOnePageCheckOut.address1Message },
        { "id": "BillingNewAddress_ZipPostalCode", "message": commonOnePageCheckOut.zipPostalCodeMessage },
        { "id": "BillingNewAddress_PhoneNumber", "message": commonOnePageCheckOut.phoneNumberMessage }
      ];
    } else {
      var controlList = [
        { "id": "BillingNewAddress_FirstName", "message": commonOnePageCheckOut.firstNameMessage },
        { "id": "BillingNewAddress_LastName", "message": commonOnePageCheckOut.lastNameMessage },
        { "id": "BillingNewAddress_Email", "message": commonOnePageCheckOut.emailMessage },
        { "id": "BillingNewAddress_City", "message": commonOnePageCheckOut.cityMessage },
        { "id": "BillingNewAddress_Address1", "message": commonOnePageCheckOut.address1Message },
        { "id": "BillingNewAddress_ZipPostalCode", "message": commonOnePageCheckOut.zipPostalCodeMessage },
        { "id": "BillingNewAddress_PhoneNumber", "message": commonOnePageCheckOut.phoneNumberMessage }
      ];
    }
    for (i = 0; i < controlList.length; ++i) {
      var valid = billingFormvalid(controlList[i].id, controlList[i].message);
      if (!valid) {
        validCount++;
      }
    }

    // BillingNewAddress_CountryId
    var country = $("#BillingNewAddress_CountryId").val();
    if (country == 0 || country == "" || country == null) {
      $('#BillingNewAddress_CountryId').next('span').next('div.billingvalid').remove();
      $('#BillingNewAddress_CountryId').next('span').after('<div class="billingvalid">' + commonOnePageCheckOut.countryMessage + '</div>');
      validCount++;
    } else {
      $('#BillingNewAddress_CountryId').next('span').next('div.billingvalid').remove();
    }

    // BillingNewAddress_StateProvienceId
    var state = $("#BillingNewAddress_StateProvinceId").val();
    if (state == 0 || state == "" || state == null) {
      var statetext = $("#BillingNewAddress_StateProvinceId option:selected").text();
      if (statetext == "Other") {
        $('#BillingNewAddress_StateProvinceId').next('span').next('div.billingvalid').remove();
      } else {
        $('#BillingNewAddress_StateProvinceId').next('span').next('div.billingvalid').remove();
        $('#BillingNewAddress_StateProvinceId').next('span').after('<div class="billingvalid">' + commonOnePageCheckOut.stateprovince + '</div>');
        validCount++;
      }
    }
    else {
      $('#BillingNewAddress_StateProvinceId').next('span').next('div.billingvalid').remove();
    }

    if (validCount > 0) {
      flag = false;
    }

    return flag;

  },

  shippingFormControlvalidation: function () {

    $("#ShippingNewAddress_FirstName").blur(function () {
      shippingControlValid(this, commonOnePageCheckOut.firstNameMessage);
    });

    $("#ShippingNewAddress_LastName").blur(function () {
      shippingControlValid(this, commonOnePageCheckOut.lastNameMessage);
    });

    $("#ShippingNewAddress_Email").blur(function () {
      shippingControlValid(this, commonOnePageCheckOut.emailMessage);
    });

    $("#ShippingNewAddress_City").blur(function () {
      shippingControlValid(this, commonOnePageCheckOut.cityMessage);
    });
    $("#ShippingNewAddress_Address1").blur(function () {
      shippingControlValid(this, commonOnePageCheckOut.address1Message);
    });
    $("#ShippingNewAddress_ZipPostalCode").blur(function () {
      shippingControlValid(this, commonOnePageCheckOut.zipPostalCodeMessage);
    });
    $("#ShippingNewAddress_PhoneNumber").blur(function () {
      shippingControlValid(this, commonOnePageCheckOut.phoneNumberMessage);
    });


  },

  checkShippingFormValidation: function () {
    var flag = true;
    var validCount = 0;
    var shippingControlList = [
      { "id": "ShippingNewAddress_FirstName", "message": commonOnePageCheckOut.firstNameMessage },
      { "id": "ShippingNewAddress_LastName", "message": commonOnePageCheckOut.lastNameMessage },
      { "id": "ShippingNewAddress_Email", "message": commonOnePageCheckOut.emailMessage },
      { "id": "ShippingNewAddress_City", "message": commonOnePageCheckOut.cityMessage },
      { "id": "ShippingNewAddress_Address1", "message": commonOnePageCheckOut.address1Message },
      { "id": "ShippingNewAddress_ZipPostalCode", "message": commonOnePageCheckOut.zipPostalCodeMessage },
      { "id": "ShippingNewAddress_PhoneNumber", "message": commonOnePageCheckOut.phoneNumberMessage },
    ];

    for (i = 0; i < shippingControlList.length; ++i) {
      var valid = shippingFormvalid(shippingControlList[i].id, shippingControlList[i].message);
      if (!valid) {
        validCount++;
      }
    }

    // ShippingNewAddress_CountryId
    var country = $("#ShippingNewAddress_CountryId").val();
    if (country == 0 || country == "" || country == null) {
      $('#ShippingNewAddress_CountryId').next('span').next('div.shippingvalid').remove();
      $('#ShippingNewAddress_CountryId').next('span').after('<div class="shippingvalid">' + commonOnePageCheckOut.countryMessage + '</div>');
      validCount++;
    } else {
      $('#ShippingNewAddress_CountryId').next('span').next('div.shippingvalid').remove();
    }

    // ShippingNewAddress_StateProvienceId
    var state = $("#ShippingNewAddress_StateProvinceId").val();
    if (state == 0 || state == "" || state == null) {
      var statetext = $("#ShippingNewAddress_StateProvinceId option:selected").text();
      if (statetext == "Other") {
        $('#ShippingNewAddress_StateProvinceId').next('span').next('div.billingvalid').remove();
      } else {
        $('#ShippingNewAddress_StateProvinceId').next('span').next('div.billingvalid').remove();
        $('#ShippingNewAddress_StateProvinceId').next('span').after('<div class="billingvalid">' + commonOnePageCheckOut.stateprovince + '</div>');
        validCount++;
      }
    }
    else {
      $('#ShippingNewAddress_StateProvinceId').next('span').next('div.billingvalid').remove();
    }
    if (validCount > 0) {
      flag = false;
    }

    return flag;
  },
}

//#region Billing Form Validation

function billingControlValid(control, message) {
  if ($(control).val().length == 0) {
    $(control).next('span').next('div.billingvalid').remove();
    $(control).next('span').after('<div class="billingvalid">' + message + '</div>');
  } else {
    $(control).next('span').next('div.billingvalid').remove();
  }
}


function billingFormvalid(control, message) {
  control = "#" + control;
  if ($(control).val().length == 0) {
    $(control).next('span').next('div.billingvalid').remove();
    $(control).next('span').after('<div class="billingvalid">' + message + '</div>');
    return false;
  } else {
    $(control).next('span').next('div.billingvalid').remove();
    return true;
  }
}

//#endregion Billing Form Validation

//#region Shipping Form Validation


function shippingControlValid(control, message) {
  if ($(control).val().length == 0) {
    $(control).next('span').next('div.shippingvalid').remove();
    $(control).next('span').after('<div class="shippingvalid">' + message + '</div>');
  } else {
    $(control).next('span').next('div.shippingvalid').remove();
  }
}

function shippingFormvalid(control, message) {
  control = "#" + control;
  if ($(control).val().length == 0) {
    $(control).next('span').next('div.shippingvalid').remove();
    $(control).next('span').after('<div class="shippingvalid">' + message + '</div>');
    return false;
  } else {
    $(control).next('span').next('div.shippingvalid').remove();
    return true;
  }
}

//#endregion Shipping Form Validation