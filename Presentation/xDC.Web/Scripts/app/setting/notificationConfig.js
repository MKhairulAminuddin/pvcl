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

        $("#cnEmail").dxTextBox({
            validationRules: [{
                type: "required",
                message: "fak u!"
            }]
        });

        var cnEmailEnable = $("#cnEmailEnable").dxSwitch('instance');
        var cnEmailCcEnable = $("#cnEmailCcEnable").dxSwitch('instance');
        var peEmailEnable = $("#peEmailEnable").dxSwitch('instance');
        var peEmailCcEnable = $("#peEmailCcEnable").dxSwitch('instance');
        var propertyEmailEnable = $("#propertyEmailEnable").dxSwitch('instance');
        var propertyEmailCcEnable = $("#propertyEmailCcEnable").dxSwitch('instance');
        var loanEmailEnable = $("#loanEmailEnable").dxSwitch('instance');
        var loanEmailCcEnable = $("#loanEmailCcEnable").dxSwitch('instance');
        var fcaTaggingEmailEnable = $("#fcaTaggingEmailEnable").dxSwitch('instance');
        var approvedTreasuryEnable = $("#approvedTreasuryEnable").dxSwitch('instance');

        var cnEmail = $("#cnEmail").dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value);}
            }],
        }).dxTextBox("instance");
        var cnEmailCc = $("#cnEmailCc").dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        }).dxTextBox("instance");
        var peEmail = $("#peEmail").dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        }).dxTextBox("instance");
        var peEmailCc = $("#peEmailCc").dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        }).dxTextBox("instance");
        var propertyEmail = $("#propertyEmail").dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        }).dxTextBox("instance");
        var propertyEmailCc = $("#propertyEmailCc").dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        }).dxTextBox("instance");
        var loanEmail = $("#loanEmail").dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        }).dxTextBox("instance");
        var loanEmailCc = $("#loanEmailCc").dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        }).dxTextBox("instance");
        var fcaTaggingEmail = $("#fcaTaggingEmail").dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        }).dxTextBox("instance");
        var approvedTreasury = $("#approvedTreasury").dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        }).dxTextBox("instance");

        $("#tradeSettlementSaveButton").dxButton("instance").option("onClick", function (e) {
            
            var data = {
                "tsCnEmailEnable": cnEmailEnable.option('value'),
                "tsCnEmailCcEnable": cnEmailCcEnable.option('value'),
                "tsPeEmailEnable": peEmailEnable.option('value'),
                "tsPeEmailCcEnable": peEmailCcEnable.option('value'),
                "tsPropertyEmailEnable": propertyEmailEnable.option('value'),
                "tsPropertyEmailCcEnable": propertyEmailCcEnable.option('value'),
                "tsLoanEmailEnable": loanEmailEnable.option('value'),
                "tsLoanEmailCcEnable": loanEmailCcEnable.option('value'),
                "tsFcaTaggingEmailEnable": fcaTaggingEmailEnable.option('value'),
                "tsApprovedTreasuryEnable": approvedTreasuryEnable.option('value'),

                "tsCnEmail": cnEmail.option('value'),
                "tsCnEmailCc": cnEmailCc.option('value'),
                "tsPeEmail": peEmail.option('value'),
                "tsPeEmailCc": peEmailCc.option('value'),
                "tsPropertyEmail": propertyEmail.option('value'),
                "tsPropertyEmailCc": propertyEmailCc.option('value'),
                "tsLoanEmail": loanEmail.option('value'),
                "tsLoanEmailCc": loanEmailCc.option('value'),
                "tsFcaTaggingEmail": fcaTaggingEmail.option('value'),
                "tsApprovedTreasury": approvedTreasury.option('value')
            }
            console.log(data);

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

        function validateEmails(string) {
            var regex = /^([\w-\.]+@([\w-]+\.)+[\w-]{2,4})?$/;
            var result = string.replace(/\s/g, "").split(/,/);
            for (var i = 0; i < result.length; i++) {
                if (!regex.test(result[i])) {
                    return false;
                }
            }
            return true;
        }

    });
}(window.jQuery, window, document));