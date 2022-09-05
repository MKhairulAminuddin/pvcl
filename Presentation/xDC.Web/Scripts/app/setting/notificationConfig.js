(function ($, window, document) {

    $(function () {
        var adUsers = DevExpress.data.AspNet.createStore({
            key: "username",
            loadUrl: "../api/common/GetActiveDirectoryUsers"
        });

        var cnEmailDs = function () {
            return $.ajax({
                url: referenceUrl.dsInflowDeposit + window.location.pathname.split("/").pop(),
                type: "get"
            });
        };

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

        $("#cnEmail").dxTagBox({
            dataSource: adUsers,
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
            
            var selectedCnEmail = $("#cnEmail").dxTagBox("instance").option('selectedItems');
            var cnEmail = [];
            $.each(selectedCnEmail, function (key, value) {
                cnEmail.push(value.email);
            });

            var data = {
                "tsCnEmail": cnEmail,
                "tsCnEmailCc": cnEmailCc,
                "tsPeEmail": peEmail,
                "tsPeEmailCc": peEmailCc,
                "tsPropertyEmail": propertyEmail,
                "tsPropertyEmailCc": propertyEmailCc,
                "tsLoanEmail": loanEmail,
                "tsLoanEmailCc": loanEmailCc,
                "tsFcaTaggingEmail": fcaTaggingEmail,
                "tsApprovedTreasury": approvedTreasury
            }

            $.ajax({
                data: data,
                dataType: 'json',
                url: '../api/setting/UpdateTsNotificationSetting',
                method: 'post'
            }).done(function (data) {
                $("#error_container").bs_success("Trade Settlement setting updated");

            }).fail(function (jqXHR, textStatus, errorThrown) {
                app.alertError(textStatus + ': ' + errorThrown);
            });

        });

    });
}(window.jQuery, window, document));