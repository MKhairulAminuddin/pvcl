(function ($, window, document) {
    $(function () {
        //#region Variable Definition

        var $tabpanel,
            $openingBalanceGrid,

            $equityGrid,
            $bondGrid,
            $cpGrid,
            $notesPaperGrid,
            $repoGrid,
            $couponGrid,
            $feesGrid,
            $mtmGrid,
            $fxSettlementGrid,
            $contributionCreditedGrid,
            $altidGrid,
            $othersGrid,
            
            $saveChangesBtn,
            $printBtn;

        var referenceUrl = {
            getAvailableTrades: window.location.origin + "/api/fid/Ts10AmAccountAssignment/AvailableTrades/" + window.location.pathname.split("/").pop(),

            equityLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem/equity/" + window.location.pathname.split("/").pop(),
            bondLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem/bond/" + window.location.pathname.split("/").pop(),
            cpLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem/cp/" + window.location.pathname.split("/").pop(),
            notesPaperLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem/notesPaper/" + window.location.pathname.split("/").pop(),
            repoLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem/repo/" + window.location.pathname.split("/").pop(),
            couponLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem/coupon/" + window.location.pathname.split("/").pop(),
            feesLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem/fees/" + window.location.pathname.split("/").pop(),
            mtmLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem/mtm/" + window.location.pathname.split("/").pop(),
            fxLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem/fxSettlement/" + window.location.pathname.split("/").pop(),
            contributionLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem/contributionCredited/" + window.location.pathname.split("/").pop(),
            altidLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem/altid/" + window.location.pathname.split("/").pop(),
            othersLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem/others/" + window.location.pathname.split("/").pop(),

            updateTradeItem: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem",


            opBalanceLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/opBalance/" + window.location.pathname.split("/").pop(),
            opBalanceUpdate: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/opBalance",
            
        };

        //#endregion

        //#region Data Source & Functions

        var dsAccountLookup = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: referenceUrl.opBalanceLoad
        });
        
        var loadData = function () {
            return $.ajax({
                dataType: "json",
                url: referenceUrl.getAvailableTrades,
                method: "get",
                success: function (response) {
                    loadTabs(response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $("#error_container").bs_alert(errorThrown + ": " + jqXHR.responseJSON);
                },
                complete: function (data) {
                    //$selectApproverModal.modal("hide");
                }
            });
        }

        var loadTabs = function (response) {
            var tabPanelItems = $tabpanel.option("dataSource");

            if (response.data[0].equity) {
                var newItem = {
                    title: "Equity",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='equityGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.equityLoad,
                                updateUrl: referenceUrl.updateTradeItem
                            }),
                            columns: [
                                {
                                    dataField: "inflowTo", width: "120px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account", allowClearing: true, allowClearing: true
                                    }
                                },
                                {
                                    dataField: "outflowFrom", width: "120px",
                                    caption: "Outflow From",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account", allowClearing: true, allowClearing: true
                                    }
                                },
                                {
                                    dataField: "instrumentCode",
                                    caption: "Equity",
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
                                    dataField: "assignedBy",
                                    caption: "Assigned By",
                                    allowEditing: false,
                                    visible: false
                                },
                                {
                                    dataField: "assignedDate",
                                    caption: "Assigned Date",
                                    dataType: "datetime",
                                    format: "dd/MM/yyyy HH:mm a",
                                    allowEditing: false,
                                    visible: false
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
                            },
                            showBorders: true
                        });
                        $equityGrid = newTabView;
                        newTabView.appendTo(element);
                    }
                };
                tabPanelItems.push(newItem);
            }

            if (response.data[0].bond) {
                var newItem = {
                    title: "Bond",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='bondGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.bondLoad,
                                updateUrl: referenceUrl.updateTradeItem
                            }),
                            columns: [
                                {
                                    dataField: "inflowTo", width: "120px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account", allowClearing: true, allowClearing: true
                                    }
                                },
                                {
                                    dataField: "outflowFrom", width: "120px",
                                    caption: "Outflow From",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account", allowClearing: true, allowClearing: true
                                    }
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
                                    dataField: "assignedBy",
                                    caption: "Assigned By",
                                    allowEditing: false,
                                    visible: false
                                },
                                {
                                    dataField: "assignedDate",
                                    caption: "Assigned Date",
                                    dataType: "datetime",
                                    format: "dd/MM/yyyy HH:mm a",
                                    allowEditing: false,
                                    visible: false
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
                            },
                            showBorders: true
                        });
                        $bondGrid = newTabView;
                        newTabView.appendTo(element);
                    }
                };
                tabPanelItems.push(newItem);
            }

            if (response.data[0].cp) {
                var newItem = {
                    title: "CP",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='cpGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.cpLoad,
                                updateUrl: referenceUrl.updateTradeItem
                            }),
                            columns: [
                                {
                                    dataField: "inflowTo", width: "120px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "outflowFrom", width: "120px",
                                    caption: "Outflow From",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
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
                                    dataField: "assignedBy",
                                    caption: "Assigned By",
                                    allowEditing: false,
                                    visible: false
                                },
                                {
                                    dataField: "assignedDate",
                                    caption: "Assigned Date",
                                    dataType: "datetime",
                                    format: "dd/MM/yyyy HH:mm a",
                                    allowEditing: false,
                                    visible: false
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
                            },
                            showBorders: true
                        });
                        $cpGrid = newTabView;
                        newTabView.appendTo(element);
                    }
                };
                tabPanelItems.push(newItem);
            }

            if (response.data[0].notesPaper) {
                var newItem = {
                    title: "Notes & Papers",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='notesPaperGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.notesPaperLoad,
                                updateUrl: referenceUrl.updateTradeItem
                            }),
                            columns: [
                                {
                                    dataField: "inflowTo", width: "120px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "outflowFrom", width: "120px",
                                    caption: "Outflow From",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
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
                                    dataField: "assignedBy",
                                    caption: "Assigned By",
                                    allowEditing: false,
                                    visible: false
                                },
                                {
                                    dataField: "assignedDate",
                                    caption: "Assigned Date",
                                    dataType: "datetime",
                                    format: "dd/MM/yyyy HH:mm a",
                                    allowEditing: false,
                                    visible: false
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
                            },
                            showBorders: true
                        });
                        $notesPaperGrid = newTabView;
                        newTabView.appendTo(element);
                    }
                };
                tabPanelItems.push(newItem);
            }

            if (response.data[0].repo) {
                var newItem = {
                    title: "REPO",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='repoGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.repoLoad,
                                updateUrl: referenceUrl.updateTradeItem
                            }),
                            columns: [
                                {
                                    dataField: "inflowTo", width: "120px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "outflowFrom", width: "120px",
                                    caption: "Outflow From",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
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
                                    dataField: "assignedBy",
                                    caption: "Assigned By",
                                    allowEditing: false,
                                    visible: false
                                },
                                {
                                    dataField: "assignedDate",
                                    caption: "Assigned Date",
                                    dataType: "datetime",
                                    format: "dd/MM/yyyy HH:mm a",
                                    allowEditing: false,
                                    visible: false
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
                            },
                            showBorders: true
                        });
                        $repoGrid = newTabView;
                        newTabView.appendTo(element);
                    }
                };
                tabPanelItems.push(newItem);
            }

            if (response.data[0].coupon) {
                var newItem = {
                    title: "Coupon",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='couponGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.couponLoad,
                                updateUrl: referenceUrl.updateTradeItem
                            }),
                            columns: [
                                {
                                    dataField: "inflowTo", width: "120px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "outflowFrom", width: "120px",
                                    caption: "Outflow From",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "instrumentCode",
                                    caption: "Coupon Received",
                                    allowEditing: false
                                },
                                {
                                    dataField: "stockCode",
                                    caption: "Stock Code/ ISIN",
                                    allowEditing: false
                                },
                                {
                                    dataField: "amountPlus",
                                    caption: "Amount (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "assignedBy",
                                    caption: "Assigned By",
                                    allowEditing: false,
                                    visible: false
                                },
                                {
                                    dataField: "assignedDate",
                                    caption: "Assigned Date",
                                    dataType: "datetime",
                                    format: "dd/MM/yyyy HH:mm a",
                                    allowEditing: false,
                                    visible: false
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
                            editing: {
                                mode: "batch",
                                allowUpdating: true,
                                allowDeleting: false,
                                allowAdding: false
                            },
                            showBorders: true
                        });
                        $couponGrid = newTabView;
                        newTabView.appendTo(element);
                    }
                };
                tabPanelItems.push(newItem);
            }

            if (response.data[0].fees) {
                var newItem = {
                    title: "Fees",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='feesGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.feesLoad,
                                updateUrl: referenceUrl.updateTradeItem
                            }),
                            columns: [
                                {
                                    dataField: "inflowTo", width: "120px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "outflowFrom", width: "120px",
                                    caption: "Outflow From",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "instrumentCode",
                                    caption: "Fees",
                                    allowEditing: false
                                },
                                {
                                    dataField: "amountPlus",
                                    caption: "Amount (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "assignedBy",
                                    caption: "Assigned By",
                                    allowEditing: false,
                                    visible: false
                                },
                                {
                                    dataField: "assignedDate",
                                    caption: "Assigned Date",
                                    dataType: "datetime",
                                    format: "dd/MM/yyyy HH:mm a",
                                    allowEditing: false,
                                    visible: false
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
                            editing: {
                                mode: "batch",
                                allowUpdating: true,
                                allowDeleting: false,
                                allowAdding: false
                            },
                            showBorders: true
                        });
                        $feesGrid = newTabView;
                        newTabView.appendTo(element);
                    }
                };
                tabPanelItems.push(newItem);
            }

            if (response.data[0].mtm) {
                var newItem = {
                    title: "Receipts/Payments (MTM)",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='mtmGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.mtmLoad,
                                updateUrl: referenceUrl.updateTradeItem
                            }),
                            columns: [
                                {
                                    dataField: "inflowTo", width: "120px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "outflowFrom", width: "120px",
                                    caption: "Outflow From",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "instrumentCode",
                                    caption: "Payment/ Receipt (MTM)",
                                    allowEditing: false
                                },
                                {
                                    dataField: "amountPlus",
                                    caption: "Amount (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "amountMinus",
                                    caption: "Amount (-)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "assignedBy",
                                    caption: "Assigned By",
                                    allowEditing: false,
                                    visible: false
                                },
                                {
                                    dataField: "assignedDate",
                                    caption: "Assigned Date",
                                    dataType: "datetime",
                                    format: "dd/MM/yyyy HH:mm a",
                                    allowEditing: false,
                                    visible: false
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
                            editing: {
                                mode: "batch",
                                allowUpdating: true,
                                allowDeleting: false,
                                allowAdding: false
                            },
                            showBorders: true
                        });
                        $mtmGrid = newTabView;
                        newTabView.appendTo(element);
                    }
                };
                tabPanelItems.push(newItem);
            }

            if (response.data[0].fx) {
                var newItem = {
                    title: "FX Settlement",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='fxSettlementGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.fxLoad,
                                updateUrl: referenceUrl.updateTradeItem
                            }),
                            columns: [
                                {
                                    dataField: "inflowTo", width: "120px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "outflowFrom", width: "120px",
                                    caption: "Outflow From",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "instrumentCode",
                                    caption: "FX Settlement",
                                    allowEditing: false
                                },
                                {
                                    dataField: "amountPlus",
                                    caption: "Amount (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "amountMinus",
                                    caption: "Amount (-)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "assignedBy",
                                    caption: "Assigned By",
                                    allowEditing: false,
                                    visible: false
                                },
                                {
                                    dataField: "assignedDate",
                                    caption: "Assigned Date",
                                    dataType: "datetime",
                                    format: "dd/MM/yyyy HH:mm a",
                                    allowEditing: false,
                                    visible: false
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
                            editing: {
                                mode: "batch",
                                allowUpdating: true,
                                allowDeleting: false,
                                allowAdding: false
                            },
                            showBorders: true
                        });
                        $fxSettlementGrid = newTabView;
                        newTabView.appendTo(element);
                    }
                };
                tabPanelItems.push(newItem);
            }

            if (response.data[0].contribution) {
                var newItem = {
                    title: "Contribution Credited",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='contributionCreditedGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.contributionLoad,
                                updateUrl: referenceUrl.updateTradeItem
                            }),
                            columns: [
                                {
                                    dataField: "inflowTo", width: "120px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "outflowFrom", width: "120px",
                                    caption: "Outflow From",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "instrumentCode",
                                    caption: "Contribution Credited",
                                    allowEditing: false
                                },
                                {
                                    dataField: "amountPlus",
                                    caption: "Amount (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "assignedBy",
                                    caption: "Assigned By",
                                    allowEditing: false,
                                    visible: false
                                },
                                {
                                    dataField: "assignedDate",
                                    caption: "Assigned Date",
                                    dataType: "datetime",
                                    format: "dd/MM/yyyy HH:mm a",
                                    allowEditing: false,
                                    visible: false
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
                            editing: {
                                mode: "batch",
                                allowUpdating: true,
                                allowDeleting: false,
                                allowAdding: false
                            },
                            showBorders: true
                        });
                        $contributionCreditedGrid = newTabView;
                        newTabView.appendTo(element);
                    }
                };
                tabPanelItems.push(newItem);
            }

            if (response.data[0].altid) {
                var newItem = {
                    title: "ALTID",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='altidGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.altidLoad,
                                updateUrl: referenceUrl.updateTradeItem
                            }),
                            columns: [
                                {
                                    dataField: "inflowTo", width: "120px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "outflowFrom", width: "120px",
                                    caption: "Outflow From",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "instrumentCode",
                                    caption: "ALTID Distribution & Drawdown",
                                    allowEditing: false
                                },
                                {
                                    dataField: "amountPlus",
                                    caption: "Amount (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "amountMinus",
                                    caption: "Amount (-)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "assignedBy",
                                    caption: "Assigned By",
                                    allowEditing: false,
                                    visible: false
                                },
                                {
                                    dataField: "assignedDate",
                                    caption: "Assigned Date",
                                    dataType: "datetime",
                                    format: "dd/MM/yyyy HH:mm a",
                                    allowEditing: false,
                                    visible: false
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
                            editing: {
                                mode: "batch",
                                allowUpdating: true,
                                allowDeleting: false,
                                allowAdding: false
                            },
                            showBorders: true
                        });
                        $altidGrid = newTabView;
                        newTabView.appendTo(element);
                    }
                };
                tabPanelItems.push(newItem);
            }

            if (response.data[0].others) {
                var newItem = {
                    title: "Others",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='othersGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.othersLoad,
                                updateUrl: referenceUrl.updateTradeItem
                            }),
                            columns: [
                                {
                                    dataField: "inflowTo", width: "120px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "outflowFrom", width: "120px",
                                    caption: "Outflow From",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "account",
                                        displayExpr: "account",
                                        allowClearing: true
                                    }
                                },
                                {
                                    dataField: "instrumentCode",
                                    caption: "Others",
                                    allowEditing: false
                                },
                                {
                                    dataField: "amountPlus",
                                    caption: "Amount (+)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "amountMinus",
                                    caption: "Amount (-)",
                                    dataType: "number",
                                    format: {
                                        type: "fixedPoint",
                                        precision: 2
                                    },
                                    allowEditing: false
                                },
                                {
                                    dataField: "assignedBy",
                                    caption: "Assigned By",
                                    allowEditing: false,
                                    visible: false
                                },
                                {
                                    dataField: "assignedDate",
                                    caption: "Assigned Date",
                                    dataType: "datetime",
                                    format: "dd/MM/yyyy HH:mm a",
                                    allowEditing: false,
                                    visible: false
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
                            editing: {
                                mode: "batch",
                                allowUpdating: true,
                                allowDeleting: false,
                                allowAdding: false
                            },
                            showBorders: true
                        });
                        $othersGrid = newTabView;
                        newTabView.appendTo(element);
                    }
                };
                tabPanelItems.push(newItem);
            }
            
            //updateButtonsState(tabPanelItems);

            $tabpanel.option("dataSource", tabPanelItems);
            $tabpanel.option("selectedIndex", tabPanelItems[0]);
        }
        
        //#endregion

        //#region Other Widgets

        $saveChangesBtn = $("#saveChangesBtn").dxButton({
            text: "Save Changes",
            stylingMode: "contained",
            //type: "success",
            icon: "save",
            onClick: function (e) {
                e.event.preventDefault();

                $openingBalanceGrid.saveEditData();

                if (typeof ($equityGrid) !== "undefined") {
                    $("#equityGrid").dxDataGrid("instance").saveEditData();
                }
                if (typeof ($bondGrid) !== "undefined") {
                    $("#bondGrid").dxDataGrid("instance").saveEditData();
                }
                if (typeof ($cpGrid) !== "undefined" ) {
                    $("#cpGrid").dxDataGrid("instance").saveEditData();
                }
                if (typeof ($notesPaperGrid) !== "undefined" ) {
                    $("#notesPaperGrid").dxDataGrid("instance").saveEditData();
                }
                if (typeof ($repoGrid) !== "undefined") {
                    $("#repoGrid").dxDataGrid("instance").saveEditData();
                }
                if (typeof ($couponGrid) !== "undefined") {
                    $("#couponGrid").dxDataGrid("instance").saveEditData();
                }
                if (typeof ($feesGrid) !== "undefined") {
                    $("#feesGrid").dxDataGrid("instance").saveEditData();
                }
                if (typeof ($mtmGrid) !== "undefined") {
                    $("#mtmGrid").dxDataGrid("instance").saveEditData();
                }
                if (typeof ($fxSettlementGrid) !== "undefined") {
                    $("#fxSettlementGrid").dxDataGrid("instance").saveEditData();
                }
                if (typeof ($contributionCreditedGrid) !== "undefined") {
                    $("#contributionCreditedGrid").dxDataGrid("instance").saveEditData();
                }
                if (typeof ($altidGrid) !== "undefined") {
                    $("#altidGrid").dxDataGrid("instance").saveEditData();
                }
                if (typeof ($othersGrid) !== "undefined") {
                    $("#othersGrid").dxDataGrid("instance").saveEditData();
                }

                app.toast("Dah Saved", "Success");
            }
        }).dxButton("instance");

        $tabpanel = $("#tabpanel").dxTabPanel({
            dataSource: [],
            showNavButtons: true
        }).dxTabPanel("instance");

        //#endregion

        //#region DataGrid

        $openingBalanceGrid = $("#openingBalanceGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.opBalanceLoad,
                updateUrl: referenceUrl.opBalanceUpdate
            }),
            columns: [
                {
                    dataField: "fcaAccount",
                    caption: "FCA Account"
                },
                {
                    dataField: "fcaAmount",
                    caption: "FCA Amount"
                },
                {
                    dataField: "account",
                    caption: "Account",
                    allowEditing: false
                },
                {
                    dataField: "amount",
                    caption: "Amount",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "assignedBy",
                    caption: "Assigned By",
                    allowEditing: false,
                    visible: false
                },
                {
                    dataField: "assignedDate",
                    caption: "Assigned Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm a",
                    allowEditing: false,
                    visible: false
                }
            ],
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: false,
                allowAdding: false
            },
            showBorders: true
        }).dxDataGrid("instance");
        
        //#endregion DataGrid

        //#region Events
        

        //#endregion

        //#region Immediate Invocation function
        
        loadData();

        //#endregion
    });
}(window.jQuery, window, document));