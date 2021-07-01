(function ($, window, document) {

    $(function () {

        var $amsdGrid;
        

        $amsdGrid = $("#amsdGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/amsd/GetAmsdForms"
            }),
            columns: [
                {
                    dataField: "id",
                    caption: "Form ID",
                    alignment: "left"
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
                    caption: "Actions",
                    type: "buttons",
                    width: 110,
                    buttons: [
                        {
                            hint: "View Form",
                            icon: "fa fa-external-link",
                            onClick: function (e) {
                                window.location.href = "/amsd/InflowFundsFormStatus?id=" + e.row.data.id;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Print Form",
                            icon: "fa fa-print",
                            onClick: function (e) {
                                alert("Development in Progress....");
                                e.event.preventDefault();
                            }
                        }
                    ]
                }
            ],
            showBorders: true,
            height: 300,
            editing: {
                mode: "row",
                allowUpdating: false,
                allowDeleting: false,
                allowAdding: false
            }
        }).dxDataGrid("instance");

        $amsdGrid.option(dxGridUtils.viewOnlyGridConfig);

    });
}(window.jQuery, window, document));