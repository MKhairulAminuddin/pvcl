(function ($, window, document) {

    $(function () {

        var $dropdownConfigGrid;


        $dropdownConfigGrid = $("#applicationConfigGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/setting/GetApplicationConfig",
                insertUrl: "../api/setting/InsertApplicationConfig",
                updateUrl: "../api/setting/UpdateApplicationConfig",
                deleteUrl: "../api/setting/DeleteApplicationConfig"
            }),
            columns: [
                {
                    dataField: "key",
                    caption: "Key"
                },
                {
                    dataField: "value",
                    caption: "Value"
                },
                {
                    dataField: "createdBy",
                    caption: "Created By",
                    visible: false,
                    allowEditing: false
                },
                {
                    dataField: "createdDate",
                    caption: "Created Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm",
                    visible: false,
                    allowEditing: false
                },
                {
                    dataField: "updatedBy",
                    caption: "Updated By",
                    visible: false,
                    allowEditing: false
                },
                {
                    dataField: "updatedDate",
                    caption: "Updated Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm",
                    visible: false,
                    allowEditing: false
                }
            ],
            showBorders: true,
            editing: {
                mode: "batch",
                allowAdding: true,
                allowDeleting: true,
                allowUpdating: true,
                selectTextOnEditStart: true,
                startEditAction: "click"
            }
        }).dxDataGrid("instance");

        $dropdownConfigGrid.option(dxGridUtils.commonMainGridConfig);

    });
}(window.jQuery, window, document));