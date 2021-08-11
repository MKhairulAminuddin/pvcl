(function ($, window, document) {

    $(function () {
        DevExpress.setTemplateEngine("underscore");

        var approverStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: window.location.origin + "/api/common/GetTradeSettlementApprover"
        });

        var $tabpanel, $equityGrid, $bondGrid, $cpGrid, $notesPaperGrid, $repoGrid, $couponGrid, $feesGrid,
            $mtmGrid, $fxSettlementGrid, $contributionCreditedGrid, $altidGrid, $othersGrid,
            $tradeSettlementForm, $currencySelectBox, $obRentasTb, $obMmaTb, $cbRentasTb, $cbMmaTb,
            $approverDropdown, $settlementDateBox, $loadPanel, isSaveAsDraft;
        

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

        $tradeSettlementForm = $("#tradeSettlementForm").on("submit",
            function (e) {
                e.preventDefault();
                saveAllGridEditData();
                
                if (moment().subtract(1, "days").isAfter($settlementDateBox.option("value")))
                {
                    alert("T-n only available for viewing..");
                }
                else {
                    if (isSaveAsDraft) {
                        // new clean draft
                        $loadPanel.option("position", { of: "#tradeSettlementForm" });
                        $loadPanel.show();

                        var data = {
                            currency: $currencySelectBox.option("value"),
                            settlementDateEpoch: moment($settlementDateBox.option("value")).unix(),
                            isSaveAsDraft: true,

                            equity: $equityGrid.getDataSource().items(),
                            bond: $bondGrid.getDataSource().items(),
                            cp: $cpGrid.getDataSource().items(),
                            notesPaper: $notesPaperGrid.getDataSource().items(),
                            repo: $repoGrid.getDataSource().items(),
                            coupon: $couponGrid.getDataSource().items(),
                            fees: $feesGrid.getDataSource().items(),
                            mtm: $mtmGrid.getDataSource().items(),
                            fxSettlement: $fxSettlementGrid.getDataSource().items(),
                            contributionCredited: $contributionCreditedGrid.getDataSource().items(),
                            altid: $altidGrid.getDataSource().items(),
                            others: $othersGrid.getDataSource().items(),

                            rentasOpeningBalance: $obRentasTb.option("value"),
                            mmaOpeningBalance: $cbMmaTb.option("value"),
                            rentasClosingBalance: $cbRentasTb.option("value"),
                            mmaClosingBalance: $obMmaTb.option("value")
                        };

                        $.ajax({
                            data: data,
                            dataType: "json",
                            url: window.location.origin + "/api/issd/TradeSettlement/New",
                            method: "post",
                            success: function (data) {
                                window.location.href = "/issd/TradeSettlement/View/" + data;
                            },
                            fail: function (jqXHR, textStatus, errorThrown) {
                                $("#error_container").bs_alert(textStatus + ": " + errorThrown);
                            },
                            complete: function (data) {
                                $loadPanel.hide();
                            }
                        });
                    }
                    else
                    {
                        $('#selectApproverModal').modal('show');
                    }
                }

                
            });

        $("#submitForApprovalModalBtn").on({
            "click": function (e) {
                $loadPanel.option("position", { of: "#selectApproverModalContainer" });
                $loadPanel.show();

                var data = {
                    currency: $currencySelectBox.option("value"),
                    settlementDateEpoch: moment($settlementDateBox.option("value")).unix(),

                    equity: $equityGrid.getDataSource().items(),
                    bond: $bondGrid.getDataSource().items(),
                    cp: $cpGrid.getDataSource().items(),
                    notesPaper: $notesPaperGrid.getDataSource().items(),
                    repo: $repoGrid.getDataSource().items(),
                    coupon: $couponGrid.getDataSource().items(),
                    fees: $feesGrid.getDataSource().items(),
                    mtm: $mtmGrid.getDataSource().items(),
                    fxSettlement: $fxSettlementGrid.getDataSource().items(),
                    contributionCredited: $contributionCreditedGrid.getDataSource().items(),
                    altid: $altidGrid.getDataSource().items(),
                    others: $othersGrid.getDataSource().items(),

                    rentasOpeningBalance: $obRentasTb.option("value"),
                    mmaOpeningBalance: $cbMmaTb.option("value"),

                    rentasClosingBalance: $cbRentasTb.option("value"),
                    mmaClosingBalance: $obMmaTb.option("value"),

                    approver: $approverDropdown.option("value")
                };

                $.ajax({
                    data: data,
                    dataType: "json",
                    url: window.location.origin + "/api/issd/TradeSettlement/New",
                    method: "post",
                    success: function(data) {
                        window.location.href = "/issd/TradeSettlement/View/" + data;
                    },
                    fail: function(jqXHR, textStatus, errorThrown) {
                        $("#error_container").bs_alert(textStatus + ": " + errorThrown);
                    },
                    complete: function(data) {
                        $loadPanel.hide();
                    }
                });

                e.preventDefault();
            }
        });

        $("#saveAsDraftBtn").on({
            "click": function (e) {
                isSaveAsDraft = true;
            }
        });

        $("#submitForApprovalBtn").on({
            "click": function(e) {
                isSaveAsDraft = false;
            }
        });

        //#region Other Widgets

        $settlementDateBox = $("#settlementDateBox").dxDateBox({
            type: "date",
            displayFormat: "dd/MM/yyyy",
            value: new Date(),
            //min: moment().subtract(1, 'days'),
            onValueChanged: function (data) {
                populateDwData(data.value, $currencySelectBox.option("value"));
            }
        }).dxValidator({
            validationRules: [
                {
                    type: "required",
                    message: "Settlement Date is required"
                }
            ]
        }).dxDateBox("instance");

        $currencySelectBox =
            $("#currencySelectBox").dxSelectBox({
                items: ["MYR", "USD"],
                placeHolder: "Currency..",
                onValueChanged: function (data) {
                    populateDwData($settlementDateBox.option("value"), data.value);
                }
            })
            .dxValidator({
                validationRules: [
                    {
                        type: "required",
                        message: "Currency is required"
                    }
                ]
            })
            .dxSelectBox("instance");

        $obRentasTb = $("#obRentasTb").dxNumberBox({
            readOnly: true,
            stylingMode: "filled"
        }).dxNumberBox("instance");

        $obMmaTb = $("#obMmaTb").dxNumberBox({
            readOnly: true,
            stylingMode: "filled"
        }).dxNumberBox("instance");

        $cbRentasTb = $("#cbRentasTb").dxNumberBox({
            readOnly: true,
            stylingMode: "filled"
        }).dxNumberBox("instance");

        $cbMmaTb = $("#cbMmaTb").dxNumberBox({
            readOnly: true,
            stylingMode: "filled"
        }).dxNumberBox("instance");

        $tabpanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { titleId: "titleBadge1", title: "Equity", template: "equityTab" },
                { titleId: "titleBadge2", title: "Bond", template: "bondTab" },
                { titleId: "titleBadge3", title: "CP", template: "cpTab" },
                { titleId: "titleBadge4", title: "Notes & Papers", template: "notesPaperTab" },
                { titleId: "titleBadge5", title: "REPO", template: "repoTab" },
                { titleId: "titleBadge6", title: "Coupon", template: "couponReceivedTab" },
                { titleId: "titleBadge7", title: "Fees", template: "feesTab" },
                { titleId: "titleBadge8", title: "MTM", template: "mtmTab" },
                { titleId: "titleBadge9", title: "FX", template: "fxSettlementTab" },
                { titleId: "titleBadge10", title: "Contribution", template: "contributionCreditedTab" },
                { titleId: "titleBadge11", title: "ALTID", template: "altidTab" },
                { titleId: "titleBadge12", title: "Others", template: "othersTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });

        function togglePanelTitleItemCount(countTagId, gridInstance) {
            var count = gridInstance.getDataSource().items().length;

            if (count > 0) {
                $("#" + countTagId).addClass("label label-danger").css("margin-left", "4px").text(count);
            } else {
                $("#" + countTagId).removeClass("label label-danger").css("margin-left", "0").text("");
            }
        }

        function togglePanelTitleItemCountOnFetchEdw() {
            togglePanelTitleItemCount("titleBadge1", $equityGrid);
            togglePanelTitleItemCount("titleBadge2", $bondGrid);
            togglePanelTitleItemCount("titleBadge3", $cpGrid);
            togglePanelTitleItemCount("titleBadge4", $notesPaperGrid);
            togglePanelTitleItemCount("titleBadge5", $repoGrid);
            togglePanelTitleItemCount("titleBadge6", $couponGrid);
        }

        //#endregion

        //#region Data Source

        function ds(instrumentType, settlementDate, currency) {
            return $.ajax({
                url: window.location.origin + "/api/issd/GetTradeSettlementFromEdw/" + instrumentType + "/" + moment(settlementDate).unix() + "/" + currency,
                type: "get"
            });
        }

        function dsOpeningBalance(instrumentType, settlementDate, currency) {
            return $.ajax({
                url: window.location.origin + "/api/issd/GetOpeningBalance/" + instrumentType + "/" + moment(settlementDate).unix() + "/" + currency,
                type: "get"
            });
        }

        function populateDwData(settlementDate, currency) {
            if (settlementDate && currency) {
                $.when(
                        ds("EQUITY", settlementDate, currency),
                        ds("BOND", settlementDate, currency),
                        ds("COMMERCIAL PAPER", settlementDate, currency),
                        ds("NOTES AND PAPERS", settlementDate, currency),
                        ds("REPO", settlementDate, currency),
                        ds("COUPON", settlementDate, currency),
                        dsOpeningBalance("RENTAS", settlementDate, currency),
                        dsOpeningBalance("MMA", settlementDate, currency)
                    )
                    .done(function (data1, data2, data3, data4, data5, data6, data7, data8) {
                        $equityGrid.option("dataSource", data1[0].data);
                        $equityGrid.repaint();
                        
                        $bondGrid.option("dataSource", data2[0].data);
                        $bondGrid.repaint();
                        
                        $cpGrid.option("dataSource", data3[0].data);
                        $cpGrid.repaint();
                        
                        $notesPaperGrid.option("dataSource", data4[0].data);
                        $notesPaperGrid.repaint();
                        
                        $repoGrid.option("dataSource", data5[0].data);
                        $repoGrid.repaint();
                        
                        $couponGrid.option("dataSource", data6[0].data);
                        $couponGrid.repaint();
                        
                        $obRentasTb.option("value", data7[0]);
                        $obMmaTb.option("value", data8[0]);

                        setTimeout(calculateClosingBalance, 500);
                        setTimeout(togglePanelTitleItemCountOnFetchEdw, 500);
                    })
                    .always(function (dataOrjqXHR, textStatus, jqXHRorErrorThrown) {
                        toast("Data Updated", "info");
                    })
                    .then(function () {
                    });
            } else {
                dxGridUtils.clearGrid($equityGrid);
                dxGridUtils.clearGrid($bondGrid);
                dxGridUtils.clearGrid($cpGrid);
                dxGridUtils.clearGrid($notesPaperGrid);
                dxGridUtils.clearGrid($repoGrid);
                dxGridUtils.clearGrid($couponGrid);
            }
        }

        function calculateClosingBalance() {
            var totalRentasClosing = $obRentasTb.option("value")

                + $equityGrid.getTotalSummaryValue("sales")
                + $bondGrid.getTotalSummaryValue("sales")
                + $cpGrid.getTotalSummaryValue("sales")
                + $notesPaperGrid.getTotalSummaryValue("sales")
                + $repoGrid.getTotalSummaryValue("firstLeg")
                + $couponGrid.getTotalSummaryValue("amountPlus")
                + $feesGrid.getTotalSummaryValue("amountPlus")
                + $mtmGrid.getTotalSummaryValue("amountPlus")
                + $fxSettlementGrid.getTotalSummaryValue("amountPlus")
                + $contributionCreditedGrid.getTotalSummaryValue("amountPlus")
                + $altidGrid.getTotalSummaryValue("amountPlus")
                + $othersGrid.getTotalSummaryValue("amountPlus")

                - $equityGrid.getTotalSummaryValue("purchase")
                - $bondGrid.getTotalSummaryValue("purchase")
                - $cpGrid.getTotalSummaryValue("purchase")
                - $notesPaperGrid.getTotalSummaryValue("purchase")
                - $repoGrid.getTotalSummaryValue("secondLeg")
                - $mtmGrid.getTotalSummaryValue("amountMinus")
                - $fxSettlementGrid.getTotalSummaryValue("amountMinus")
                - $altidGrid.getTotalSummaryValue("amountMinus")
                - $othersGrid.getTotalSummaryValue("amountMinus")
                ;
            
            $cbRentasTb.option("value", totalRentasClosing.toFixed(2));

            toast("Closing balance recalculated", "info");
        }

        function saveAllGridEditData() {
            $couponGrid.saveEditData();
            $feesGrid.saveEditData();
            $mtmGrid.saveEditData();
            $fxSettlementGrid.saveEditData();
            $contributionCreditedGrid.saveEditData();
            $altidGrid.saveEditData();
            $othersGrid.saveEditData();
        }
        
        //#endregion

        // #region Data Grid

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
        
        $bondGrid = $("#bondGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "Bond"
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
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: false,
                allowAdding: false
            }
        }).dxDataGrid("instance");
        
        $cpGrid = $("#cpGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "CP"
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
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: false,
                allowAdding: false
            }
        }).dxDataGrid("instance");
        
        $notesPaperGrid = $("#notesPaperGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "Notes & Papers"
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
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: false,
                allowAdding: false
            }
        }).dxDataGrid("instance");
        
        $repoGrid = $("#repoGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "REPO"
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN"
                },
                {
                    dataField: "firstLeg",
                    caption: "1st Leg (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "secondLeg",
                    caption: "2nd Leg (-)",
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
                        column: "firstLeg",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "secondLeg",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ]
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: false,
                allowAdding: false
            }
        }).dxDataGrid("instance");
        
        $couponGrid = $("#couponGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "Coupon Received"
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN"
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
                calculateClosingBalance();
                togglePanelTitleItemCount("titleBadge6", $couponGrid);
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            }
        }).dxDataGrid("instance");
        
        $feesGrid = $("#feesGrid").dxDataGrid({
            dataSource: [],
            columns: [
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
                calculateClosingBalance();
                togglePanelTitleItemCount("titleBadge7", $feesGrid);
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            }
        }).dxDataGrid("instance");
        
        $mtmGrid = $("#mtmGrid").dxDataGrid({
            dataSource: [],
            columns: [
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
                calculateClosingBalance();
                togglePanelTitleItemCount("titleBadge8", $mtmGrid);
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
                calculateClosingBalance();
                togglePanelTitleItemCount("titleBadge9", $fxSettlementGrid);
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
                calculateClosingBalance();
                togglePanelTitleItemCount("titleBadge10", $contributionCreditedGrid);
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            }
        }).dxDataGrid("instance");
        
        $altidGrid = $("#altidGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "ALTID Distribution & Drawdown"
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
                calculateClosingBalance();
                togglePanelTitleItemCount("titleBadge11", $altidGrid);
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
                calculateClosingBalance();
                togglePanelTitleItemCount("titleBadge12", $othersGrid);
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            }
        }).dxDataGrid("instance");

        $equityGrid.option(dxGridUtils.viewOnlyGridConfig);
        $bondGrid.option(dxGridUtils.viewOnlyGridConfig);
        $cpGrid.option(dxGridUtils.viewOnlyGridConfig);
        $notesPaperGrid.option(dxGridUtils.viewOnlyGridConfig);
        $repoGrid.option(dxGridUtils.viewOnlyGridConfig);
        $couponGrid.option(dxGridUtils.tradeSettlementGridConfig);
        $feesGrid.option(dxGridUtils.tradeSettlementGridConfig);
        $mtmGrid.option(dxGridUtils.tradeSettlementGridConfig);
        $fxSettlementGrid.option(dxGridUtils.tradeSettlementGridConfig);
        $contributionCreditedGrid.option(dxGridUtils.tradeSettlementGridConfig);
        $altidGrid.option(dxGridUtils.tradeSettlementGridConfig);
        $othersGrid.option(dxGridUtils.tradeSettlementGridConfig);

        // #endregion Data Grid

        $loadPanel = $("#loadpanel").dxLoadPanel({
            shadingColor: "rgba(0,0,0,0.4)",
            visible: false,
            showIndicator: true,
            showPane: true,
            shading: true,
            closeOnOutsideClick: false
        }).dxLoadPanel("instance");
    });
}(window.jQuery, window, document));