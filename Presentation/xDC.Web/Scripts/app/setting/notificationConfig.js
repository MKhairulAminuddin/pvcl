(function ($, window, document) {

    $(function () {
        var adUsersStore = DevExpress.data.AspNet.createStore({
            key: "username",
            loadUrl: "../api/common/GetActiveDirectoryUsersByDepartment/Contribution"
        });

        $("#inflowFundSaveButton").dxButton("instance").option("onClick", function (e) {

            var data = {
                "inflowFundEnableNotification": $("#inFundEnableNotiCb").dxCheckBox("instance").option('value'),
                "inflowFundEnableAdminModificationNotification": true,
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
                app.alertError(textStatus + ': ' + errorThrown);
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

        $("#tradeSettlementSaveButton").dxButton("instance").option("onClick", function (e) {
            
            var selectedContributionEmail = $("#contributionEmailList").dxTagBox("instance").option('selectedItems');
            var emailList = [];
            $.each(selectedContributionEmail, function (key, value) {
                //console.log(key, value);
                emailList.push(value.email);
            });

            var data = {
                "tradeSettlementContributionEmail": emailList
            }

            $.ajax({
                data: data,
                dataType: 'json',
                url: '../api/setting/UpdateTradeSettlementNotificationSetting',
                method: 'post'
            }).done(function (data) {
                $("#error_container").bs_success("Trade Settlement setting updated");

            }).fail(function (jqXHR, textStatus, errorThrown) {
                app.alertError(textStatus + ': ' + errorThrown);
            });

        });

    });
}(window.jQuery, window, document));