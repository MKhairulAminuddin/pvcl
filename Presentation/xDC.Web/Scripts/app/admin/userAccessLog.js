$(function () {
    var $grid1;

    $grid1 = $("#grid1").dxDataGrid({
        dataSource: window.location.origin + "/api/admin/userAccessLog",
        selection: { mode: "single" },
        editing: {
            enabled: false
        },
        columns: [
            {
                caption: "Username",
                dataField: "userName"
            },
            {
                caption: "IP Address",
                dataField: "clientAddress"
            },
            {
                caption: "Browser",
                dataField: "clientBrowser"
            },
            {
                caption: "Recorded Date",
                dataField: "recordedDate",
                dataType: "date",
                format: "dd/MM/yyyy HH:mm:ss",
                sortIndex: 1,
                sortOrder: "desc"
            }
        ],
        headerFilter: {
            visible: true,
            allowSearch: true
        },
        searchPanel: {
            visible: true
        },
        export: {
            enabled: true
        },
    }).dxDataGrid('instance');

    $grid1.option(dxGridUtils.viewOnlyGridConfig);
});