(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        
        var $tabpanel,
            $grid,
            $dateSelectionBtn,
            $printBtn,
            $refreshBtn,
            $viewTypeDropdown;

        var referenceUrl = {
            load10amCutOff: window.location.origin + "/api/TenAmDealCutOff/Summary/",
            update10amCutOffClosingBalance: window.location.origin + "/api/TenAmDealCutOff/Summary/ClosingBalance/",
        };

        //#endregion

        //#region Data Source & Functions

        var ds = function (selectedDate, viewType) {
            return DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.load10amCutOff + moment(selectedDate).unix() + "/" + viewType,
                updateUrl: referenceUrl.update10amCutOffClosingBalance + moment(selectedDate).unix()
            });
        };

        function populateData(selectedDate, viewType) {
            $.when(ds(selectedDate, viewType))
                .done(function (data1) {
                    $grid.option("dataSource", data1);
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
                        $grid.option("dataSource", data1);
                        $grid.repaint();

                        app.toast("Data fetched", "info");
                    });
            }
        }).dxDateBox("instance");

        $refreshBtn = $("#refreshBtn").dxButton({
            icon: "refresh",
            onClick: function (e) {
                populateData($dateSelectionBtn.option("value"), $viewTypeDropdown.option("value"));
            }
        }).dxButton("instance");

        $viewTypeDropdown = $("#viewTypeDropdown").dxSelectBox({
            items: ["Approved", "LIVE"],
            value: "Approved",
            onValueChanged: function (data) {
                populateData($dateSelectionBtn.option("value"), data.value);
            }
        }).dxSelectBox("instance");

        /*$closingBalanceHistoryBtn = $("#closingBalanceHistoryBtn").dxButton({
            icon: "fa fa-history",
            text: "Closing Balance",
            onClick: function (e) {
                alert("Clicked history");
            }
        }).dxButton("instance");*/

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
                    isExportAsExcel: (e.itemData.id == 1),
                    viewType: $viewTypeDropdown.option("value")
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
                    dataField: "id",
                    caption: "ID",
                    allowEditing: false,
                    visible: false
                },
                {
                    dataField: "currency",
                    caption: "Currency",
                    groupIndex: 0,
                    allowEditing: false
                },
                {
                    dataField: "account",
                    caption: "Account",
                    allowEditing: false
                },
                {
                    dataField: "openingBalance",
                    caption: "Opening Balance",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
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
                    },
                    allowEditing: false
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
                    },
                    allowEditing: false
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
                    },
                    allowEditing: false
                },
                {
                    dataField: "closingBalance",
                    headerCellTemplate: function (container) {
                        container.append($(
                            "<div><strong>Closing Balance</strong><br/>(EOD)</div>"));
                    },
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: true,
                    cellTemplate: function (container, options) {
                        container.addClass("cell-bg-gray");
                        $("<span>" + options.text + "</span>").appendTo(container);
                    }
                },
                {
                    dataField: "closingBalanceModifiedDate",
                    headerCellTemplate: function (container) {
                        container.append($(
                            "<div><strong>Closing Balance</strong><br/>(Inputted Date)</div>"));
                    },
                    dataType: "datetime",
                    format: "dd/MM/yyyy hh:mm a",
                    allowEditing: false,
                    visible: false
                },
                {
                    dataField: "closingBalanceModifiedBy",
                    headerCellTemplate: function (container) {
                        container.append($(
                            "<div><strong>Closing Balance</strong><br/>(Inputted By)</div>"));
                    },
                    allowEditing: false,
                    visible: false
                }
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
                    },
                    {
                        column: "closingBalance",
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
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: false,
                allowAdding: false
            },
            columnChooser: {
                enabled: true,
                mode: "select"
            }
        }).dxDataGrid("instance");
        
        // #endregion DataGrid

        //#region Events

        

        //#endregion

        //#region Immediate Invocation function

        populateData(new Date(), "Approved");

        //#endregion
    });
}(window.jQuery, window, document));