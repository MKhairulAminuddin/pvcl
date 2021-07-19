(function ($, window, document) {

    $(function () {
        var aduserStores = DevExpress.data.AspNet.createStore({
            key: "username",
            loadUrl: "../api/common/GetActiveDirectoryUsers"
        });

        var $emailPicker = $("#emailPicker").dxSelectBox({
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
                    emailRecipient: $emailPicker.option("value")
                };

                $.ajax({
                    dataType: 'json',
                    url: '../api/admin/testEmail',
                    method: 'post',
                    data: data
                }).done(function (data) {
                    $("#error_container").bs_warning("Email Sent");

                }).fail(function (jqXHR, textStatus, errorThrown) {
                    $("#error_container").bs_alert(textStatus + ': ' + errorThrown);

                });

            }
        }).dxButton("instance");


        var $adGrid = $("#activeDirectoryGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "username",
                loadUrl: "../api/common/GetActiveDirectoryUsers"
            }),
            columns: [
                {
                    dataField: "username",
                    caption: "Username",
                    sortOrder: "asc"
                },
                {
                    dataField: "email",
                    caption: "Email"
                },
                {
                    dataField: "displayName",
                    caption: "Display Name"
                },
                {
                    dataField: "title",
                    caption: "Title"
                },
                {
                    dataField: "department",
                    caption: "Department"
                },
                {
                    dataField: "telNo",
                    caption: "Tel. No"
                },
                {
                    dataField: "office",
                    caption: "Base Office"
                },
                {
                    dataField: "distinguishedName",
                    caption: "Distinguished Name",
                    dataType: "string"
                },
                {
                    dataField: "adAccountCreated",
                    caption: "AD Account Created",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm"
                },
                {
                    dataField: "adAccountChanged",
                    caption: "AD Account Changed",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm",
                    sortOrder: "desc"
                },
                {
                    dataField: "lastBadPasswordAttempt",
                    caption: "Last Bad Password Attempt",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm"
                },
                {
                    dataField: "lastLogon",
                    caption: "Last Logon",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm"
                },
                {
                    dataField: "lastPasswordSet",
                    caption: "Last Password Set",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm"
                }
            ]
        }).dxDataGrid("instance");

        $adGrid.option(dxGridUtils.viewOnlyGridConfig);
    });
}(window.jQuery, window, document));