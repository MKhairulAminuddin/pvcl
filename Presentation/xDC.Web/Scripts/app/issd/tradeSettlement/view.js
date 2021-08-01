(function ($, window, document) {

    $(function () {
        populateData();

        var $tabpanel, $equityGrid, $bondGrid, $cpGrid, $notesPaperGrid, $repoGrid, $couponGrid, $feesGrid,
            $mtmGrid, $fxSettlementGrid, $contributionCreditedGrid, $altidGrid, $othersGrid,
            $tradeSettlementForm, $currencySelectBox, $obRentasTb, $obMmaTb, $cbRentasTb, $cbMmaTb,
            $approverDropdown, $printBtn, $loadPanel;

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

        //#region Other Widgets

        $cbRentasTb = $("#cbRentasTb").dxNumberBox({
            readOnly: true,
            stylingMode: "filled"
        }).dxNumberBox("instance");

        $cbMmaTb = $("#cbMmaTb").dxNumberBox({
            readOnly: true,
            stylingMode: "filled"
        }).dxNumberBox("instance");

        $printBtn = $("#printBtn").dxDropDownButton({
            text: "Print",
            icon: "print",
            type: "normal",
            stylingMode: "contained",
            dropDownOptions: {
                width: 230
            },
            items: [
                "Excel Workbook (*.xlsx)",
                "PDF"
            ],
            onItemClick: function (e) {
                if (e.itemData == "Excel Workbook (*.xlsx)") {
                    //$loadPanel.show();

                    $.ajax({
                        type: "POST",
                        url: window.location.origin + "/issd/Print",
                        data: {
                            id: window.location.pathname.split("/").pop(),
                            isExportAsExcel: true
                        },
                        dataType: "text",
                        success: function(data) {
                            var url = window.location.origin + "/issd/ViewPrinted/" + data;
                            window.location = url;
                        },
                        fail: function(jqXHR, textStatus, errorThrown) {
                            $("#error_container").bs_alert(textStatus + ": " + errorThrown);
                        },
                        complete: function(data) {
                            //$loadPanel.hide();
                        }
                    });
                    e.event.preventDefault();
                } else {
                    //$loadPanel.show();

                    $.ajax({
                        type: "POST",
                        url: window.location.origin + "/issd/Print",
                        data: {
                            id: window.location.pathname.split("/").pop(),
                            isExportAsExcel: false
                        },
                        dataType: "text",
                        success: function (data) {
                            var url = window.location.origin + "/issd/ViewPrinted/" + data;
                            window.location = url;
                        },
                        fail: function (jqXHR, textStatus, errorThrown) {
                            $("#error_container").bs_alert(textStatus + ": " + errorThrown);
                        },
                        complete: function (data) {
                            //$loadPanel.hide();
                        }
                    });
                    e.event.preventDefault();
                }
            }
        }).dxDropDownButton("instance");
        
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
            togglePanelTitleItemCount("titleBadge7", $feesGrid);
            togglePanelTitleItemCount("titleBadge8", $mtmGrid);
            togglePanelTitleItemCount("titleBadge9", $fxSettlementGrid);
            togglePanelTitleItemCount("titleBadge10", $contributionCreditedGrid);
            togglePanelTitleItemCount("titleBadge11", $altidGrid);
            togglePanelTitleItemCount("titleBadge12", $othersGrid);
        }

        //#endregion

        //#region Data Source

        function ds(instrumentType) {
            return $.ajax({
                url: window.location.origin +
                    "/api/issd/TradeSettlement/TradeItem/" +
                    window.location.pathname.split("/").pop() +
                    "/" +
                    instrumentType,
                type: "get"
            });
        }

        function dsBalance(balanceType, balanceCategory) {
            return $.ajax({
                url: window.location.origin +
                    "/api/issd/GetBalance/" +
                    window.location.pathname.split("/").pop() +
                    "/" +
                    balanceType +
                    "/" +
                    balanceCategory,
                type: "get"
            });
        }

        function populateData() {
            $.when(
                ds("equity"),
                ds("bond"),
                ds("cp"),
                ds("notesPaper"),
                ds("repo"),
                ds("coupon"),
                ds("fees"),
                ds("mtm"),
                ds("fxSettlement"),
                ds("contributionCredited"),
                ds("altid"),
                ds("others"),

                dsBalance("CLOSING", "RENTAS"),
                dsBalance("CLOSING", "MMA")
            )
                .done(function (data1, data2, data3, data4, data5, data6, data7, data8, data9, data10, data11, data12, data13, data14) {
                    $equityGrid.option("dataSource", data1[0].data);
                    $bondGrid.option("dataSource", data2[0].data);
                    $cpGrid.option("dataSource", data3[0].data);
                    $notesPaperGrid.option("dataSource", data4[0].data);
                    $repoGrid.option("dataSource", data5[0].data);
                    $couponGrid.option("dataSource", data6[0].data);
                    $feesGrid.option("dataSource", data7[0].data);
                    $mtmGrid.option("dataSource", data8[0].data);
                    $fxSettlementGrid.option("dataSource", data9[0].data);
                    $contributionCreditedGrid.option("dataSource", data10[0].data);
                    $altidGrid.option("dataSource", data11[0].data);
                    $othersGrid.option("dataSource", data12[0].data);

                    $equityGrid.repaint();
                    $bondGrid.repaint();
                    $cpGrid.repaint();
                    $notesPaperGrid.repaint();
                    $repoGrid.repaint();
                    $couponGrid.repaint();
                    $feesGrid.repaint();
                    $mtmGrid.repaint();
                    $fxSettlementGrid.repaint();
                    $contributionCreditedGrid.repaint();
                    $altidGrid.repaint();
                    $othersGrid.repaint();

                    $cbRentasTb.option("value", data13[0]);
                    $cbMmaTb.option("value", data14[0]);

                    setTimeout(togglePanelTitleItemCountOnFetchEdw, 1000);
                })
                .then(function () {
                    console.log("Done load data");
                });
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

        // #region DataGrid

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
            }
        }).dxDataGrid("instance");

        $equityGrid.option(dxGridUtils.viewOnlyGridConfig);
        $bondGrid.option(dxGridUtils.viewOnlyGridConfig);
        $cpGrid.option(dxGridUtils.viewOnlyGridConfig);
        $notesPaperGrid.option(dxGridUtils.viewOnlyGridConfig);
        $repoGrid.option(dxGridUtils.viewOnlyGridConfig);
        $couponGrid.option(dxGridUtils.viewOnlyGridConfig);
        $feesGrid.option(dxGridUtils.viewOnlyGridConfig);
        $mtmGrid.option(dxGridUtils.viewOnlyGridConfig);
        $fxSettlementGrid.option(dxGridUtils.viewOnlyGridConfig);
        $contributionCreditedGrid.option(dxGridUtils.viewOnlyGridConfig);
        $altidGrid.option(dxGridUtils.viewOnlyGridConfig);
        $othersGrid.option(dxGridUtils.viewOnlyGridConfig);

        // #endregion DataGrid

        $("#approveFormBtn").on({
            "click": function (e) {

                var data = {
                    approvalNote: $("#approvalNoteTextBox").dxTextArea("instance").option("value"),
                    approvalStatus: true,
                    formId: getUrlParameter("id")
                };

                $.ajax({
                    data: data,
                    dataType: "json",
                    url: window.location.origin + "/api/issd/TradeSettlement/Approval",
                    method: "post"
                }).done(function (data) {
                    window.location.href = window.location.origin + "/issd/TradeSettlement/View?id=" + data;

                }).fail(function (jqXHR, textStatus, errorThrown) {
                    $("#error_container").bs_alert(textStatus + ": " + errorThrown);
                });

                e.preventDefault();
            }
        });

        $("#rejectFormBtn").on({
            "click": function (e) {

                var data = {
                    approvalNote: $("#rejectionNoteTextBox").dxTextArea("instance").option("value"),
                    approvalStatus: false,
                    formId: getUrlParameter("id")
                };

                $.ajax({
                    data: data,
                    dataType: "json",
                    url: window.location.origin + "/api/issd/TradeSettlement/Approval",
                    method: "post"
                }).done(function (data) {
                    window.location.href = window.location.origin + "/issd/TradeSettlement/View?id=" + data;

                }).fail(function (jqXHR, textStatus, errorThrown) {
                    $("#error_container").bs_alert(textStatus + ": " + errorThrown);
                });

                e.preventDefault();
            }
        });

        $loadPanel = $("#loadpanel").dxLoadPanel({
            shadingColor: "rgba(0,0,0,0.4)",
            position: { of: "#formContainer" },
            visible: false,
            showIndicator: true,
            showPane: true,
            shading: true,
            closeOnOutsideClick: false
        }).dxLoadPanel("instance");

    });
}(window.jQuery, window, document));