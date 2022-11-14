(function ($, window, document) {

    $(function () {

        var referenceUrl = {
            UpdateIssd: window.location.origin + "/api/setting/UpdIssdNotification"
        }


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



        $("#formSaveButton").dxButton({
            onClick: function (e) {
                if (issdCnEmail.validate()) {


                    var data = {
                        "inflowFundCutOffTime": new Date(moment($("#inFundCutOffTimeDb").dxDateBox("instance").option('value'))).toISOString(),

                        "issd_Enable_TsCn": cnEmailEnable.option('value'),
                        "issd_Enable_TsCnCc": cnEmailCcEnable.option('value'),
                        "issd_TsPe": peEmailEnable.option('value'),
                        "issd_Enable_TsPeCc": peEmailCcEnable.option('value'),
                        "issd_Enable_TsProperty": propertyEmailEnable.option('value'),
                        "issd_Enable_TsPropertyCc": propertyEmailCcEnable.option('value'),
                        "issd_Enable_TsLoan": loanEmailEnable.option('value'),
                        "issd_Enable_TsLoanCc": loanEmailCcEnable.option('value'),
                        "issd_Enable_FcaTagging": fcaTaggingEmailEnable.option('value'),
                        "issd_Enable_TApproved": approvedTreasuryEnable.option('value'),

                        "issd_TsCn": issdCnEmail.dxTextBox("instance").option('value'),
                        "issd_TsCnCc": issdCnEmailCc.dxTextBox("instance").option('value'),
                        "issd_TsPe": issdPeEmail.dxTextBox("instance").option('value'),
                        "issd_TsPeCc": issdEmailCc.dxTextBox("instance").option('value'),
                        "issd_TsProperty": issdPropertyEmail.dxTextBox("instance").option('value'),
                        "issd_TsPropertyCc": issdPropertyEmailCc.dxTextBox("instance").option('value'),
                        "issd_TsLoan": issdLoanEmail.dxTextBox("instance").option('value'),
                        "issd_TsLoanCc": issdLoanEmailCc.dxTextBox("instance").option('value'),
                        "issd_FcaTagging": issdFcaTaggingEmail.dxTextBox("instance").option('value'),
                        "issd_TApproved": issdApprovedTreasury.dxTextBox("instance").option('value'),


                        "fid_T_SubmissionCc": treasurySubmissionEmailCc.option('value'),
                        "fid_T_ApprovalCc": treasuryApprovedEmailCc.option('value'),

                        "fid_Enable_T_SubmissionCc": treasurySubmissionEmailCcEnable.option('value'),
                        "fid_Enable_T_ApprovalCc": treasuryApprovalEmailCcEnable.option('value'),
                    }


                    $.ajax({
                        data: data,
                        dataType: 'json',
                        url: referenceUrl.UpdateIssd,
                        method: 'post',
                        success: function (response) {
                            app.toast("Saved: " + response, "info", 3000);
                            console.log(response)
                            //setTimeout(() => window.location.reload(), 1000);

                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            app.alertError(errorThrown + ": " + jqXHR.responseJSON);
                        },
                        complete: function (data) {

                        }
                    });

                }


                e.event.preventDefault();
            }
        });

        //#endregion


        function validateEmails(string) {
            if (string === undefined || string === "" || string === null) {
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