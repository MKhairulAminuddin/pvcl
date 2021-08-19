(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        
        var $tabpanel,
            $grid,
            $dateSelectionBtn,
            $printBtn;

        var referenceUrl = {
            adminEdit: window.location.origin + "/issd/TradeSettlement/PartA/Edit/",

            submitApprovalRequest: window.location.origin + "/api/issd/TradeSettlement/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/PartA/View/"
        };

        //#endregion

        //#region Data Source & Functions

        var ds = function(selectedDate) {
            return $.ajax({
                url: window.location.origin + "/api/fid/10AmCutOff/" + moment(selectedDate).unix(),
                type: "get"
            });
        };

        function populateData() {
            $.when(ds(new Date()))
                .done(function (data1) {
                    $grid.option("dataSource", data1.data);
                    $grid.repaint();
                });
        }

        //#endregion

        //#region Other Widgets

        $dateSelectionBtn = $("#dateSelectionBtn").dxDateBox({
            type: "date",
            displayFormat: "dd/MM/yyyy",
            value: new Date(),
            onValueChanged: function (data) {
                $.when(ds(data.value))
                    .done(function (data1) {
                        $grid.option("dataSource", data1.data);
                        $grid.repaint();
                    });
            }
        }).dxDateBox("instance");
        

        //#endregion

        // #region DataGrid
        
        $grid = $("#grid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "currency",
                    caption: "Currency",
                    groupIndex: 0
                },
                {
                    dataField: "account",
                    caption: "Account"
                },
                {
                    dataField: "openingBalance",
                    caption: "Opening Balance",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "totalInflow",
                    headerCellTemplate: function (container) {
                        container.append($("<div><strong>Total Inflow</strong><br/>(including Deposit Maturity Only)</div>"));
                    },
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "totalOutflow",
                    headerCellTemplate: function (container) {
                        container.append($("<div><strong>Total Outflow</strong><br/>(excluding MM investment value = T)</div>"));
                    },
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "net",
                    headerCellTemplate: function (container) {
                        container.append($("<div><strong>Net</strong><br/>(including Deposit maturity)<br/><small>* available funds for MM investment</small></div>"));
                    },
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
            ],
            summary: {
                groupItems: [
                    {
                        column: "openingBalance",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        },
                        showInGroupFooter: true
                    },
                    {
                        column: "totalInflow",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        },
                        showInGroupFooter: true
                    },
                    {
                        column: "totalOutflow",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        },
                        showInGroupFooter: true
                    },
                    {
                        column: "net",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        },
                        showInGroupFooter: true
                    }
                ]
            },
            showBorders: true,
            grouping: {
                autoExpandAll: true,
            }
        }).dxDataGrid("instance");
        
        // #endregion DataGrid

        //#region Events

        

        //#endregion

        //#region Immediate Invocation function

        populateData();

        //#endregion
    });
}(window.jQuery, window, document));