(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        
        tradeSettlement.setSideMenuItemActive("/issd/TradeSettlement/PartA");

        var $tabpanel,
            $equityGrid,
            $openingBalanceGrid,
            $tradeSettlementForm,
            $currencySelectBox,
            $approverDropdown,
            $printBtn;

        var referenceUrl = {
            submitApprovalRequest: window.location.origin + "/api/issd/TradeSettlement/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/PartA/View/"
        };

        //#endregion

        //#region Data Source & Functions
        
        var populateData = function() {
            $.when(
                    tradeSettlement.dsTradeItem("equity"),
                    tradeSettlement.dsOpeningBalance()
                )
                .done(function(data1, data2) {
                    $equityGrid.option("dataSource", data1[0].data);
                    $equityGrid.repaint();

                    $openingBalanceGrid.option("dataSource", data2[0].data);
                    $openingBalanceGrid.repaint();

                    tradeSettlement.defineTabBadgeNumbers([
                        { titleId: "titleBadge1", dxDataGrid: $equityGrid }
                    ]);

                })
                .then(function() {
                    console.log("Done load data");
                });
        }

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
                { titleId: "titleBadge1", title: "Equity", template: "equityTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });
        
        //#endregion
        
        // #region DataGrid

        $openingBalanceGrid = $("#openingBalanceGrid").dxDataGrid({
            dataSource: [],
            showColumnHeaders: false,
            showColumnLines: false,
            columns: [
                {
                    dataField: "balanceCategory",
                    caption: "Opening Balance"
                },
                {
                    dataField: "currency",
                    caption: "Currency",
                    visible: false
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
            ]
        }).dxDataGrid("instance");

        $equityGrid = $("#equityGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "Equity"
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN"
                },
                {
                    dataField: "maturity",
                    caption: "Maturity (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "sales",
                    caption: "Sales (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "purchase",
                    caption: "Purchase (-)",
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
                        column: "maturity",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "sales",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "purchase",
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