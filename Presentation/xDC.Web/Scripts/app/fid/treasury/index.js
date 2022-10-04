﻿(function ($, window, document) {

    $(function () {
        var $grid1,
            $todaySubmissionBtn = $("#todaySubmissionBtn"),
            $mySubmissionBtn = $("#mySubmissionBtn"),
            $pendingMyApprovalBtn = $("#pendingMyApprovalBtn"),
            $clearFilterBtn = $("#clearFilterBtn");

        var referenceUrl = {
            printRequest: window.location.origin + "/fid/Treasury/Print",
            printResponse: window.location.origin + "/fid/Treasury/Printed/"
        };

        $pendingMyApprovalBtn.dxButton({
            onClick: function (e) {
                $grid1.filter([
                    ["approvedBy", "=", window.currentUser],
                    "and",
                    [
                        ["formStatus", "=", "Pending Approval"],
                        "or",
                        ["formStatus", "=", "Pending Approval (Resubmission)"]
                    ]
                    
                ]);
            }
        });

        $mySubmissionBtn.dxButton({
            onClick: function (e) {
                $grid1.filter([
                    ["preparedBy", "=", window.currentUser]
                ]);
            }
        });

        $todaySubmissionBtn.dxButton({
            onClick: function (e) {
                $grid1.filter([
                    ["preparedDate", ">=", moment().startOf("day").toDate()],
                    "and",
                    ["preparedDate", "<", moment().add(1, "days").toDate()]
                ]);
            }
        });

        $clearFilterBtn.dxButton({
            onClick: function (e) {
                $grid1.clearFilter();
            }
        });
        
        
        $grid1 = $("#grid1").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: window.location.origin + "/api/fid/treasury"
            }),
            columns: [
                {
                    dataField: "id",
                    caption: "Form ID",
                    width: "100px",
                    alignment: "left",
                    allowHeaderFiltering: false,
                    visible: false
                },
                {
                    dataField: "valueDate",
                    caption: "Value Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    sortIndex: 0,
                    sortOrder: "desc"
                },
                {
                    dataField: "currency",
                    caption: "Currency",
                    groupIndex: 0
                },
                {
                    dataField: "preparedBy",
                    caption: "Preparer"
                },
                {
                    dataField: "preparedDate",
                    caption: "Prepared Date",
                    dataType: "date",
                    format: "dd/MM/yyyy HH:mm",
                    sortIndex: 1,
                    sortOrder: "desc"
                },
                {
                    dataField: "approvedBy",
                    caption: "Approver"
                },
                {
                    dataField: "approvedDate",
                    caption: "Approved Date",
                    dataType: "date",
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
                            hint: "Edit",
                            icon: "fa fa-pencil-square",
                            cssClass: "dx-datagrid-command-btn",
                            visible: function (e) {
                                return (e.row.data.isEditAllowed);
                            },
                            onClick: function (e) {
                                window.location.href = window.location.origin + "/fid/Treasury/Edit/" + e.row.data.id;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Delete",
                            icon: "fa fa-trash-o",
                            cssClass: "dx-datagrid-command-btn text-red",
                            visible: function (e) {
                                return (e.row.data.isDeleteAllowed);
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
                                        url: window.location.origin + "/api/fid/Treasury/",
                                        data: data,
                                        success: function (data) {
                                            app.toast("Form Deleted", "warning", 2000);

                                            $grid1.refresh();
                                        },
                                        fail: function (jqXHR, textStatus, errorThrown) {
                                            $app.toast(textStatus + ": " + errorThrown, "error");

                                        }
                                    });
                                    e.event.preventDefault();
                                }
                            }
                        },
                        {
                            hint: "View",
                            icon: "fa fa-eye",
                            cssClass: "dx-datagrid-command-btn",
                            onClick: function (e) {
                                window.location.href = window.location.origin + "/fid/Treasury/View/" + e.row.data.id;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Download as Excel",
                            icon: "fa fa-file-excel-o",
                            cssClass: "dx-datagrid-command-btn text-green",
                            onClick: function (e) {
                                app.toast("Generating...");

                                var data = {
                                    id: e.row.data.id,
                                    isExportAsExcel: true
                                };

                                $.ajax({
                                    type: "POST",
                                    url: referenceUrl.printRequest,
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        var url = referenceUrl.printResponse + data;
                                        window.location = url;
                                    },
                                    fail: function (jqXHR, textStatus, errorThrown) {
                                        app.alertError(textStatus + ": " + errorThrown);
                                    },
                                    complete: function (data) {

                                    }
                                });

                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Download as PDF",
                            icon: "fa fa-file-pdf-o",
                            cssClass: "dx-datagrid-command-btn text-orange",
                            onClick: function (e) {

                                app.toast("Generating...");

                                var data = {
                                    id: e.row.data.id,
                                    isExportAsExcel: false
                                };

                                $.ajax({
                                    type: "POST",
                                    url: referenceUrl.printRequest,
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        var url = referenceUrl.printResponse + data;
                                        window.location = url;
                                    },
                                    fail: function (jqXHR, textStatus, errorThrown) {
                                        app.alertError(textStatus + ": " + errorThrown);
                                    },
                                    complete: function (data) {

                                    }
                                });
                                
                                e.event.preventDefault();
                            }
                        }
                    ]
                }
            ],
            onRowPrepared: function (e) {
                if (e.rowType == "data") {
                    if (e.data.isPendingMyApproval) {
                        e.rowElement.css("background-color", "#FFEBEE");
                    }
                    if (e.data.isMyFormRejected) {
                        e.rowElement.css("background-color", "#FFEBEE");
                    }
                }
            },
            groupPanel: {
                visible: true
            },
            showBorders: true,
            showColumnLines: true,
            showRowLines: true,
            sorting: {
                mode: "multiple"
            },
            searchPanel: {
                visible: true
            },
            headerFilter: {
                visible: true
            },
            paging: {
                pageSize: 10
            },
            pager: {
                visible: true,
                allowedPageSizes: [10, 20, 50, "all"],
                showPageSizeSelector: true,
                showInfo: true,
                showNavigationButtons: true
            },
            filterPanel: {
                visible: true
            },
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: "xDC_Treasury_Grid_1"
            }
        }).dxDataGrid("instance");
        
    });
}(window.jQuery, window, document));