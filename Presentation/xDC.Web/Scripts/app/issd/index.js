(function ($, window, document) {

    $(function () {

        var $issdGrid;
        
        $issdGrid = $("#issdGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: window.location.origin + "/api/Issd/GetIssdForm"
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
                                window.location.href = "/issd/TradeSettlement/Edit?id=" + e.row.data.id;
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
                                window.location.href = "/issd/TradeSettlement/View?id=" + e.row.data.id;
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
                                    url: '/issd/printForm',
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        var url = '/issd/viewPrintedForm?id=' + data;
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

        $issdGrid.option(dxGridUtils.viewOnlyGridConfig);


    });
}(window.jQuery, window, document));