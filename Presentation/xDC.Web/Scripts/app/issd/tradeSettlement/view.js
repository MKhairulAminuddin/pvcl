(function ($, window, document) {

    $(function () {
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
                    //$loadPanel.show();

                    var data = {
                        id: getUrlParameter("id"),
                        isExportAsExcel: true
                    };

                    $.ajax({
                        type: "POST",
                        url: window.location.origin + "/issd/Print",
                        data: data,
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

                    var data = {
                        id: getUrlParameter("id"),
                        isExportAsExcel: false
                    };

                    $.ajax({
                        type: "POST",
                        url: window.location.origin + "/issd/Print",
                        data: data,
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

        function gridDataSource(tradeType) {
            return DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: window.location.origin +
                    "/api/issd/TradeSettlement/TradeItem/" + window.location.pathname.split("/").pop() + "/" + tradeType
            });
        }

        $equityGrid = $("#equityGrid").dxDataGrid({
            dataSource: gridDataSource("equity"),
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

        $equityGrid.option(dxGridUtils.viewOnlyGridConfig);

        $bondGrid = $("#bondGrid").dxDataGrid({
            dataSource: gridDataSource("bond"),
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

        $bondGrid.option(dxGridUtils.viewOnlyGridConfig);

        $cpGrid = $("#cpGrid").dxDataGrid({
            dataSource: gridDataSource("cp"),
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

        $cpGrid.option(dxGridUtils.viewOnlyGridConfig);

        $notesPaperGrid = $("#notesPaperGrid").dxDataGrid({
            dataSource: gridDataSource("notesPaper"),
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "Notes & Papers",
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

        $notesPaperGrid.option(dxGridUtils.viewOnlyGridConfig);

        $repoGrid = $("#repoGrid").dxDataGrid({
            dataSource: gridDataSource("repo"),
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

        $repoGrid.option(dxGridUtils.viewOnlyGridConfig);

        $couponGrid = $("#couponGrid").dxDataGrid({
            dataSource: gridDataSource("coupon"),
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

        $couponGrid.option(dxGridUtils.viewOnlyGridConfig);

        $feesGrid = $("#feesGrid").dxDataGrid({
            dataSource: gridDataSource("fees"),
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

        $feesGrid.option(dxGridUtils.viewOnlyGridConfig);

        $mtmGrid = $("#mtmGrid").dxDataGrid({
            dataSource: gridDataSource("mtm"),
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

        $mtmGrid.option(dxGridUtils.viewOnlyGridConfig);

        $fxSettlementGrid = $("#fxSettlementGrid").dxDataGrid({
            dataSource: gridDataSource("fxSettlement"),
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

        $fxSettlementGrid.option(dxGridUtils.viewOnlyGridConfig);

        $contributionCreditedGrid = $("#contributionCreditedGrid").dxDataGrid({
            dataSource: gridDataSource("contributionCredited"),
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

        $contributionCreditedGrid.option(dxGridUtils.viewOnlyGridConfig);

        $altidGrid = $("#altidGrid").dxDataGrid({
            dataSource: gridDataSource("altid"),
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

        $altidGrid.option(dxGridUtils.viewOnlyGridConfig);

        $othersGrid = $("#othersGrid").dxDataGrid({
            dataSource: gridDataSource("others"),
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