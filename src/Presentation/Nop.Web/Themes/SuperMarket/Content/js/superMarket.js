var superMarket = {

  form: false,
  saveUrl: false,

  // Public Methods
  init: function (form, saveUrl) {
    this.form = form;
    this.saveUrl = saveUrl;

    this.saveshippingmethod(form, saveUrl);

    $('input[type=radio][name=shippingoption]').change(function () {
      superMarket.saveshippingmethod(form, saveUrl);
    });

  },

  saveshippingmethod: function (form, saveUrl) {
      $.ajax({
        cache: false,
        url: saveUrl,
        data: $(form).serialize(),
        type: "POST",
        success: function (data) {
        },
        complete: function (data) {
          superMarket.getOrderSummaryHtml();
        },
      });
  },

  getOrderSummaryHtml: function() {
    $.ajax({
      cache: false,
      type: "GET",
      url: "/Checkout/ShippingAddress",
      success: function (data) {
        var divOrderSummaryData = data;
        $("#divordersummary").html('');
        if (divOrderSummaryData != null && divOrderSummaryData != "" && divOrderSummaryData != undefined)
          $("#divordersummary").html($(divOrderSummaryData).find('div#divordersummary').html());
      },
      complete: function (data) {
      },
      error: function (xhr, ajaxOptions, thrownError) {
        alert(thrownError);
      }
    });
  }

}