(function ($, window, document) {

    $(function () {

        var referenceUrl = {
            UpdateIssd: window.location.origin + "/api/setting/UpdIssdNotification"
        }

        //#region Inflow Fund Form - Email Notification Setting

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

        //#endregion

        //#region Trade Settlement Form - Email Notification Setting

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

        var issdCnEmail = $("#cnEmail");
        var issdCnEmailCc = $("#cnEmailCc");
        var issdPeEmail = $("#peEmail");
        var issdEmailCc = $("#peEmailCc");
        var issdPropertyEmail = $("#propertyEmail");
        var issdPropertyEmailCc = $("#propertyEmailCc");
        var issdLoanEmail = $("#loanEmail");
        var issdLoanEmailCc = $("#loanEmailCc");
        var issdFcaTaggingEmail = $("#fcaTaggingEmail");
        var issdApprovedTreasury = $("#approvedTreasury");

        issdCnEmail.dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        issdCnEmailCc.dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        issdPeEmail.dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        issdEmailCc.dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        issdPropertyEmail.dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        issdPropertyEmailCc.dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        issdLoanEmail.dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        issdLoanEmailCc.dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        issdFcaTaggingEmail.dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        issdApprovedTreasury.dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });

        $("#IssdNotificationForm").on("submit", function (e) {
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

                    "tsCnEmail": issdCnEmail.dxTextBox("instance").option('value'),
                    "tsCnEmailCc": issdCnEmailCc.dxTextBox("instance").option('value'),
                    "tsPeEmail": issdPeEmail.dxTextBox("instance").option('value'),
                    "tsPeEmailCc": issdEmailCc.dxTextBox("instance").option('value'),
                    "tsPropertyEmail": issdPropertyEmail.dxTextBox("instance").option('value'),
                    "tsPropertyEmailCc": issdPropertyEmailCc.dxTextBox("instance").option('value'),
                    "tsLoanEmail": issdLoanEmail.dxTextBox("instance").option('value'),
                    "tsLoanEmailCc": issdLoanEmailCc.dxTextBox("instance").option('value'),
                    "tsFcaTaggingEmail": issdFcaTaggingEmail.dxTextBox("instance").option('value'),
                    "tsApprovedTreasury": issdApprovedTreasury.dxTextBox("instance").option('value'),
                }
                

                $.ajax({
                    data: data,
                    dataType: 'json',
                    url: referenceUrl.UpdateIssd,
                    method: 'post',
                    success: function (response) {
                        app.toast("Saving....", "info", 3000);
                        //setTimeout(() => window.location.reload(), 1000);
                        
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        app.alertError(errorThrown + ": " + jqXHR.responseJSON);
                    },
                    complete: function (data) {

                    }
                });

            e.preventDefault();
        });

        //#endregion


        //#region  Treasury Form - Email Notification Setting

        //#endregion



        var treasurySubmissionEmailCcEnable = $("#treasurySubmissionEmailCcEnable").dxSwitch('instance');
        var treasuryApprovalEmailCcEnable = $("#treasuryApprovalEmailCcEnable").dxSwitch('instance');

        var treasurySubmissionEmailCc = $("#treasurySubmissionEmailCc").dxTextBox().dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        }).dxTextBox("instance");
        var treasuryApprovedEmailCc = $("#treasuryApprovedEmailCc").dxTextBox().dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        }).dxTextBox("instance");

        $("#treasurySaveButton").dxButton({
            onClick: function (e) {
                var data = {
                    "tSubmissionEmailCcEnable": treasurySubmissionEmailCcEnable.option('value'),
                    "tApprovedEmailCcEnable": treasuryApprovalEmailCcEnable.option('value'),

                    "tSubmissionEmailCc": treasurySubmissionEmailCc.option('value'),
                    "tApprovedEmailCc": treasuryApprovedEmailCc.option('value')
                }
                console.log(data);

                $.ajax({
                    data: data,
                    dataType: 'json',
                    url: '../api/setting/UpdTreasuryFormNotificationSetting',
                    method: 'post'
                }).done(function (data) {
                    $("#error_container").bs_success("Treasury setting updated");

                }).fail(function (jqXHR, textStatus, errorThrown) {
                    app.alertError(textStatus + ': ' + errorThrown);
                });
            }
        });

        function validateEmails(string) {
            if (string === undefined || string === "") {
                return true;
            }

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