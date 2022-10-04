(function ($, window, document) {
    $(function () {
        //#region Variable Definition
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

            equityLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/equity/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            bondLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/bond/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            cpLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/cp/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            notesPaperLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/notesPaper/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            repoLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/repo/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            couponLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/coupon/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            feesLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/fees/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            mtmLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/mtm/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            fxLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/fxSettlement/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            contributionLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/contributionCredited/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            altidLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/altid/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],
            othersLoad: window.location.origin + "/api/issd/FcaTaggingGrid/tradeItem/others/" + window.location.pathname.split("/").slice(-2)[0] + "/" + window.location.pathname.split("/").slice(-2)[1],

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

            if (response.data[0].equity) {
                var newItem = {
                    title: "Equity",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='equityGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.equityLoad
                            }),
                            columns: [
                                {
                                    caption: "Inflow To",
                                    dataField: "inflowTo",
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
                                    },
                                    width: "150px",
                                },
                                {
                                    dataField: "outflowFrom",
                                    width: "150px",
                                    caption: "Outflow From",
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "name",
                                        displayExpr: "name",
                                        allowClearing: true
                                    },
                                    cellTemplate: function (container, options) {
                                        if (options.data.outflowAmount == 0) {
                                            container.addClass("dxDataGrid-cell-grey");
                                        }
                                        $("<span>" + ((options.data.outflowFrom == null) ? "" : options.data.outflowFrom) + "</span>").appendTo(container);
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
                            showBorders: true
                        });
                        $equityGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].countPendingEquity > 0) {
                    newItem.badge = response.data[0].countPendingEquity;
                }
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
                                loadUrl: referenceUrl.bondLoad
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
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "name",
                                        displayExpr: "name",
                                        allowClearing: true
                                    },
                                    cellTemplate: function (container, options) {
                                        if (options.data.outflowFrom == 0) {
                                            container.addClass("dxDataGrid-cell-grey");
                                        }
                                        $("<span>" + ((options.data.outflowFrom == null) ? "" : options.data.outflowFrom) + "</span>").appendTo(container);
                                    }
                                },
                                {
                                    dataField: "bondType",
                                    caption: "Bond Type",
                                    lookup: {
                                        dataSource: ["GOV", "CORP"]
                                    },
                                    validationRules: [{ type: 'required' }]
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
                            showBorders: true
                        });
                        $bondGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].countPendingBond > 0) {
                    newItem.badge = response.data[0].countPendingBond;
                }
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
                                loadUrl: referenceUrl.cpLoad
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
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "name",
                                        displayExpr: "name",
                                        allowClearing: true
                                    },
                                    cellTemplate: function (container, options) {
                                        if (options.data.outflowFrom == 0) {
                                            container.addClass("dxDataGrid-cell-grey");
                                        }
                                        $("<span>" + ((options.data.outflowFrom == null) ? "" : options.data.outflowFrom) + "</span>").appendTo(container);
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
                            showBorders: true
                        });
                        $cpGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].countPendingCp > 0) {
                    newItem.badge = response.data[0].countPendingCp;
                }
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
                                loadUrl: referenceUrl.notesPaperLoad
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
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "name",
                                        displayExpr: "name",
                                        allowClearing: true
                                    },
                                    cellTemplate: function (container, options) {
                                        if (options.data.outflowFrom == 0) {
                                            container.addClass("dxDataGrid-cell-grey");
                                        }
                                        $("<span>" + ((options.data.outflowFrom == null) ? "" : options.data.outflowFrom) + "</span>").appendTo(container);
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
                            showBorders: true
                        });
                        $notesPaperGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].countPendingNotesPapers > 0) {
                    newItem.badge = response.data[0].countPendingNotesPapers;
                }
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
                                loadUrl: referenceUrl.repoLoad
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
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "name",
                                        displayExpr: "name",
                                        allowClearing: true
                                    },
                                    cellTemplate: function (container, options) {
                                        if (options.data.outflowAmount == 0) {
                                            container.addClass("dxDataGrid-cell-grey");
                                        }
                                        $("<span>" + ((options.data.outflowFrom == null) ? "" : options.data.outflowFrom) + "</span>").appendTo(container);
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
                            showBorders: true
                        });
                        $repoGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].countPendingRepo > 0) {
                    newItem.badge = response.data[0].countPendingRepo;
                }
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
                                loadUrl: referenceUrl.couponLoad
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
                                    dataField: "couponType",
                                    caption: "Coupon Type",
                                    lookup: {
                                        dataSource: ["GOV", "CORP"]
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
                            showBorders: true
                        });
                        $couponGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].countPendingCoupon > 0) {
                    newItem.badge = response.data[0].countPendingCoupon;
                }
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
                                loadUrl: referenceUrl.feesLoad
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
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "name",
                                        displayExpr: "name",
                                        allowClearing: true
                                    },
                                    cellTemplate: function (container, options) {
                                        if (options.data.outflowAmount == 0) {
                                            container.addClass("dxDataGrid-cell-grey");
                                        }
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

            if (response.data[0].mtm) {
                var newItem = {
                    title: "Receipts/Payments (MTM)",
                    template: function (itemData, itemIndex, element) {
                        const newTabView = $("<div id='mtmGrid' class='grid-container-tabview' style='width: 100%'/>")
                        newTabView.dxDataGrid({
                            dataSource: DevExpress.data.AspNet.createStore({
                                key: "id",
                                loadUrl: referenceUrl.mtmLoad
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
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "name",
                                        displayExpr: "name",
                                        allowClearing: true
                                    },
                                    cellTemplate: function (container, options) {
                                        if (options.data.outflowAmount == 0) {
                                            container.addClass("dxDataGrid-cell-grey");
                                        }
                                        $("<span>" + ((options.data.outflowFrom == null) ? "" : options.data.outflowFrom) + "</span>").appendTo(container);
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
                            showBorders: true
                        });
                        $mtmGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].countPendingMtm > 0) {
                    newItem.badge = response.data[0].countPendingMtm;
                }
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
                                loadUrl: referenceUrl.fxLoad
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
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "name",
                                        displayExpr: "name",
                                        allowClearing: true
                                    },
                                    cellTemplate: function (container, options) {
                                        if (options.data.outflowAmount == 0) {
                                            container.addClass("dxDataGrid-cell-grey");
                                        }
                                        $("<span>" + ((options.data.outflowFrom == null) ? "" : options.data.outflowFrom) + "</span>").appendTo(container);
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
                            showBorders: true
                        });
                        $fxSettlementGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].countPendingFx > 0) {
                    newItem.badge = response.data[0].countPendingFx;
                }
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
                                loadUrl: referenceUrl.contributionLoad
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
                            showBorders: true
                        });
                        $contributionCreditedGrid = newTabView.dxDataGrid("instance");
                        newTabView.appendTo(element);
                    }
                };
                if (response.data[0].countPendingContribution > 0) {
                    newItem.badge = response.data[0].countPendingContribution;
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
                                loadUrl: referenceUrl.altidLoad
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
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "name",
                                        displayExpr: "name",
                                        allowClearing: true
                                    },
                                    cellTemplate: function (container, options) {
                                        if (options.data.outflowAmount == 0) {
                                            container.addClass("dxDataGrid-cell-grey");
                                        }
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
                                loadUrl: referenceUrl.othersLoad
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
                                    lookup: {
                                        dataSource: dsAccountLookup,
                                        valueExpr: "name",
                                        displayExpr: "name",
                                        allowClearing: true
                                    },
                                    cellTemplate: function (container, options) {
                                        if (options.data.outflowAmount == 0) {
                                            container.addClass("dxDataGrid-cell-grey");
                                        }
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