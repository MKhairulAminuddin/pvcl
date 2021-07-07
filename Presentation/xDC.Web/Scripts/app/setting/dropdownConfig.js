(function ($, window, document) {

    $(function () {
        var dropdownConfigKeyStore = DevExpress.data.AspNet.createStore({
            key: "key",
            loadUrl: "../api/common/GetConfigDropdownKey"
        });

        var $dropdownConfigGrid;


        $dropdownConfigGrid = $("#dropdownConfigGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/setting/GetDropdownConfig",
                insertUrl: "../api/setting/InsertDropdownConfig",
                updateUrl: "../api/setting/UpdateDropdownConfig",
                deleteUrl: "../api/setting/DeleteDropdownConfig"
            }),
            columns: [
                {
                    dataField: "key",
                    caption: "Key",
                    lookup: {
                        dataSource: dropdownConfigKeyStore,
                        valueExpr: "key",
                        displayExpr: "key"
                    }
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