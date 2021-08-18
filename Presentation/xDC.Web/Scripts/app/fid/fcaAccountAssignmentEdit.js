(function ($, window, document) {
    $(function () {
        //#region Variable Definition

        var $tabpanel,
            $equityGrid,


            $printBtn;

        var referenceUrl = {
            equityLoad: window.location.origin + "/api/fid/Ts10AmAccountAssignmentGrid/tradeItem/equity/" + window.location.pathname.split("/").pop(),

            update: window.location.origin + "/api/fid/Ts10AmAccountAssignment/tradeItem"
        };

        //#endregion

        //#region Data Source & Functions
        
        //#endregion

        //#region Other Widgets



        //#endregion

        // #region DataGrid


        $equityGrid = $("#equityGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.equityLoad,
                updateUrl: referenceUrl.update
            }),
            columns: [
                {
                    dataField: "inflowTo",
                    caption: "Inflow To"
                },
                {
                    dataField: "outflowFrom",
                    caption: "Outflow From"
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
                    allowEditing: false
                },
                {
                    dataField: "assignedDate",
                    caption: "Assigned Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm a",
                    allowEditing: false
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
            }
        }).dxDataGrid("instance");


        // #endregion DataGrid

        //#region Events


        //#endregion

        //#region Immediate Invocation function


        //#endregion
    });
}(window.jQuery, window, document));