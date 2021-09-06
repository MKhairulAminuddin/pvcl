(function ($, window, document) {

    $(function () {
        //#region Variable Definition
        
        tradeSettlement.setSideMenuItemActive("/issd/TradeSettlement");
        
        var $tabpanel,
            $mtmGrid,
            $fxSettlementGrid,
            
            $approverDropdown,
            $approvalNotes,

            $settlementDateBox,
            $currencySelectBox,

            $saveAsDraftBtn,
            $submitForApprovalBtn,

            $selectApproverModal = $('#selectApproverModal'),
            $submitForApprovalModalBtn,

            $tradeSettlementForm,
            isSaveAsDraft,
            formTypeId = 6;

        var referenceUrl = {
            postNewFormRequest: window.location.origin + "/api/issd/TradeSettlement/New",
            postNewFormResponse: window.location.origin + "/issd/TradeSettlement/PartD/View/",
        };
        
        //#endregion

        //#region Data Source & Functions

        var populateDwData = function(settlementDate, currency) {
            if (settlementDate && currency) {
                $.when(
                    tradeSettlement.dsTradeItemEdw("PAYMENT/RECEIVED (MTM)", settlementDate, currency),
                    tradeSettlement.dsTradeItemEdw("FX SETTLEMENT", settlementDate, currency)
                    )
                    .done(function(data1, data2) {
                        $mtmGrid.option("dataSource", data1[0].data);
                        $mtmGrid.repaint();

                        $fxSettlementGrid.option("dataSource", data2[0].data);
                        $fxSettlementGrid.repaint();

                        tradeSettlement.defineTabBadgeNumbers([
                            { titleId: "titleBadge8", dxDataGrid: $mtmGrid },
                            { titleId: "titleBadge9", dxDataGrid: $fxSettlementGrid }
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
                formType: formTypeId,
                isSaveAsDraft: isDraft,
                
                mtm: $mtmGrid.getDataSource().items(),
                fxSettlement: $fxSettlementGrid.getDataSource().items(),

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
                    app.alertError(errorThrown + ": " + jqXHR.responseJSON);
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
                { titleId: "titleBadge8", title: "MTM", template: "mtmTab" },
                { titleId: "titleBadge9", title: "FX", template: "fxSettlementTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });

        $approverDropdown = $("#approverDropdown").dxSelectBox(tradeSettlement.submitApproverSelectBox).dxSelectBox("instance");

        $approvalNotes = $("#approvalNotes").dxTextArea(tradeSettlement.submitApprovalNotesTextArea).dxTextArea("instance");
        
        //#endregion
        
        // #region Data Grid
        
        $mtmGrid = $("#mtmGrid").dxDataGrid({
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
                    caption: "Payment/ Receipt (MTM)"
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
                    { titleId: "titleBadge8", dxDataGrid: $mtmGrid }
                ]);
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            }
        }).dxDataGrid("instance");

        $fxSettlementGrid = $("#fxSettlementGrid").dxDataGrid({
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
                    caption: "FX Settlement"
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
                    { titleId: "titleBadge9", dxDataGrid: $fxSettlementGrid }
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
                tradeSettlement.saveAllGrids($mtmGrid, $fxSettlementGrid);

                if (tradeSettlement.val_isTMinus1($settlementDateBox.option("value"))) {
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
                tradeSettlement.saveAllGrids($mtmGrid, $fxSettlementGrid);

                setTimeout(function () {
                    postData(false);
                }, 1000);
                e.preventDefault();
            }
        });
        
        //#endregion
    });
}(window.jQuery, window, document));