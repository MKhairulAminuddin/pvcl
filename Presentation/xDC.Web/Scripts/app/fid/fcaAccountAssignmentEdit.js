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
        
        //#endregion

        //#region Other Widgets

        $saveChangesBtn = $("#saveChangesBtn").dxButton({
            text: "Save Changes",
            stylingMode: "contained",
            type: "success",
            icon: "save"
        }).dxButton("instance");

        //#endregion

        // #region DataGrid

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

        $equityGrid = $("#equityGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.equityLoad,
                updateUrl: referenceUrl.updateTradeItem
            }),
            columns: [
                {
                    dataField: "inflowTo",
                    caption: "Inflow To",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
                    }
                },
                {
                    dataField: "outflowFrom",
                    caption: "Outflow From",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
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
        }).dxDataGrid("instance");
        
        $bondGrid = $("#bondGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.bondLoad,
                updateUrl: referenceUrl.updateTradeItem
            }),
            columns: [
                {
                    dataField: "inflowTo",
                    caption: "Inflow To",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
                    }
                },
                {
                    dataField: "outflowFrom",
                    caption: "Outflow From",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
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
        }).dxDataGrid("instance");

        $cpGrid = $("#cpGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.cpLoad,
                updateUrl: referenceUrl.updateTradeItem
            }),
            columns: [
                {
                    dataField: "inflowTo",
                    caption: "Inflow To",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
                    }
                },
                {
                    dataField: "outflowFrom",
                    caption: "Outflow From",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
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
        }).dxDataGrid("instance");

        $notesPaperGrid = $("#notesPaperGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.notesPaperLoad,
                updateUrl: referenceUrl.updateTradeItem
            }),
            columns: [
                {
                    dataField: "inflowTo",
                    caption: "Inflow To",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
                    }
                },
                {
                    dataField: "outflowFrom",
                    caption: "Outflow From",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
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
        }).dxDataGrid("instance");

        $repoGrid = $("#repoGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.repoLoad,
                updateUrl: referenceUrl.updateTradeItem
            }),
            columns: [
                {
                    dataField: "inflowTo",
                    caption: "Inflow To",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
                    }
                },
                {
                    dataField: "outflowFrom",
                    caption: "Outflow From",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
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
        }).dxDataGrid("instance");

        $couponGrid = $("#couponGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.couponLoad,
                updateUrl: referenceUrl.updateTradeItem
            }),
            columns: [
                {
                    dataField: "inflowTo",
                    caption: "Inflow To",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
                    }
                },
                {
                    dataField: "outflowFrom",
                    caption: "Outflow From",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
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
        }).dxDataGrid("instance");

        $feesGrid = $("#feesGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.feesLoad,
                updateUrl: referenceUrl.updateTradeItem
            }),
            columns: [
                {
                    dataField: "inflowTo",
                    caption: "Inflow To",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
                    }
                },
                {
                    dataField: "outflowFrom",
                    caption: "Outflow From",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
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
        }).dxDataGrid("instance");

        $mtmGrid = $("#mtmGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.mtmLoad,
                updateUrl: referenceUrl.updateTradeItem
            }),
            columns: [
                {
                    dataField: "inflowTo",
                    caption: "Inflow To",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
                    }
                },
                {
                    dataField: "outflowFrom",
                    caption: "Outflow From",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
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
        }).dxDataGrid("instance");

        $fxSettlementGrid = $("#fxSettlementGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.fxLoad,
                updateUrl: referenceUrl.updateTradeItem
            }),
            columns: [
                {
                    dataField: "inflowTo",
                    caption: "Inflow To",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
                    }
                },
                {
                    dataField: "outflowFrom",
                    caption: "Outflow From",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
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
        }).dxDataGrid("instance");

        $contributionCreditedGrid = $("#contributionCreditedGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.contributionLoad,
                updateUrl: referenceUrl.updateTradeItem
            }),
            columns: [
                {
                    dataField: "inflowTo",
                    caption: "Inflow To",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
                    }
                },
                {
                    dataField: "outflowFrom",
                    caption: "Outflow From",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
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
        }).dxDataGrid("instance");

        $altidGrid = $("#altidGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.altidLoad,
                updateUrl: referenceUrl.updateTradeItem
            }),
            columns: [
                {
                    dataField: "inflowTo",
                    caption: "Inflow To",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
                    }
                },
                {
                    dataField: "outflowFrom",
                    caption: "Outflow From",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
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
        }).dxDataGrid("instance");

        $othersGrid = $("#othersGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.othersLoad,
                updateUrl: referenceUrl.updateTradeItem
            }),
            columns: [
                {
                    dataField: "inflowTo",
                    caption: "Inflow To",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
                    }
                },
                {
                    dataField: "outflowFrom",
                    caption: "Outflow From",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "account",
                        displayExpr: "account"
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
        }).dxDataGrid("instance");


        // #endregion DataGrid

        //#region Events

        $saveChangesBtn.on({
            "click": function (e) {
                e.event.preventDefault();

                $openingBalanceGrid.saveEditData();

                $equityGrid.saveEditData();
                $bondGrid.saveEditData();
                $cpGrid.saveEditData();
                $notesPaperGrid.saveEditData();
                $repoGrid.saveEditData();
                $couponGrid.saveEditData();
                $feesGrid.saveEditData();
                $mtmGrid.saveEditData();
                $fxSettlementGrid.saveEditData();
                $contributionCreditedGrid.saveEditData();
                $altidGrid.saveEditData();
                $othersGrid.saveEditData();

                
            }
        });

        //#endregion

        //#region Immediate Invocation function


        //#endregion
    });
}(window.jQuery, window, document));