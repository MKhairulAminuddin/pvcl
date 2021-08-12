(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        
        tradeSettlement.setSideMenuItemActive("/issd/TradeSettlement");

        var $tabpanel,

            $feesGrid,
            $contributionCreditedGrid,
            $othersGrid,

            $tradeSettlementForm,
            $currencySelectBox,
            $approverDropdown,
            $printBtn;

        var referenceUrl = {
            submitApprovalRequest: window.location.origin + "/api/issd/TradeSettlement/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/PartE/View/"
        };

        //#endregion

        //#region Data Source & Functions

        var populateData = function() {
            $.when(
                    tradeSettlement.dsTradeItem("fees"),
                    tradeSettlement.dsTradeItem("contributionCredited"),
                    tradeSettlement.dsTradeItem("others")
                )
                .done(function(data1, data2, data3) {
                    $feesGrid.option("dataSource", data1[0].data);
                    $feesGrid.repaint();

                    $contributionCreditedGrid.option("dataSource", data2[0].data);
                    $contributionCreditedGrid.repaint();

                    $othersGrid.option("dataSource", data3[0].data);
                    $othersGrid.repaint();

                    tradeSettlement.defineTabBadgeNumbers([
                        { titleId: "titleBadge7", dxDataGrid: $feesGrid },
                        { titleId: "titleBadge10", dxDataGrid: $contributionCreditedGrid },
                        { titleId: "titleBadge12", dxDataGrid: $othersGrid }
                    ]);
                })
                .then(function() {
                    console.log("Done load data");
                });
        };

        var submitApprovalRequest = function (isToApprove) {
            var data = {
                approvalNote: (isToApprove)
                    ? $("#approvalNoteTextBox").dxTextArea("instance").option("value")
                    : $("#rejectionNoteTextBox").dxTextArea("instance").option("value"),
                approvalStatus: isToApprove,
                formId: tradeSettlement.getIdFromQueryString
            };
            
            $.ajax({
                data: data,
                dataType: "json",
                url: referenceUrl.submitApprovalRequest,
                method: "post"
            }).done(function (data) {
                window.location.href = referenceUrl.submitApprovalResponse + data;

            }).fail(function (jqXHR, textStatus, errorThrown) {
                $("#error_container").bs_alert(textStatus + ": " + errorThrown);
            });
        }

        //#endregion
        
        //#region Other Widgets
        
        $printBtn = $("#printBtn").dxDropDownButton(tradeSettlement.printBtnWidgetSetting).dxDropDownButton("instance");
        
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
        
        //#endregion
        
        // #region DataGrid

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
                },
                {
                    dataField: "modifiedBy",
                    caption: "Modified"
                },
                {
                    dataField: "modifiedDate",
                    caption: "Modified Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm a"
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
                },
                {
                    dataField: "modifiedBy",
                    caption: "Modified"
                },
                {
                    dataField: "modifiedDate",
                    caption: "Modified Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm a"
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
                },
                {
                    dataField: "modifiedBy",
                    caption: "Modified"
                },
                {
                    dataField: "modifiedDate",
                    caption: "Modified Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm a"
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
            }
        }).dxDataGrid("instance");
        
        // #endregion DataGrid
        
        //#region Events

        $("#approveBtn").on({
            "click": function (e) {
                $("#approvalNoteModal").modal("show");
                e.preventDefault();
            }
        });

        $("#rejectBtn").on({
            "click": function (e) {
                $("#rejectionNoteModal").modal("show");
                e.preventDefault();
            }
        });

        $("#approveFormBtn").on({
            "click": function (e) {
                submitApprovalRequest(true);
                e.preventDefault();
            }
        });

        $("#rejectFormBtn").on({
            "click": function (e) {
                submitApprovalRequest(false);
                e.preventDefault();
            }
        });

        //#endregion

        //#region Immediate Invocation function

        populateData();

        //#endregion
    });
}(window.jQuery, window, document));