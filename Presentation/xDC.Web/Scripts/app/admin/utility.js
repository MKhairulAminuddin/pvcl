(function ($, window, document) {

    $(function () {
        var aduserStores = DevExpress.data.AspNet.createStore({
            key: "username",
            loadUrl: "../api/common/GetActiveDirectoryUsers"
        });

        var $emailPicker1 = $("#emailPicker1").dxSelectBox({
            dataSource: aduserStores,
            displayExpr: "email",
            valueExpr: "email",
            searchEnabled: true
        }).dxSelectBox("instance");

        var $testEmailBtn = $("#testEmailBtn").dxButton({
            text: "Test Email",
            type: "default",
            stylingMode: "contained",
            onClick: function (e) {

                var data = {
                    emailRecipient: $emailPicker1.option("value")
                };

                $.ajax({
                    dataType: 'json',
                    url: '../api/admin/testEmail',
                    method: 'post',
                    data: data
                }).done(function (data) {
                    $("#error_container").bs_warning("Email Sent");

                }).fail(function (jqXHR, textStatus, errorThrown) {
                    app.alertError(textStatus + ': ' + errorThrown);

                });

            }
        }).dxButton("instance");


        var $emailPicker2 = $("#emailPicker2").dxSelectBox({
            dataSource: aduserStores,
            displayExpr: "email",
            valueExpr: "email",
            searchEnabled: true,
            onValueChanged(data) {

                $.ajax({
                    dataType: 'json',
                    url: "../api/common/GetActiveDirectoryUser",
                    method: 'POST',
                    data: {
                        "email": data.value
                    }
                }).done(function (response) {
                    $form.option('formData', response);

                }).fail(function (jqXHR, textStatus, errorThrown) {
                    app.alertError(textStatus + ': ' + errorThrown);

                });
            }
        }).dxSelectBox("instance");

        var $form = $('#selectedUserDetails').dxForm({
            readOnly: false,
            showColonAfterLabel: true,
            minColWidth: 300,
            colCount: 2
        }).dxForm("instance");

        var $syncAdBtn = $("#SyncAdBtn").dxButton({
            icon: "fa fa-refresh",
            stylingMode: "contained",
            text: "Sync AD",
            type: "normal",
            onClick: function (e) {
                app.toast("Sync in progress... chillax and wait for it to complete in 5 minutes");

                $.ajax({
                    dataType: 'json',
                    url: "../api/common/SyncActiveDirectory",
                    method: 'GET',
                });

            }
        }).dxButton("instance");


    });
}(window.jQuery, window, document));