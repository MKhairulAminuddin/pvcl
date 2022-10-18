(function ($, window, document) {

    $(function () {
        var $submitForApprovalModalBtn = $("#submitForApprovalModalBtn"),
            $inflowFundForm = $("#inflowFundForm"),
            $selectApproverModal = $("#selectApproverModal");


        var fundTypeStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: window.location.origin + "/api/common/GetInflowFundsFundType"
        });

        var bankStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: window.location.origin + "/api/common/GetInflowFundsBank"
        });

        var approverStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: window.location.origin + "/api/common/GetApproverAmsdInflowFunds"
        });
        

        var $inflowFundsGrid, $approverDropdown, $historyBtn, $submitBtn, $tbFormId, $tbFormStatus, $approvalNotes;

        var referenceUrl = {
            postForm: window.location.origin + "/api/amsd/InflowFund/New",
            postResult: window.location.origin + "/amsd/InflowFund/View/"
        };

        var postData = function (isDraft) {
            var data = {
                amsdInflowFunds: $inflowFundsGrid.getDataSource().items(),

                approver: (isDraft) ? null : $approverDropdown.option("value"),
                approvalNotes: (isDraft) ? null : $approvalNotes.option("value")
            };

            $.ajax({
                data: data,
                dataType: 'json',
                url: referenceUrl.postForm,
                method: 'post',
                success: function (data) {
                    if (isDraft) {
                        app.toast("Draft saved!", "success", 3000);
                    } else {
                        app.toast("Submitted", "success", 3000);
                    }

                    window.location.href = referenceUrl.postResult + data;
                },
                fail: function (jqXHR, textStatus, errorThrown) {
                    app.alertError(textStatus + ': ' + errorThrown);
                },
                complete: function (data) {

                }
            });
        }


        $approverDropdown = $("#approverDropdown").dxSelectBox({
            dataSource: approverStore,
            displayExpr: "displayName",
            valueExpr: "username",
            searchEnabled: true,
            itemTemplate: function (data) {
                return "<div class='active-directory-dropdown'>" +
                    "<p class='active-directory-title'>" + data.displayName + "</p>" +
                    "<p class='active-directory-subtitle'>" + data.title + ", " + data.department + "</p>" +
                    "<p class='active-directory-subtitle'>" + data.email + "</p>" +
                    "</div>";
            }
        }).dxSelectBox("instance");

        $approvalNotes = $("#approvalNotes").dxTextArea({
            height: 90
        }).dxTextArea("instance");

        //#region Events & Invocations

        $("#saveAsDraftBtn").dxButton({
            onClick: function (e) {
                app.toast("Saving....", "info", 2000);
                app.saveAllGrids($inflowFundsGrid);

                if (jQuery.isEmptyObject($inflowFundsGrid.getDataSource().items())) {
                    $("#error_container").bs_warning("Please key in at least one item.");
                } else {
                    setTimeout(function () {
                        postData(true);
                    }, 1000);
                }
            }
        });

        $submitForApprovalModalBtn.on({
            "click": function (e) {
                app.toast("Submitting for approval....", "info", 3000);
                app.saveAllGrids($inflowFundsGrid);

                if (jQuery.isEmptyObject($inflowFundsGrid.getDataSource().items())) {
                    $("#error_container").bs_warning("Please key in at least one item.");
                } else {
                    setTimeout(function () {
                        postData(false);
                    }, 1000);
                }

                e.preventDefault();
            }
        });

        $inflowFundForm = $("#inflowFundForm").on("submit",
            function (e) {
                e.preventDefault();

                app.saveAllGrids($inflowFundsGrid);

                if (jQuery.isEmptyObject($inflowFundsGrid.getDataSource().items())) {
                    $("#error_container").bs_warning("Please key in at least one item.");
                } else {
                    $selectApproverModal.modal("show");
                }
            });

        //#endregion
        
        $inflowFundsGrid = $("#inflowFundsGrid1").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "fundType",
                    caption: "Fund Types",
                    lookup: {
                        dataSource: fundTypeStore,
                        valueExpr: "value",
                        displayExpr: "value"
                    },
                    validationRules: [{ type: "required" }]
                },
                {
                    dataField: "bank",
                    caption: "Bank",
                    lookup: {
                        dataSource: bankStore,
                        valueExpr: "value",
                        displayExpr: "value"
                    },
                    validationRules: [{ type: "required" }]
                },
                {
                    dataField: "amount",
                    caption: "Amount",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                }
            ],
            showBorders: true,
            height: 300,
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            },
            onRowUpdated: function (e) {
                console.log(e);
            },
            onRowValidating: function (e) {
                if (!e.isValid) {
                    app.alertError("Row contained invalid data. See cell with red border.");
                }
            },
            summary: {
                totalItems: [
                    {
                        column: "type",
                        displayFormat: "TOTAL"
                    },
                    {
                        column: "amount",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ]
            }
        }).dxDataGrid("instance");
        
    });
}(window.jQuery, window, document));