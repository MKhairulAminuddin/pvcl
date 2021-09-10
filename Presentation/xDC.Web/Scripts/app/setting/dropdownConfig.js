(function ($, window, document) {

    $(function () {
        var dropdownConfigKeyStore = DevExpress.data.AspNet.createStore({
            key: "key",
            loadUrl: window.location.origin + "/api/common/GetConfigDropdownKey"
        });

        var $dropdownConfigGrid;


        $dropdownConfigGrid = $("#dropdownConfigGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: window.location.origin + "/api/setting/GetDropdownConfig",
                insertUrl: window.location.origin + "/api/setting/InsertDropdownConfig",
                updateUrl: window.location.origin + "/api/setting/UpdateDropdownConfig",
                deleteUrl: window.location.origin + "/api/setting/DeleteDropdownConfig"
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