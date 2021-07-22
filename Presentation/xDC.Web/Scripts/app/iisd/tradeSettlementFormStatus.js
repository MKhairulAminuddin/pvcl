(function ($, window, document) {

    $(function () {
        var instrumentCodeEquityData = ["Stock AA", "Stock BB"];
        var stockCodeData = ["GT170006", "MX070003", "MO170004", "GJ180003", "MZ160002", "MO140001"];
        
        var $tabpanel, $equityGrid, $bondGrid, $cpGrid, $notesPaperGrid, $repoGrid, $couponGrid, $feesGrid,
            $mtmGrid, $fxSettlementGrid, $contributionCreditedGrid, $altidGrid, $othersGrid,
            $tradeSettlementForm, $currencySelectBox, $obRentasTb, $obMmaTb, $cbRentasTb, $cbMmaTb,
            $approverDropdown, $printBtn;

        $("#approveBtn").on({
            "click": function (e) {
                $('#approvalNoteModal').modal('show');

                e.preventDefault();
            }
        });

        $("#rejectBtn").on({
            "click": function (e) {
                $('#rejectionNoteModal').modal('show');

                e.preventDefault();
            }
        });

        $printBtn = $("#printBtn").dxDropDownButton({
            text: "Print",
            icon: "print",
            type: "normal",
            stylingMode: "contained",
            dropDownOptions: {
                width: 230
            },
            onItemClick: function (e) {
                if (e.itemData == "Excel Workbook (*.xlsx)") {
                    DevExpress.ui.notify("Download " + e.itemData, "success", 600);

                    var data = {
                        id: getUrlParameter("id")
                    };

                    $.ajax({
                        type: "POST",
                        url: '/iisd/PrintTradeSettlement',
                        data: data,
                        dataType: "text",
                        success: function (data) {
                            var url = '/iisd/GetPrintTradeSettlement?id=' + data;
                            window.location = url;
                        }
                    });
                    e.event.preventDefault();
                }

            },
            items: [
                "Excel Workbook (*.xlsx)",
                "PDF"
            ]
        }).dxDropDownButton("instance");
        
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

        // #region DataGrid

        $equityGrid = $("#equityGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/iisd/GetTradeSettlement?id=" + getUrlParameter('id') + "&tradeType=equity"
            }),
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
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/iisd/GetTradeSettlement?id=" + getUrlParameter('id') + "&tradeType=bond"
            }),
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
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/iisd/GetTradeSettlement?id=" + getUrlParameter('id') + "&tradeType=cp"
            }),
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
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/iisd/GetTradeSettlement?id=" + getUrlParameter('id') + "&tradeType=notesPaper"
            }),
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
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/iisd/GetTradeSettlement?id=" + getUrlParameter('id') + "&tradeType=repo"
            }),
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
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/iisd/GetTradeSettlement?id=" + getUrlParameter('id') + "&tradeType=coupon"
            }),
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
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/iisd/GetTradeSettlement?id=" + getUrlParameter('id') + "&tradeType=fees"
            }),
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
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/iisd/GetTradeSettlement?id=" + getUrlParameter('id') + "&tradeType=mtm"
            }),
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
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/iisd/GetTradeSettlement?id=" + getUrlParameter('id') + "&tradeType=fxSettlement"
            }),
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
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/iisd/GetTradeSettlement?id=" + getUrlParameter('id') + "&tradeType=contributionCredited"
            }),
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
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/iisd/GetTradeSettlement?id=" + getUrlParameter('id') + "&tradeType=altid"
            }),
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
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/iisd/GetTradeSettlement?id=" + getUrlParameter('id') + "&tradeType=others"
            }),
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

        // #endregion DataGrid

        $("#approveFormBtn").on({
            "click": function (e) {

                var data = {
                    approvalNote: $("#approvalNoteTextBox").dxTextArea("instance").option('value'),
                    approvalStatus: true,
                    formId: getUrlParameter('id')
                };

                $.ajax({
                    data: data,
                    dataType: 'json',
                    url: '../api/amsd/InflowFundsFormApproval',
                    method: 'post'
                }).done(function (data) {
                    window.location.href = "../amsd/InflowFundsFormStatus?id=" + data;

                }).fail(function (jqXHR, textStatus, errorThrown) {
                    $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                });

                e.preventDefault();
            }
        });

        $("#rejectFormBtn").on({
            "click": function (e) {

                var data = {
                    approvalNote: $("#rejectionNoteTextBox").dxTextArea("instance").option('value'),
                    approvalStatus: false,
                    formId: getUrlParameter('id')
                };

                $.ajax({
                    data: data,
                    dataType: 'json',
                    url: '../api/amsd/InflowFundsFormApproval',
                    method: 'post'
                }).done(function (data) {
                    window.location.href = "../amsd/InflowFundsFormStatus?id=" + data;

                }).fail(function (jqXHR, textStatus, errorThrown) {
                    $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                });

                e.preventDefault();
            }
        });



    });
}(window.jQuery, window, document));