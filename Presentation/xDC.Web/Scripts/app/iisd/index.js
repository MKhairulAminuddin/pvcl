(function ($, window, document) {

    $(function () {

        var $iisdGrid;
        
        $iisdGrid = $("#iisdGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/Iisd/GetIisdForms"
            }),
            columns: [
                {
                    dataField: "id",
                    caption: "Form ID"
                },
                {
                    dataField: "formType",
                    caption: "Form Type"
                },
                {
                    dataField: "preparedBy",
                    caption: "Preparer"
                },
                {
                    dataField: "preparedDate",
                    caption: "Prepared Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm"
                },
                {
                    dataField: "approvedBy",
                    caption: "Approved By"
                },
                {
                    dataField: "approvedDate",
                    caption: "Approved Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm"
                },
                {
                    dataField: "formStatus",
                    caption: "Status"
                },
                {
                    caption: "",
                    width: "100px"
                }
            ],
            editing: {
                mode: "row",
                allowUpdating: false,
                allowDeleting: false,
                allowAdding: false
            }
        }).dxDataGrid("instance");

        $iisdGrid.option(dxGridUtils.viewOnlyGridConfig);


    });
}(window.jQuery, window, document));