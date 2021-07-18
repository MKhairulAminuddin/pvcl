(function ($, window, document) {

    $(function () {
        var adUsersStore = DevExpress.data.AspNet.createStore({
            key: "username",
            loadUrl: "../api/common/GetActiveDirectoryUsers"
        });

        $("#inflowFundSaveButton").dxButton("instance").option("onClick", function (e) {

            var data = {
                "inflowFundEnableNotification": $("#inFundEnableNotiCb").dxCheckBox("instance").option('value'),
                "inflowFundEnableAdminModificationNotification": $("#inFundEnableAdminEditNotiCb").dxCheckBox("instance").option('value'),
                "inflowFundCutOffTime": new Date(moment($("#inFundCutOffTimeDb").dxDateBox("instance").option('value'))).toISOString()
            };

            $.ajax({
                data: data,
                dataType: 'json',
                url: '../api/setting/UpdateInflowFundFormNotificationSetting',
                method: 'post'
            }).done(function (data) {
                $("#error_container").bs_success("Inflow Form setting updates saved");

            }).fail(function (jqXHR, textStatus, errorThrown) {
                $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
            });


        });

        $("#contributionEmailList").dxTagBox({
            dataSource: adUsersStore,
            displayExpr: "displayName",
            valueExpr: "email",
            searchEnabled: true,
            itemTemplate: function (data) {
                return "<div class='active-directory-dropdown'>" +
                    "<p class='active-directory-title'>" + data.displayName + "</p>" +
                    "<p class='active-directory-subtitle'>" + data.title + ", " + data.department + "</p>" +
                    "<p class='active-directory-subtitle'>" + data.email + "</p>" +
                    "</div>";
            }
        }).dxTagBox("instance");

    });
}(window.jQuery, window, document));