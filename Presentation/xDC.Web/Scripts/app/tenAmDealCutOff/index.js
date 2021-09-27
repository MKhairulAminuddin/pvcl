(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        
        var $tabpanel,
            $grid,
            $dateSelectionBtn,
            $printBtn,
            $refreshBtn;

        var referenceUrl = {
            load10amCutOff: window.location.origin + "/api/TenAmDealCutOff/Summary/"
        };

        //#endregion

        //#region Data Source & Functions

        var ds = function(selectedDate) {
            return $.ajax({
                url: referenceUrl.load10amCutOff + moment(selectedDate).unix(),
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

                        app.toast("Data fetched", "info");
                    });
            }
        }).dxDateBox("instance");

        $refreshBtn = $("#refreshBtn").dxButton({
            icon: "refresh",
            onClick: function (e) {
                $.when(ds($dateSelectionBtn.option("value")))
                    .done(function (data1) {
                        $grid.option("dataSource", data1.data);
                        $grid.repaint();

                        app.toast("Refreshed", "info");
                    });
            }
        }).dxButton("instance");

        $printBtn = $("#printBtn").dxDropDownButton({
            text: "Print",
            icon: "print",
            type: "normal",
            stylingMode: "contained",
            dropDownOptions: {
                width: 230
            },
            displayExpr: "name",
            keyExpr: "id",
            items: [
                { id: 1, name: "Excel Workbook (*.xlsx)", icon: "fa fa-file-excel-o" },
                { id: 2, name: "PDF", icon: "fa fa-file-pdf-o" }
            ],
            onItemClick: function (e) {
                app.toast("Generating...");

                var data = {
                    SelectedDateEpoch: moment($dateSelectionBtn.option("value")).unix(),
                    isExportAsExcel: (e.itemData.id == 1)
                };

                $.ajax({
                    type: "POST",
                    url: window.location.origin + "/TenAmDealCutOff/Print",
                    data: data,
                    dataType: "text",
                    success: function (data) {
                        var url = window.location.origin + "/TenAmDealCutOff/Printed/" + data;
                        window.location = url;
                    },
                    fail: function (jqXHR, textStatus, errorThrown) {
                        app.alertError(textStatus + ": " + errorThrown);
                    },
                    complete: function (data) {

                    }
                });

                e.event.preventDefault();
            }
        }).dxDropDownButton("instance");
        

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
                    headerCellTemplate: function(container) {
                        container.append(
                            $("<div><strong>Total Inflow</strong><br/>(including Deposit Maturity Only)</div>"));
                    },
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "totalOutflow",
                    headerCellTemplate: function(container) {
                        container.append($(
                            "<div><strong>Total Outflow</strong><br/>(excluding MM investment value = T)</div>"));
                    },
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "net",
                    headerCellTemplate: function(container) {
                        container.append($(
                            "<div><strong>Net</strong><br/>(including Deposit maturity)<br/><small>* available funds for MM investment</small></div>"));
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
                        name: "openingBalance",
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
                        name: "totalInflow",
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
                        name: "totalOutflow",
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
            },
            paging: {
                enabled: false
            },
        }).dxDataGrid("instance");
        
        // #endregion DataGrid

        //#region Events

        

        //#endregion

        //#region Immediate Invocation function

        populateData();

        //#endregion
    });
}(window.jQuery, window, document));