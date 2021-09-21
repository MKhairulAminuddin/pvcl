(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        app.setSideMenuItemActive("/issd/FcaTagging");
        $('[data-toggle="tooltip"]').tooltip();

        var $tabpanel,

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
            getAvailableTrades: window.location.origin + "/api/issd/FcaTagging/AvailableTrades/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            
            feesLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/fees/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            altidLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/altid/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            othersLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/others/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],

            updateTradeItem: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem",


            dsFcaAccount: window.location.origin + "/api/issd/FcaTagging/FcaAccount/"
            
        };

        //#endregion

        //#region Data Source & Functions

        var dsAccountLookup = function () {
            return {
                store: DevExpress.data.AspNet.createStore({
                    key: "name",
                    loadUrl: referenceUrl.dsFcaAccount
                }),
                paginate: true,
                pageSize: 20
            };
        }
        
        var loadData = function () {
            return $.ajax({
                dataType: "json",
                url: referenceUrl.getAvailableTrades,
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
                                    dataField: "inflowTo",
                                    width: "150px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "name",
                                        displayExpr: "name",
                                        allowClearing: true
                                    },
                                    cellTemplate: function (container, options) {
                                        if (options.data.inflowAmount == 0) {
                                            container.addClass("dxDataGrid-cell-grey");
                                        }
                                        $("<span>" + ((options.data.inflowTo == null) ? "" : options.data.inflowTo) + "</span>").appendTo(container);
                                    }
                                },
                                {
                                    dataField: "outflowFrom",
                                    width: "150px",
                                    caption: "Outflow From",
                                    cellTemplate: function (container, options) {
                                        container.addClass("dxDataGrid-cell-grey");
                                        $("<span>" + ((options.data.outflowFrom == null) ? "" : options.data.outflowFrom) + "</span>").appendTo(container);
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
                                    }
                                ]
                            },
                            editing: {
                                mode: "batch",
                                allowUpdating: true,
                                allowDeleting: false,
                                allowAdding: false
                            },
                            onEditorPreparing: function (e) {
                                if (e.parentType === "dataRow" && e.dataField === "inflowTo") {
                                    if (e.row.data.amountPlus == 0) {
                                        e.editorOptions.disabled = true;
                                    }
                                }

                                if (e.parentType === "dataRow" && e.dataField === "outflowFrom") {
                                    if (e.row.data.amountMinus == 0) {
                                        e.editorOptions.disabled = true;
                                    }
                                }
                            },
                            showBorders: true
                        });
                        $feesGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].countPendingFees > 0) {
                    newItem.badge = response.data[0].countPendingFees;
                }
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
                                    dataField: "inflowTo",
                                    width: "150px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "name",
                                        displayExpr: "name",
                                        allowClearing: true
                                    },
                                    cellTemplate: function (container, options) {
                                        if (options.data.inflowAmount == 0) {
                                            container.addClass("dxDataGrid-cell-grey");
                                        }
                                        $("<span>" + ((options.data.inflowTo == null) ? "" : options.data.inflowTo) + "</span>").appendTo(container);
                                    }
                                },
                                {
                                    dataField: "outflowFrom",
                                    width: "150px",
                                    caption: "Outflow From",
                                    cellTemplate: function (container, options) {
                                        container.addClass("dxDataGrid-cell-grey");
                                        $("<span>" + ((options.data.outflowFrom == null) ? "" : options.data.outflowFrom) + "</span>").appendTo(container);
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
                            onEditorPreparing: function (e) {
                                if (e.parentType === "dataRow" && e.dataField === "inflowTo") {
                                    if (e.row.data.amountPlus == 0) {
                                        e.editorOptions.disabled = true;
                                    }
                                }

                                if (e.parentType === "dataRow" && e.dataField === "outflowFrom") {
                                    if (e.row.data.amountMinus == 0) {
                                        e.editorOptions.disabled = true;
                                    }
                                }
                            },
                            showBorders: true
                        });
                        $altidGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].countPendingAltid > 0) {
                    newItem.badge = response.data[0].countPendingAltid;
                }
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
                                    dataField: "inflowTo",
                                    width: "150px",
                                    caption: "Inflow To",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "name",
                                        displayExpr: "name",
                                        allowClearing: true
                                    },
                                    cellTemplate: function (container, options) {
                                        if (options.data.inflowAmount == 0) {
                                            container.addClass("dxDataGrid-cell-grey");
                                        }
                                        $("<span>" + ((options.data.inflowTo == null) ? "" : options.data.inflowTo) + "</span>").appendTo(container);
                                    }
                                },
                                {
                                    dataField: "outflowFrom",
                                    width: "150px",
                                    caption: "Outflow From",
                                    cellTemplate: function (container, options) {
                                        container.addClass("dxDataGrid-cell-grey");
                                        $("<span>" + ((options.data.outflowFrom == null) ? "" : options.data.outflowFrom) + "</span>").appendTo(container);
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
                            onEditorPreparing: function (e) {
                                if (e.parentType === "dataRow" && e.dataField === "inflowTo") {
                                    if (e.row.data.amountPlus == 0) {
                                        e.editorOptions.disabled = true;
                                    }
                                }

                                if (e.parentType === "dataRow" && e.dataField === "outflowFrom") {
                                    if (e.row.data.amountMinus == 0) {
                                        e.editorOptions.disabled = true;
                                    }
                                }
                            },
                            showBorders: true
                        });
                        $othersGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].countPendingOthers > 0) {
                    newItem.badge = response.data[0].countPendingOthers;
                }
                tabPanelItems.push(newItem);
            }
            
            //updateButtonsState(tabPanelItems);

            $tabpanel.option("dataSource", tabPanelItems);
            $tabpanel.option("selectedIndex", tabPanelItems[0]);
        }
        
        //#endregion

        //#region Other Widgets

        $tabpanel = $("#tabpanel").dxTabPanel({
            dataSource: [],
            showNavButtons: true
        }).dxTabPanel("instance");

        $saveChangesBtn = $("#saveChangesBtn").dxButton({
            text: "Save",
            stylingMode: "contained",
            icon: "save",
            onClick: function (e) {
                e.event.preventDefault();

                var tabPanelItems = [];
                tabPanelItems = $tabpanel.option("dataSource");
                $tabpanel.option("selectedIndex", tabPanelItems[0]);
                
                if (typeof ($equityGrid) !== "undefined") {
                    $equityGrid.saveEditData();
                }
                if (typeof ($bondGrid) !== "undefined") {
                    $bondGrid.saveEditData();
                }
                if (typeof ($cpGrid) !== "undefined" ) {
                    $cpGrid.saveEditData();
                }
                if (typeof ($notesPaperGrid) !== "undefined" ) {
                    $notesPaperGrid.saveEditData();
                }
                if (typeof ($repoGrid) !== "undefined") {
                    $repoGrid.saveEditData();
                }
                if (typeof ($couponGrid) !== "undefined") {
                    $couponGrid.saveEditData();
                }
                if (typeof ($feesGrid) !== "undefined") {
                    $feesGrid.saveEditData();
                }
                if (typeof ($mtmGrid) !== "undefined") {
                    $mtmGrid.saveEditData();
                }
                if (typeof ($fxSettlementGrid) !== "undefined") {
                    $fxSettlementGrid.saveEditData();
                }
                if (typeof ($contributionCreditedGrid) !== "undefined") {
                    $contributionCreditedGrid.saveEditData();
                }
                if (typeof ($altidGrid) !== "undefined") {
                    $altidGrid.saveEditData();
                }
                if (typeof ($othersGrid) !== "undefined") {
                    $othersGrid.saveEditData();
                }

                loadData();
                app.toast("Saved", "Success");
            }
        }).dxButton("instance");
        
        //#endregion

        //#region DataGrid
        
        //#endregion DataGrid

        //#region Events
        

        //#endregion

        //#region Immediate Invocation function
        
        loadData();

        //#endregion
    });
}(window.jQuery, window, document));