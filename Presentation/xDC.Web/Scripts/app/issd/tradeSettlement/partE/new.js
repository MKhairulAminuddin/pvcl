﻿(function ($, window, document) {

    $(function () {
        //#region Variable Definition
        
        tradeSettlement.setSideMenuItemActive("/issd/TradeSettlement");
        
        var $tabpanel,

            $feesGrid,
            $contributionCreditedGrid,
            $othersGrid,
            
            $approverDropdown,
            $approvalNotes,

            $settlementDateBox,
            $currencySelectBox,

            $saveAsDraftBtn,
            $submitForApprovalBtn,

            $selectApproverModal = $('#selectApproverModal'),
            $submitForApprovalModalBtn,

            $tradeSettlementForm,
            isSaveAsDraft;

        var referenceUrl = {
            postNewFormRequest: window.location.origin + "/api/issd/TradeSettlement/New",
            postNewFormResponse: window.location.origin + "/issd/TradeSettlement/PartE/View/",
        };
        
        //#endregion

        //#region Data Source & Functions

        var populateDwData = function(settlementDate, currency) {
            if (settlementDate && currency) {
                $.when(
                    tradeSettlement.dsTradeItemEdw("FEES", settlementDate, currency),
                    tradeSettlement.dsTradeItemEdw("CONTRIBUTION CREDITED", settlementDate, currency),
                    tradeSettlement.dsTradeItemEdw("OTHERS", settlementDate, currency)
                    )
                    .done(function(data1, data2, data3) {
                        $feesGrid.option("dataSource", data1[0].data);
                        $feesGrid.repaint();

                        $contributionCreditedGrid.option("dataSource", data2[0].data);
                        $contributionCreditedGrid.repaint();

                        $othersGrid.option("dataSource", data3[0].data);
                        $othersGrid.repaint();

                        tradeSettlement.defineTabBadgeNumbers([
                            { titleId: "titleBadge7", title: "Fees", template: "feesTab" },
                            { titleId: "titleBadge10", title: "Contribution", template: "contributionCreditedTab" },
                            { titleId: "titleBadge12", title: "Others", template: "othersTab" }
                        ]);
                    })
                    .always(function(dataOrjqXHR, textStatus, jqXHRorErrorThrown) {
                        tradeSettlement.toast("Data Updated", "info");
                    })
                    .then(function() {

                    });
            } else {
                dxGridUtils.clearGrid($equityGrid);
            }
        };

        function postData(isDraft) {
            
            var data = {
                currency: $currencySelectBox.option("value"),
                settlementDateEpoch: moment($settlementDateBox.option("value")).unix(),
                formType: 7,
                isSaveAsDraft: isDraft,
                
                fees: $feesGrid.getDataSource().items(),
                contributionCredited: $contributionCreditedGrid.getDataSource().items(),
                others: $othersGrid.getDataSource().items(),

                approver: (isDraft) ? null : $approverDropdown.option("value"),
                approvalNotes: (isDraft) ? null : $approvalNotes.option("value")
            };

            return $.ajax({
                data: data,
                dataType: "json",
                url: referenceUrl.postNewFormRequest,
                method: "post",
                success: function (response) {
                    window.location.href = referenceUrl.postNewFormResponse + response;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $("#error_container").bs_alert(errorThrown + ": " + jqXHR.responseJSON);
                },
                complete: function (data) {
                    
                }
            });
        }

        //#endregion
        
        //#region Other Widgets

        $settlementDateBox = $("#settlementDateBox").dxDateBox(tradeSettlement.settlementDateBox).dxValidator({
            validationRules: [
                {
                    type: "required",
                    message: "Settlement Date is required"
                }
            ]
        }).dxDateBox("instance");

        $currencySelectBox = $("#currencySelectBox").dxSelectBox(tradeSettlement.currencySelectBox)
            .dxValidator({
                validationRules: [
                    {
                        type: "required",
                        message: "Currency is required"
                    }
                ]
            })
            .dxSelectBox("instance");

        $tabpanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { titleId: "titleBadge7", title: "Fees", template: "feesTab" },
                { titleId: "titleBadge10", title: "Contribution", template: "contributionCreditedTab" },
                { titleId: "titleBadge12", title: "Others", template: "othersTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });

        $approverDropdown = $("#approverDropdown").dxSelectBox(tradeSettlement.submitApproverSelectBox).dxSelectBox("instance");

        $approvalNotes = $("#approvalNotes").dxTextArea(tradeSettlement.submitApprovalNotesTextArea).dxTextArea("instance");
        
        //#endregion
        
        // #region Data Grid
        
        $feesGrid = $("#feesGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "id",
                    caption: "Id",
                    visible: false
                },
                {
                    dataField: "formId",
                    caption: "Form Id",
                    visible: false
                },
                {
                    dataField: "instrumentCode",
                    caption: "Fees"
                },
                {
                    dataField: "amountPlus",
                    caption: "Amount (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "remarks",
                    caption: "Remarks",
                    dataType: "text"
                }
            ],
            summary: {
                totalItems: [
                    {
                        column: "instrumentCode",
                        displayFormat: "TOTAL"
                    },
                    {
                        column: "amountPlus",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ]
            },
            onSaved: function () {
                tradeSettlement.defineTabBadgeNumbers([
                    { titleId: "titleBadge7", title: "Fees", template: "feesTab" }
                ]);
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            }
        }).dxDataGrid("instance");

        $contributionCreditedGrid = $("#contributionCreditedGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "id",
                    caption: "Id",
                    visible: false
                },
                {
                    dataField: "formId",
                    caption: "Form Id",
                    visible: false
                },
                {
                    dataField: "instrumentCode",
                    caption: "Contribution Credited"
                },
                {
                    dataField: "amountPlus",
                    caption: "Amount (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "remarks",
                    caption: "Remarks",
                    dataType: "text"
                }
            ],
            summary: {
                totalItems: [
                    {
                        column: "instrumentCode",
                        displayFormat: "TOTAL"
                    },
                    {
                        column: "amountPlus",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ]
            },
            onSaved: function () {
                tradeSettlement.defineTabBadgeNumbers([
                    { titleId: "titleBadge10", title: "Contribution", template: "contributionCreditedTab" }
                ]);
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            }
        }).dxDataGrid("instance");

        $othersGrid = $("#othersGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "id",
                    caption: "Id",
                    visible: false
                },
                {
                    dataField: "formId",
                    caption: "Form Id",
                    visible: false
                },
                {
                    dataField: "instrumentCode",
                    caption: "Others"
                },
                {
                    dataField: "amountPlus",
                    caption: "Amount (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "amountMinus",
                    caption: "Amount (-)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "remarks",
                    caption: "Remarks",
                    dataType: "text"
                }
            ],
            summary: {
                totalItems: [
                    {
                        column: "instrumentCode",
                        displayFormat: "TOTAL"
                    },
                    {
                        column: "amountPlus",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "amountMinus",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ]
            },
            onSaved: function () {
                tradeSettlement.defineTabBadgeNumbers([
                    { titleId: "titleBadge12", title: "Others", template: "othersTab" }
                ]);
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            }
        }).dxDataGrid("instance");
        
        // #endregion Data Grid
 
        //#region Events

        $settlementDateBox.on("valueChanged", function (data) {
            populateDwData(data.value, $currencySelectBox.option("value"));
        });

        $currencySelectBox.on("valueChanged", function (data) {
            populateDwData($settlementDateBox.option("value"), data.value);
        });


        $saveAsDraftBtn = $("#saveAsDraftBtn").on({
            "click": function (e) {
                isSaveAsDraft = true;
            }
        });

        $submitForApprovalBtn = $("#submitForApprovalBtn").on({
            "click": function (e) {
                isSaveAsDraft = false;
            }
        });

        $tradeSettlementForm = $("#tradeSettlementForm").on("submit",
            function (e) {
                tradeSettlement.saveAllGrids($feesGrid, $contributionCreditedGrid, $othersGrid);

                if (moment().subtract(1, "days").isAfter($settlementDateBox.option("value"))) {
                    alert("T-n only available for viewing..");
                }
                else {
                    if (isSaveAsDraft) {
                        // new clean draft
                        setTimeout(function () {
                            postData(true);
                        }, 1000);
                    }
                    else {
                        $selectApproverModal.modal('show');
                    }
                }

                e.preventDefault();
            });

        $submitForApprovalModalBtn = $("#submitForApprovalModalBtn").on({
            "click": function (e) {
                tradeSettlement.saveAllGrids($feesGrid, $contributionCreditedGrid, $othersGrid);

                setTimeout(function () {
                    postData(false);
                }, 1000);
                e.preventDefault();
            }
        });
        
        //#endregion
    });
}(window.jQuery, window, document));