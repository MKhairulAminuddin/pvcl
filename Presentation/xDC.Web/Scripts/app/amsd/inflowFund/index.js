(function ($, window, document) {

    $(function () {

        var $ifFormGrid,
            $retractSubmissionModal = $("#retractSubmissionModal"),
            $retractFormBtn = $("#retractFormBtn"),
            $retractFormCancelBtn = $("#retractFormCancelBtn"),

            $retractFormId = $("#retractFormId"),
            $retractFormPreparedBy = $("#retractFormPreparedBy"),
            $retractFormSubmissionDate = $("#retractFormSubmissionDate"),
            $retractFormAssignedApprover = $("#retractFormAssignedApprover"),

            $todayFilterBtn = $("#todayFilterBtn"),
            $draftFilterBtn = $("#draftFilterBtn"),
            $pendingApprovalFilterBtn = $("#pendingApprovalFilterBtn"),
            $approvedFilterBtn = $("#approvedFilterBtn"),
            $rejectedFilterBtn = $("#rejectedFilterBtn"),
            $clearFilterBtn = $("#clearFilterBtn");

        var referenceUrl = {
            ifFormGrid: window.location.origin + "/api/amsd/inflowfund/home",

            ifFormDelete: window.location.origin + "/api/amsd/inflowfund/delete",
            ifFormRetract: window.location.origin + "/api/amsd/inflowfund/retract",
            
            editPageRedirect: window.location.origin + "/amsd/inflowfund/edit/",
            viewPageRedirect: window.location.origin + "/amsd/inflowfund/view/",

            printRequest: window.location.origin + "/api/amsd/inflowfund/GenFile",
            printResponse: window.location.origin + "/amsd/inflowfund/Download/",
        };
        
        $ifFormGrid = $("#amsdGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.ifFormGrid
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
                    format: "dd/MM/yyyy hh:mm a",
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
                    format: "dd/MM/yyyy hh:mm a"
                },
                {
                    dataField: "formStatus",
                    caption: "Status"
                },
                {
                    caption: "Actions",
                    type: "buttons",
                    width: 130,
                    buttons: [
                        {
                            hint: "Edit",
                            icon: "fa fa-pencil-square",
                            cssClass: "dx-datagrid-command-btn",
                            visible: function (e) {
                                return e.row.data.enableEdit;
                            },
                            onClick: function (e) {
                                window.location.href = referenceUrl.editPageRedirect + e.row.data.id;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Delete",
                            icon: "fa fa-trash-o",
                            cssClass: "dx-datagrid-command-btn text-red",
                            visible: function (e) {
                                return e.row.data.enableDelete;
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
                                        url: referenceUrl.ifFormDelete,
                                        data: data,
                                        success: function (data) {
                                            app.toast("Form Deleted!", "warning", 2000);
                                            $ifFormGrid.refresh();
                                        },
                                        fail: function (jqXHR, textStatus, errorThrown) {
                                            app.alertError(textStatus + ': ' + errorThrown);
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
                                window.location.href = referenceUrl.viewPageRedirect + e.row.data.id;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Retract Submission",
                            icon: "fa fa-chain-broken",
                            cssClass: "dx-datagrid-command-btn",
                            visible: function (e) {
                                return e.row.data.enableRetractSubmission;
                            },
                            onClick: function (e) {
                                $retractFormId.text(e.row.data.id);
                                $retractFormPreparedBy.text(e.row.data.preparedBy);
                                $retractFormSubmissionDate.text(moment(e.row.data.preparedDate, "YYYY-MM-DDTHH:mm:ssZ").format("DD/MM/yyyy HH:mm A"));
                                $retractFormAssignedApprover.text(e.row.data.approvedBy);
                                
                                $retractSubmissionModal.modal('show');
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Download as Excel",
                            icon: "fa fa-file-excel-o",
                            cssClass: "dx-datagrid-command-btn text-green",
                            visible: function (e) {
                                return e.row.data.enablePrint;
                            },
                            onClick: function (e) {
                                app.toast("Generating...");

                                var data = {
                                    formId: e.row.data.id,
                                    isExportAsExcel: true
                                };

                                $.ajax({
                                    type: "POST",
                                    url: referenceUrl.printRequest,
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        window.location = referenceUrl.printResponse + data;
                                    },
                                    fail: function (jqXHR, textStatus, errorThrown) {
                                        app.alertError(textStatus + ': ' + errorThrown);
                                    },
                                    complete:function(data) {
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
                                return e.row.data.enablePrint;
                            },
                            onClick: function (e) {
                                app.toast("Generating...");

                                var data = {
                                    formId: e.row.data.id,
                                    isExportAsExcel: false
                                };

                                $.ajax({
                                    type: "POST",
                                    url: referenceUrl.printRequest,
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        window.location = referenceUrl.printResponse + data;
                                    },
                                    fail: function (jqXHR, textStatus, errorThrown) {
                                        app.alertError(textStatus + ': ' + errorThrown);
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
                    if (e.data.isRejectedForm) {
                        e.rowElement.css("background-color", "#FFEBEE");
                    }
                }
            },
            remoteOperations: false,
            showRowLines: true,
            rowAlternationEnabled: false,
            showBorders: true,
            allowColumnReordering: true,
            allowColumnResizing: true,
            sorting: {
                mode: "multiple",
                showSortIndexes: true
            },
            columnFixing: {
                enabled: true
            },
            headerFilter: { visible: true },
            pager: {
                infoText: "Page {0} of {1} ({2} items)",
                showPageSizeSelector: true,
                allowedPageSizes: [10, 20, 50],
                showNavigationButtons: true,
                showInfo: true
            },
            paging: {
                pageSize: 10,
                pageIndex: 0
            },
            wordWrapEnabled: true
        }).dxDataGrid("instance");

        //#region Filter Btn

        $todayFilterBtn.dxButton({
            onClick: function (e) {
                $ifFormGrid.filter([
                    ["preparedDate", ">=", moment().startOf("day").toDate()],
                    "and",
                    ["preparedDate", "<", moment().add(1, "days").toDate()]
                ]);
            }
        });

        $draftFilterBtn.dxButton({
            onClick: function (e) {
                $ifFormGrid.filter([
                    ["formStatus", "=", "Draft"]
                ]);
            }
        });

        $pendingApprovalFilterBtn.dxButton({
            onClick: function (e) {
                $ifFormGrid.filter([
                    ["formStatus", "=", "Pending Approval"]
                ]);
            }
        });

        $approvedFilterBtn.dxButton({
            onClick: function (e) {
                $ifFormGrid.filter([
                    ["formStatus", "=", "Approved"]
                ]);
            }
        });

        $rejectedFilterBtn.dxButton({
            onClick: function (e) {
                $ifFormGrid.filter([
                    ["formStatus", "=", "Rejected"]
                ]);
            }
        });

        $clearFilterBtn.dxButton({
            onClick: function (e) {
                $ifFormGrid.clearFilter();
            }
        });

        //#endregion

        //#region Retract Form Submission

        $retractFormBtn.dxButton({
            onClick: function (e) {
                app.toast("Retract form submission...", "warning", 3000);

                $.ajax({
                    data: {
                        formId: parseInt($retractFormId.text())
                    },
                    dataType: 'json',
                    url: referenceUrl.ifFormRetract,
                    method: 'post',
                    success: function (data) {
                        app.alertSuccess("Form status retracted success");
                    },
                    fail: function (jqXHR, textStatus, errorThrown) {
                        app.alertError(textStatus + ': ' + errorThrown);
                    },
                    complete: function (data) {
                        $retractSubmissionModal.modal('hide');
                        window.location.href = window.location.href;
                    }
                });
            }
        });

        $retractFormCancelBtn.dxButton({
            onClick: function (e) {
                $retractFormId.text();
                $retractFormPreparedBy.text();
                $retractFormSubmissionDate.text();
                $retractFormAssignedApprover.text();

                $retractSubmissionModal.modal('hide');
            }
        });

        //#endregion

    });
}(window.jQuery, window, document));