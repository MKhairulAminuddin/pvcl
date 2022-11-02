(function ($, window, document) {

    $(function () {
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

        var tSubmissionEmailCcEnable = $("#tSubmissionEmailCcEnable").dxSwitch('instance');
        var tApprovedEmailCcEnable = $("#tApprovedEmailCcEnable").dxSwitch('instance');

        var cnEmail = $("#cnEmail").dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value);}
            }],
        });
        var cnEmailCc = $("#cnEmailCc").dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        var peEmail = $("#peEmail").dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        var peEmailCc = $("#peEmailCc").dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        var propertyEmail = $("#propertyEmail").dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        var propertyEmailCc = $("#propertyEmailCc").dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        var loanEmail = $("#loanEmail").dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        var loanEmailCc = $("#loanEmailCc").dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        var fcaTaggingEmail = $("#fcaTaggingEmail").dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        var approvedTreasury = $("#approvedTreasury").dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        var tSubmissionEmailCc = $("#tSubmissionEmailCc").dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });
        var tApprovedEmailCc = $("#tApprovedEmailCc").dxTextBox({}).dxValidator({
            validationRules: [{
                type: 'custom',
                message: 'Invalid input',
                validationCallback(params) { return validateEmails(params.value); }
            }],
        });

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
                "tSubmissionEmailCcEnable": tSubmissionEmailCcEnable.option('value'),
                "tApprovedEmailCcEnable": tApprovedEmailCcEnable.option('value'),

                "tsCnEmail": cnEmail.option('value'),
                "tsCnEmailCc": cnEmailCc.option('value'),
                "tsPeEmail": peEmail.option('value'),
                "tsPeEmailCc": peEmailCc.option('value'),
                "tsPropertyEmail": propertyEmail.option('value'),
                "tsPropertyEmailCc": propertyEmailCc.option('value'),
                "tsLoanEmail": loanEmail.option('value'),
                "tsLoanEmailCc": loanEmailCc.option('value'),
                "tsFcaTaggingEmail": fcaTaggingEmail.option('value'),
                "tsApprovedTreasury": approvedTreasury.option('value'),
                "tSubmissionEmailCc": tSubmissionEmailCc.option('value'),
                "tApprovedEmailCc": tApprovedEmailCc.option('value'),
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