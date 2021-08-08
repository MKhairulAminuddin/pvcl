(function ($, window, document) {
    $(function () {
        populateData();

        var $tabpanel, $equityGrid, $openingBalanceGrid,
            $tradeSettlementForm, $currencySelectBox, 
            $approverDropdown, $printBtn, $loadPanel;

        //#region Data Source & Functions

        function ds(instrumentType) {
            return $.ajax({
                url: window.location.origin +
                    "/api/issd/TradeSettlement/TradeItem/" +
                    window.location.pathname.split("/").pop() +
                    "/" + instrumentType,
                type: "get"
            });
        }

        function dsOpeningBalance(formId) {
            return $.ajax({
                url: window.location.origin + "/api/issd/GetBalance/" + formId,
                type: "get"
            });
        }
        
        function populateData() {
            $.when(
                    ds("equity"),
                    dsOpeningBalance(window.location.pathname.split("/").pop())
                )
                .done(function(data1, data2) {
                    $equityGrid.option("dataSource", data1[0].data);
                    $equityGrid.repaint();

                    $openingBalanceGrid.option("dataSource", data2[0].data);
                    $openingBalanceGrid.repaint();

                    setTimeout(defineTabBadgeNumbers, 1000);
                })
                .then(function() {
                    console.log("Done load data");
                });
        }

        function tabBadgeItemCount(countTagId, gridInstance) {
            var count = gridInstance.getDataSource().items().length;

            if (count > 0) {
                $("#" + countTagId).addClass("label label-danger").css("margin-left", "4px").text(count);
            } else {
                $("#" + countTagId).removeClass("label label-danger").css("margin-left", "0").text("");
            }
        }

        function defineTabBadgeNumbers() {
            tabBadgeItemCount("titleBadge1", $equityGrid);
        }

        //#endregion
        
        
        //#region Other Widgets
        
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

        $equityGrid.option(dxGridUtils.viewOnlyGridConfig);
        
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

        //#endregion
    });
}(window.jQuery, window, document));