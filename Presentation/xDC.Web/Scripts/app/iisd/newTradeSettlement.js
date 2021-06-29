(function ($, window, document) {

    $(function () {
        var instrumentCodeEquityData = ["Stock AA", "Stock BB"];
        var stockCodeData = ["GT170006", "MX070003", "MO170004", "GJ180003", "MZ160002", "MO140001"];

        var $tabpanel, $equityGrid, $bondGrid, $cpGrid, $notesPaperGrid, $repoGrid, $couponGrid, $feesGrid,
            $mtmGrid, $fxSettlementGrid, $contributionCreditedGrid, $altidGrid, $othersGrid,
            $tradeSettlementForm;

        $tabpanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { title: "Equity", template: "equityTab" },
                { title: "Bond", template: "bondTab" },
                { title: "CP", template: "cpTab" },
                { title: "Notes & Papers", template: "notesPaperTab" },
                { title: "REPO", template: "repoTab" },
                { title: "Coupon Received", template: "couponReceivedTab" },
                { title: "Fees", template: "feesTab" },
                { title: "MTM", template: "mtmTab" },
                { title: "FX Settlement", template: "fxSettlementTab" },
                { title: "Contribution Credited", template: "contributionCreditedTab" },
                { title: "ALTID DD", template: "altidTab" },
                { title: "Others", template: "othersTab" }
            ],
            deferRendering: false
        });

        $equityGrid = $("#equityGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "Equity",
                    lookup: {
                        dataSource: instrumentCodeEquityData
                    }
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN",
                    lookup: {
                        dataSource: stockCodeData
                    }
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

        $equityGrid.option(dxGridUtils.editingGridConfig);

        $bondGrid = $("#bondGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "Bond",
                    lookup: {
                        dataSource: instrumentCodeEquityData
                    }
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN",
                    lookup: {
                        dataSource: stockCodeData
                    }
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

        $bondGrid.option(dxGridUtils.editingGridConfig);

        $cpGrid = $("#cpGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "CP",
                    lookup: {
                        dataSource: instrumentCodeEquityData
                    }
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN",
                    lookup: {
                        dataSource: stockCodeData
                    }
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

        $cpGrid.option(dxGridUtils.editingGridConfig);

        $notesPaperGrid = $("#notesPaperGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "Notes & Papers",
                    lookup: {
                        dataSource: instrumentCodeEquityData
                    }
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN",
                    lookup: {
                        dataSource: stockCodeData
                    }
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

        $notesPaperGrid.option(dxGridUtils.editingGridConfig);

        $repoGrid = $("#repoGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "REPO",
                    lookup: {
                        dataSource: instrumentCodeEquityData
                    }
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN",
                    lookup: {
                        dataSource: stockCodeData
                    }
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

        $repoGrid.option(dxGridUtils.editingGridConfig);

        $couponGrid = $("#couponGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "Coupon Received",
                    lookup: {
                        dataSource: instrumentCodeEquityData
                    }
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN",
                    lookup: {
                        dataSource: stockCodeData
                    }
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

        $couponGrid.option(dxGridUtils.editingGridConfig);

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

        $feesGrid.option(dxGridUtils.editingGridConfig);

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

        $mtmGrid.option(dxGridUtils.editingGridConfig);

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

        $fxSettlementGrid.option(dxGridUtils.editingGridConfig);

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

        $contributionCreditedGrid.option(dxGridUtils.editingGridConfig);

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

        $altidGrid.option(dxGridUtils.editingGridConfig);

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

        $othersGrid.option(dxGridUtils.editingGridConfig);

        $tradeSettlementForm = $("#tradeSettlementForm").submit(function(e) {
            alert("Woi Submit!");
            e.preventDefault();
        });
    });
}(window.jQuery, window, document));