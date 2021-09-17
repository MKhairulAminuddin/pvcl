(function ($, window, document) {
    $(function () {
        //#region Variable Definition

        tradeSettlement.setSideMenuItemActive("/issd/TradeSettlement");

        var $tabpanel,

            $equityGrid,
            $openingBalanceGrid,

            $bondGrid,
            $cpGrid,
            $notesPaperGrid,
            $repoGrid,
            $couponGrid,

            $mtmGrid,
            $fxSettlementGrid,

            $altidGrid,

            $feesGrid,
            $cnGrid,
            $othersGrid,
            
            $tradeSettlementForm,
            $currencySelectBox,
            $approverDropdown,
            $printBtn;
        
        //#endregion

        //#region Data Source & Functions

        var loadData = function () {
            return $.ajax({
                dataType: "json",
                url: tradeSettlement.api.loadApprovedTrades(),
                method: "get",
                success: function (response) {
                    loadTabs(response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    app.alertError(errorThrown + ": " + jqXHR.responseJSON);
                },
                complete: function (data) {
                    //$selectApproverModal.modal("hide");
                }
            });
        }

        var loadTabs = function (response) {
            var tabPanelItems = [];

            if (response.data[0].totalEquity) {
                var newItem = {
                    title: "Equity",
                    template: function (itemData, itemIndex, element) {
                        var newTabView = $("<div id='equityGrid' class='grid-container-tabview' style='width: 100%'/>");
                        newTabView.dxDataGrid({
                            dataSource: tradeSettlement.dsApprovedTradeItems(response.data[0].formIdEquity, "equity"),
                            columns: [
                                {
                                    caption: "#",
                                    cellTemplate: function (cellElement, cellInfo) {
                                        cellElement.text(cellInfo.row.rowIndex + 1);
                                    },
                                    allowEditing: false,
                                    width: "30px"
                                },
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
                            paging: {
                                enabled: false
                            }
                        });
                        $equityGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].totalEquity > 0) {
                    newItem.badge = response.data[0].totalEquity;
                }
                tabPanelItems.push(newItem);
            }

            if (response.data[0].totalBond) {
                var newItem = {
                    title: "Bond",
                    template: function (itemData, itemIndex, element) {
                        var newTabView = $("<div id='bondGrid' class='grid-container-tabview' style='width: 100%'/>");
                        newTabView.dxDataGrid({
                            dataSource: tradeSettlement.dsApprovedTradeItems(response.data[0].formIdBond, "bond"),
                            columns: [
                                {
                                    caption: "#",
                                    cellTemplate: function (cellElement, cellInfo) {
                                        cellElement.text(cellInfo.row.rowIndex + 1);
                                    },
                                    allowEditing: false,
                                    width: "30px"
                                },
                                {
                                    dataField: "instrumentCode",
                                    caption: "Bond",
                                    allowEditing: false
                                },
                                {
                                    dataField: "stockCode",
                                    caption: "Stock Code/ ISIN",
                                    allowEditing: false
                                },
                                {
                                    dataField: "maturity",
                                    caption: "Maturity (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "sales",
                                    caption: "Sales (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "purchase",
                                    caption: "Purchase (-)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
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
                            paging: {
                                enabled: false
                            }
                        });
                        $bondGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].totalBond > 0) {
                    newItem.badge = response.data[0].totalBond;
                }
                tabPanelItems.push(newItem);
            }

            if (response.data[0].totalCp) {
                var newItem = {
                    title: "CP",
                    template: function (itemData, itemIndex, element) {
                        var newTabView = $("<div id='cpGrid' class='grid-container-tabview' style='width: 100%'/>");
                        newTabView.dxDataGrid({
                            dataSource: tradeSettlement.dsApprovedTradeItems(response.data[0].formIdCp, "cp"),
                            columns: [
                                {
                                    caption: "#",
                                    cellTemplate: function (cellElement, cellInfo) {
                                        cellElement.text(cellInfo.row.rowIndex + 1);
                                    },
                                    allowEditing: false,
                                    width: "30px"
                                },
                                {
                                    dataField: "instrumentCode",
                                    caption: "CP",
                                    allowEditing: false
                                },
                                {
                                    dataField: "stockCode",
                                    caption: "Stock Code/ ISIN",
                                    allowEditing: false
                                },
                                {
                                    dataField: "maturity",
                                    caption: "Maturity (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "sales",
                                    caption: "Sales (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "purchase",
                                    caption: "Purchase (-)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
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
                            paging: {
                                enabled: false
                            }
                        });
                        $cpGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].totalCp > 0) {
                    newItem.badge = response.data[0].totalCp;
                }
                tabPanelItems.push(newItem);
            }

            if (response.data[0].totalNotesPapers) {
                var newItem = {
                    title: "Notes/Papers",
                    template: function (itemData, itemIndex, element) {
                        var newTabView = $("<div id='notesPaperGrid' class='grid-container-tabview' style='width: 100%'/>");
                        newTabView.dxDataGrid({
                            dataSource: tradeSettlement.dsApprovedTradeItems(response.data[0].formIdNotesPapers, "notesPapers"),
                            columns: [
                                {
                                    caption: "#",
                                    cellTemplate: function (cellElement, cellInfo) {
                                        cellElement.text(cellInfo.row.rowIndex + 1);
                                    },
                                    allowEditing: false,
                                    width: "30px"
                                },
                                {
                                    dataField: "instrumentCode",
                                    caption: "Notes & Papers",
                                    allowEditing: false
                                },
                                {
                                    dataField: "stockCode",
                                    caption: "Stock Code/ ISIN",
                                    allowEditing: false
                                },
                                {
                                    dataField: "maturity",
                                    caption: "Maturity (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "sales",
                                    caption: "Sales (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "purchase",
                                    caption: "Purchase (-)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
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
                            paging: {
                                enabled: false
                            }
                        });
                        $notesPaperGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].totalNotesPapers > 0) {
                    newItem.badge = response.data[0].totalNotesPapers;
                }
                tabPanelItems.push(newItem);
            }

            if (response.data[0].totalRepo) {
                var newItem = {
                    title: "REPO",
                    template: function (itemData, itemIndex, element) {
                        var newTabView = $("<div id='repoGrid' class='grid-container-tabview' style='width: 100%'/>");
                        newTabView.dxDataGrid({
                            dataSource: tradeSettlement.dsApprovedTradeItems(response.data[0].formIdRepo, "repo"),
                            columns: [
                                {
                                    caption: "#",
                                    cellTemplate: function (cellElement, cellInfo) {
                                        cellElement.text(cellInfo.row.rowIndex + 1);
                                    },
                                    allowEditing: false,
                                    width: "30px"
                                },
                                {
                                    dataField: "instrumentCode",
                                    caption: "REPO",
                                    allowEditing: false
                                },
                                {
                                    dataField: "stockCode",
                                    caption: "Stock Code/ ISIN",
                                    allowEditing: false
                                },
                                {
                                    dataField: "firstLeg",
                                    caption: "1st Leg (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "secondLeg",
                                    caption: "2nd Leg (-)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
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
                            paging: {
                                enabled: false
                            }
                        });
                        $repoGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].totalRepo > 0) {
                    newItem.badge = response.data[0].totalRepo;
                }
                tabPanelItems.push(newItem);
            }

            if (response.data[0].totalCoupon) {
                var newItem = {
                    title: "Coupon",
                    template: function (itemData, itemIndex, element) {
                        var newTabView = $("<div id='couponGrid' class='grid-container-tabview' style='width: 100%'/>");
                        newTabView.dxDataGrid({
                            dataSource: tradeSettlement.dsApprovedTradeItems(response.data[0].formIdCoupon, "coupon"),
                            columns: [
                                {
                                    caption: "#",
                                    cellTemplate: function (cellElement, cellInfo) {
                                        cellElement.text(cellInfo.row.rowIndex + 1);
                                    },
                                    allowEditing: false,
                                    width: "30px"
                                },
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
                            },
                            paging: {
                                enabled: false
                            }
                        });
                        $couponGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].totalCoupon > 0) {
                    newItem.badge = response.data[0].totalCoupon;
                }
                tabPanelItems.push(newItem);
            }

            if (response.data[0].totalMtm) {
                var newItem = {
                    title: "MTM",
                    template: function (itemData, itemIndex, element) {
                        var newTabView = $("<div id='mtmGrid' class='grid-container-tabview' style='width: 100%'/>");
                        newTabView.dxDataGrid({
                            dataSource: tradeSettlement.dsApprovedTradeItems(response.data[0].formIdMtm, "mtm"),
                            columns: [
                                {
                                    caption: "#",
                                    cellTemplate: function (cellElement, cellInfo) {
                                        cellElement.text(cellInfo.row.rowIndex + 1);
                                    },
                                    allowEditing: false,
                                    width: "30px"
                                },
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
                            },
                            paging: {
                                enabled: false
                            }
                        });
                        $mtmGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].totalMtm > 0) {
                    newItem.badge = response.data[0].totalMtm;
                }
                tabPanelItems.push(newItem);
            }

            if (response.data[0].totalFx) {
                var newItem = {
                    title: "FX",
                    template: function (itemData, itemIndex, element) {
                        var newTabView = $("<div id='fxSettlementGrid' class='grid-container-tabview' style='width: 100%'/>");
                        newTabView.dxDataGrid({
                            dataSource: tradeSettlement.dsApprovedTradeItems(response.data[0].formIdFx, "fxSettlement"),
                            columns: [
                                {
                                    caption: "#",
                                    cellTemplate: function (cellElement, cellInfo) {
                                        cellElement.text(cellInfo.row.rowIndex + 1);
                                    },
                                    allowEditing: false,
                                    width: "30px"
                                },
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
                            },
                            paging: {
                                enabled: false
                            }
                        });
                        $fxSettlementGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].totalFx > 0) {
                    newItem.badge = response.data[0].totalFx;
                }
                tabPanelItems.push(newItem);
            }

            if (response.data[0].totalAltid) {
                var newItem = {
                    title: "ALTID",
                    template: function (itemData, itemIndex, element) {
                        var newTabView = $("<div id='altidGrid' class='grid-container-tabview' style='width: 100%'/>");
                        newTabView.dxDataGrid({
                            dataSource: tradeSettlement.dsApprovedTradeItems(response.data[0].formIdAltid, "altid"),
                            columns: [
                                {
                                    caption: "#",
                                    cellTemplate: function (cellElement, cellInfo) {
                                        cellElement.text(cellInfo.row.rowIndex + 1);
                                    },
                                    allowEditing: false,
                                    width: "30px"
                                },
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
                            },
                            paging: {
                                enabled: false
                            }
                        });
                        $altidGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].totalAltid > 0) {
                    newItem.badge = response.data[0].totalAltid;
                }
                tabPanelItems.push(newItem);
            }

            if (response.data[0].totalFees) {
                var newItem = {
                    title: "Fees",
                    template: function (itemData, itemIndex, element) {
                        var newTabView = $("<div id='feesGrid' class='grid-container-tabview' style='width: 100%'/>");
                        newTabView.dxDataGrid({
                            dataSource: tradeSettlement.dsApprovedTradeItems(response.data[0].formIdFees, "fees"),
                            columns: [
                                {
                                    caption: "#",
                                    cellTemplate: function (cellElement, cellInfo) {
                                        cellElement.text(cellInfo.row.rowIndex + 1);
                                    },
                                    allowEditing: false,
                                    width: "30px"
                                },
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
                                    }
                                ]
                            },
                            paging: {
                                enabled: false
                            }
                        });
                        $feesGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].totalFees > 0) {
                    newItem.badge = response.data[0].totalFees;
                }
                tabPanelItems.push(newItem);
            }

            if (response.data[0].totalCn) {
                var newItem = {
                    title: "Contribution Credited",
                    template: function (itemData, itemIndex, element) {
                        var newTabView = $("<div id='cnGrid' class='grid-container-tabview' style='width: 100%'/>");
                        newTabView.dxDataGrid({
                            dataSource: tradeSettlement.dsApprovedTradeItems(response.data[0].formIdCn, "contributionCredited"),
                            columns: [
                                {
                                    caption: "#",
                                    cellTemplate: function (cellElement, cellInfo) {
                                        cellElement.text(cellInfo.row.rowIndex + 1);
                                    },
                                    allowEditing: false,
                                    width: "30px"
                                },
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
                            },
                            paging: {
                                enabled: false
                            }
                        });
                        $cnGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].totalCn > 0) {
                    newItem.badge = response.data[0].totalCn;
                }
                tabPanelItems.push(newItem);
            }

            if (response.data[0].totalOthers) {
                var newItem = {
                    title: "Others",
                    template: function (itemData, itemIndex, element) {
                        var newTabView = $("<div id='othersGrid' class='grid-container-tabview' style='width: 100%'/>");
                        newTabView.dxDataGrid({
                            dataSource: tradeSettlement.dsApprovedTradeItems(response.data[0].formIdOthers, "others"),
                            columns: [
                                {
                                    caption: "#",
                                    cellTemplate: function (cellElement, cellInfo) {
                                        cellElement.text(cellInfo.row.rowIndex + 1);
                                    },
                                    allowEditing: false,
                                    width: "30px"
                                },
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
                            },
                            paging: {
                                enabled: false
                            }
                        });
                        $othersGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].totalOthers > 0) {
                    newItem.badge = response.data[0].totalOthers;
                }
                tabPanelItems.push(newItem);
            }

            $tabpanel.option("dataSource", tabPanelItems);
            $tabpanel.option("selectedIndex", tabPanelItems[0]);
        }

        //#endregion

        //#region Other Widgets
        
        $tabpanel = $("#tabpanel").dxTabPanel({
            dataSource: [],
            showNavButtons: true
        }).dxTabPanel("instance");

        $printBtn = $("#printBtn").dxDropDownButton(tradeSettlement.printBtnWidgetSettingConsolidated).dxDropDownButton("instance");

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

        // #endregion DataGrid

        //#region Events
        

        //#endregion

        //#region Immediate Invocation function

        loadData();

        //#endregion
    });
}(window.jQuery, window, document));