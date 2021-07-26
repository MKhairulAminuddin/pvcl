﻿(function ($, window, document) {

    $(function () {

        var $amsdGrid, $newInflowFundBtn, $loadPanel;

        $newInflowFundBtn = $("#newInflowFundBtn").dxButton({
            text: "New Inflow Funds",
            type: "default",
            stylingMode: "contained",
            onClick: function(e) {
                $.ajax({
                    dataType: 'json',
                    url: window.location.origin + '/api/amsd/IsTodayInflowFormExisted',
                    method: 'get'
                }).done(function (data) {
                    if (data) {
                        $("#error_container").bs_warning("Today's form already existed.");
                    } else {
                        window.location = window.location.origin + "/amsd/inflowfund/new";
                    }
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                });
            } 
        }).dxButton("instance");
        
        $amsdGrid = $("#amsdGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: window.location.origin + "/api/amsd/GetAmsdForms"
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
                    format: "dd/MM/yyyy HH:mm",
                    sortOrder: "desc"
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
                                window.location.href = "/amsd/inflowfund/edit?id=" + e.row.data.id;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Delete Draft",
                            icon: "fa fa-trash-o",
                            cssClass: "dx-datagrid-command-btn text-red",
                            visible: function (e) {
                                return (e.row.data.formStatus == "Draft" && e.row.data.isFormOwner);
                            },
                            onClick: function (e) {
                                if (!confirm("Do you really want to delete this?")) {
                                    return false;
                                } else {
                                    var data = {
                                        id: e.row.data.id
                                    };

                                    $.ajax({
                                        type: "delete",
                                        url: window.location.origin + '/api/amsd/DeleteInflowFundDraftForm',
                                        data: data,
                                        success: function (data) {
                                            $("#error_container").bs_success("Draft deleted");
                                            $amsdGrid.refresh();
                                        },
                                        fail: function (jqXHR, textStatus, errorThrown) {
                                            $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                                        }
                                    });
                                    e.event.preventDefault();
                                }
                            }
                        },
                        {
                            hint: "Resubmit",
                            icon: "fa fa-repeat",
                            cssClass: "dx-datagrid-command-btn",
                            visible: function (e) {
                                return (e.row.data.isResubmitEnabled);
                            },
                            onClick: function (e) {
                                window.location.href = "/amsd/inflowfund/edit?id=" + e.row.data.id;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Admin Edit",
                            icon: "fa fa-pencil-square-o",
                            cssClass: "dx-datagrid-command-btn text-red",
                            visible: function (e) {
                                return (e.row.data.isCanAdminEdit);
                            },
                            onClick: function (e) {
                                window.location.href = "/amsd/inflowfund/edit?id=" + e.row.data.id;
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
                                window.location.href = "/amsd/InflowFund/View?id=" + e.row.data.id;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Download as Excel",
                            icon: "fa fa-file-excel-o",
                            cssClass: "dx-datagrid-command-btn text-green",
                            visible: function (e) {
                                return (e.row.data.formStatus != "Draft");
                            },
                            onClick: function (e) {
                                $loadPanel.option("position", { of: "#amsdGrid" });
                                $loadPanel.show();

                                var data = {
                                    id: e.row.data.id,
                                    isExportAsExcel: true
                                };

                                $.ajax({
                                    type: "POST",
                                    url: '/amsd/PrintInflowFund',
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        var url = '/amsd/GetPrintInflowFund?id=' + data;
                                        window.location = url;
                                    },
                                    fail: function (jqXHR, textStatus, errorThrown) {
                                        $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                                    },
                                    complete:function(data) {
                                        $loadPanel.hide();
                                    }
                                });
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Download as PDF",
                            icon: "fa fa-file-pdf-o",
                            cssClass: "dx-datagrid-command-btn text-orange",
                            visible: function (e) {
                                return (e.row.data.formStatus != "Draft");
                            },
                            onClick: function (e) {
                                $loadPanel.option("position", { of: "#amsdGrid" });
                                $loadPanel.show();

                                var data = {
                                    id: e.row.data.id,
                                    isExportAsExcel: false
                                };

                                $.ajax({
                                    type: "POST",
                                    url: '/amsd/PrintInflowFund',
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        var url = '/amsd/GetPrintInflowFund?id=' + data;
                                        window.location = url;
                                    },
                                    fail: function (jqXHR, textStatus, errorThrown) {
                                        $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                                    },
                                    complete: function (data) {
                                        $loadPanel.hide();
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
            },
            onRowPrepared: function (e) {
                if (e.rowType == "data") {
                    if (e.data.isFormPendingMyApproval) {
                        e.rowElement.css("background-color", "#FFEBEE");
                    } 
                    if (e.data.isMyFormRejected) {
                        e.rowElement.css("background-color", "#FFEBEE");
                    }
                }
            },
        }).dxDataGrid("instance");

        $amsdGrid.option(dxGridUtils.viewOnlyGridConfig);

        $loadPanel = $("#loadpanel").dxLoadPanel({
            shadingColor: "rgba(0,0,0,0.4)",
            visible: false,
            showIndicator: true,
            showPane: true,
            shading: true,
            closeOnOutsideClick: false
        }).dxLoadPanel("instance");

    });
}(window.jQuery, window, document));