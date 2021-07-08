
var dxGridUtils = (function () {
    var commonMainGridConfig = {
        showRowLines: true,
        rowAlternationEnabled: false,
        showBorders: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        sorting: {
            mode: "multiple",
            showSortIndexes: true
        },
        columnChooser: {
            enabled: true,
            mode: "select"
        },
        columnFixing: {
            enabled: true
        },
        searchPanel: {
            visible: true
        },
        grouping: {
            contextMenuEnabled: true,
            autoExpandAll: false
        },
        groupPanel: {
            visible: true
        },
        headerFilter: { visible: true },
        pager: {
            infoText: "Page {0} of {1} ({2} items)",
            showPageSizeSelector: true,
            allowedPageSizes: [10, 20, 50],
            showNavigationButtons: true,
            showInfo: true
        },
        paging: {
            pageSize: 20,
            pageIndex: 0
        },
        "export": {
            enabled: true
        },
        wordWrapEnabled: true,
        onToolbarPreparing: function (gridElem) {

            gridElem.toolbarOptions.items.unshift(
                {
                    location: "after",
                    widget: "dxButton",
                    options: {
                        icon: "expand",
                        hint: "Expand Grouped Rows",
                        onClick: function (btnElem) {
                            gridElem.component.expandAll();
                        }
                    }
                },
                {
                    location: "after",
                    widget: "dxButton",
                    options: {
                        icon: "collapse",
                        hint: "Collapse Grouped Rows",
                        onClick: function (btnElem) {
                            gridElem.component.collapseAll();
                        }
                    }
                },
                {
                    location: "after",
                    widget: "dxButton",
                    options: {
                        icon: "refresh",
                        hint: "Revert Columns Position",
                        onClick: function (btnElem) {
                            gridElem.component.state({});
                        }
                    }
                },
                {
                    location: "after",
                    widget: "dxButton",
                    options: {
                        icon: "clearformat",
                        hint: "Clear Grouping",
                        onClick: function (btnElem) {
                            gridElem.component.clearGrouping();
                        }
                    }
                }
            );
        }
    };

    var commonSummaryGridConfig = {
        showRowLines: true,
        rowAlternationEnabled: false,
        showBorders: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        sorting: {
            mode: "multiple"
        },
        columnFixing: {
            enabled: true
        },
        columnChooser: {
            enabled: true,
            mode: "select"
        },
        headerFilter: { visible: false },
        "export": {
            enabled: true
        }
    };

    var editingGridConfig = {
        showRowLines: true,
        rowAlternationEnabled: false,
        showBorders: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        editing: {
            mode: "batch",
            allowUpdating: true,
            allowDeleting: true,
            allowAdding: true
        },
        sorting: {
            mode: "multiple",
            showSortIndexes: true
        },
        groupPanel: {
            visible: false
        },
        headerFilter: { visible: true },
        pager: {
            infoText: "Page {0} of {1} ({2} items)",
            showPageSizeSelector: true,
            allowedPageSizes: [10, 20, 50],
            showNavigationButtons: true,
            showInfo: true
        },
        paging: {
            pageSize: 20,
            pageIndex: 0
        },
        wordWrapEnabled: true,
        onToolbarPreparing: function (gridElem) {

            gridElem.toolbarOptions.items.unshift(
                {
                    location: "after",
                    widget: "dxButton",
                    options: {
                        icon: "refresh",
                        hint: "Clear Grid la",
                        onClick: function (btnElem) {
                            gridElem.component.state({});
                        }
                    }
                }
            );
        },
        onCellPrepared: function (e) {
            if (e.rowType === 'header') {
                e.cellElement.css("backgroundColor", "#5B8EFB");
                e.cellElement.css("color", "white");
            }
        }
    };

    var viewOnlyGridConfig = {
        showRowLines: true,
        rowAlternationEnabled: false,
        showBorders: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        sorting: {
            mode: "multiple",
            showSortIndexes: true
        },
        columnFixing: {
            enabled: true
        },
        headerFilter: { visible: true },
        pager: {
            infoText: "Page {0} of {1} ({2} items)",
            showPageSizeSelector: true,
            allowedPageSizes: [10, 20, 50],
            showNavigationButtons: true,
            showInfo: true
        },
        paging: {
            pageSize: 10,
            pageIndex: 0
        },
        wordWrapEnabled: true
    };
    

    var commonFileTitle = function (reportName, isDownload) {
        if (isDownload) {
            return moment().format('YYMMDDHHmm') + " - " + reportName;
        } else {
            return reportName;
        }
    };

    var clearGrid = function (dxElem) {
        dxElem.option("dataSource", []);
    };

    function x(worksheet, grid, i) {
        return DevExpress.excelExporter.exportDataGrid({
            worksheet: worksheet,
            component: grid,
            topLeftCell: { row: 5, column: 1 }
        });
    }

    function addWorksheet(workbook, i) {
        workbook.addWorksheet("Grid " + (i + 1));
    }

    function gridTitle(worksheet, title) {
        worksheet.getRow(2).getCell(1).value = title;
        worksheet.getRow(2).getCell(1).font = { bold: true, size: 14 };

        worksheet.getRow(3).getCell(1).value = "Exported on: " + moment().format('DD/MM/YYYY HH:mm a');
        worksheet.getRow(3).getCell(1).font = { italic: true, size: 10 };
    }

    var exportGrid = function (request) {

        var workbook = new ExcelJS.Workbook();

        var exportPromise = DevExpress.excelExporter.exportDataGrid({
            worksheet: workbook.addWorksheet("Grid 1"),
            component: request.grids[0],
            topLeftCell: { row: 5, column: 1 }
        });

        exportPromise = exportPromise.then(() => {
            if (request.grids.length > 1) {
                for (var i = 1; i < request.grids.length; i++) {
                    addWorksheet(workbook, i);
                }
            }
        });

        exportPromise = exportPromise.then(() => {
            for (var j = 0; j < request.gridsTitle.length; j++) {
                gridTitle(workbook.worksheets[j], request.gridsTitle[j], j);
            }
        });

        for (let k = 1; k < request.grids.length; k++) {
            exportPromise = exportPromise.then(function () {
                return x(workbook.worksheets[k], request.grids[k], k);
            });
        }

        exportPromise = exportPromise.then(function () {
            workbook.xlsx.writeBuffer().then(function (buffer) {
                saveAs(new Blob([buffer], { type: "application/octet-stream" }),
                    request.fileName + ".xlsx");
            });
        });

        return exportPromise;

    }
    
    return {
        commonFileTitle: commonFileTitle,
        clearGrid: clearGrid,
        exportGrid: exportGrid,


        commonMainGridConfig: commonMainGridConfig,
        commonSummaryGridConfig: commonSummaryGridConfig,
        editingGridConfig: editingGridConfig,
        viewOnlyGridConfig: viewOnlyGridConfig
    };
})();