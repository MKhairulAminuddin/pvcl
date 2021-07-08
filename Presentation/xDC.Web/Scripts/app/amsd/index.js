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
                    alignment: "left",
                    width: 100
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
                    caption: "Approver"
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
                            hint: "Edit Draft",
                            icon: "fa fa-pencil-square",
                            cssClass: "dx-datagrid-command-btn",
                            visible: function (e) {
                                return (e.row.data.formStatus == "Draft" && e.row.data.isFormOwner);
                            },
                            onClick: function (e) {
                                window.location.href = "/amsd/EditInflowFundsForm?id=" + e.row.data.id;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "View Form",
                            icon: "fa fa-eye",
                            cssClass: "dx-datagrid-command-btn",
                            visible: function (e) {
                                return (e.row.data.formStatus != "Draft");
                            },
                            onClick: function (e) {
                                window.location.href = "/amsd/InflowFundsFormStatus?id=" + e.row.data.id;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Print Form",
                            icon: "fa fa-print",
                            cssClass: "dx-datagrid-command-btn",
                            visible: function (e) {
                                return (e.row.data.formStatus != "Draft");
                            },
                            onClick: function (e) {
                                var data = {
                                    id: e.row.data.id
                                };

                                $.ajax({
                                    type: "POST",
                                    url: '/amsd/PrintInflowFund',
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        var url = '/amsd/GetPrintInflowFund?id=' + data;
                                        window.location = url;
                                    }
                                });
                                e.event.preventDefault();
                            }
                        }
                    ]
                }
            ],
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