(function ($, window, document) {

    $(function () {
        var $grid1,
            $todaySubmissionBtn = $("#todaySubmissionBtn"),
            $mySubmissionBtn = $("#mySubmissionBtn"),
            $pendingMyApprovalBtn = $("#pendingMyApprovalBtn"),
            $clearFilterBtn = $("#clearFilterBtn"),


            $retractSubmissionModal = $("#retractSubmissionModal"),
            $retractFormBtn = $("#retractFormBtn"),
            $retractFormCancelBtn = $("#retractFormCancelBtn"),
            $retractFormId = $("#retractFormId"),
            $retractFormPreparedBy = $("#retractFormPreparedBy"),
            $retractFormSubmissionDate = $("#retractFormSubmissionDate"),
            $retractFormAssignedApprover = $("#retractFormAssignedApprover");

        var referenceUrl = {
            printRequest: window.location.origin + "/api/fid/Treasury/GenFile",
            printResponse: window.location.origin + "/fid/Treasury/Download/",

            loadGrid: window.location.origin + "/api/fid/treasury",

            editPageRedirect: window.location.origin + "/fid/Treasury/Edit/",
            viewPageRedirect: window.location.origin + "/fid/Treasury/View/",
            deleteForm: window.location.origin + "/api/fid/Treasury/",
            retractForm: window.location.origin + "/api/fid/Treasury/retractForm",
        };

        $grid1 = $("#grid1").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.loadGrid
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
                                return (e.row.data.enableEdit);
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
                                        url: referenceUrl.deleteForm,
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
            }
        }).dxDataGrid("instance");

        //#region Filter Btn

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
                    url: referenceUrl.retractForm,
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