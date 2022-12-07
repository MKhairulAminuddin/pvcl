(function ($, window, document) {

    $(function () {

        var $issdGrid,
            $issdGridFilterBtn,
            $newFormBtn,
            $grid2,
            $filterStatusSb,
            $clearFilterBtn,

            $todayApprovedFilterBtn = $("#todayApprovedFilterBtn"),
            $audFilterBtn = $("#audFilterBtn"),
            $eurFilterBtn = $("#eurFilterBtn"),
            $gbpFilterBtn = $("#gbpFilterBtn"),
            $myrFilterBtn = $("#myrFilterBtn"),
            $usdFilterBtn = $("#usdFilterBtn"),
            $clearFilterGrid2Btn = $("#clearFilterGrid2Btn"),

            $retractSubmissionModal = $("#retractSubmissionModal"),
            $retractFormBtn = $("#retractFormBtn"),
            $retractFormCancelBtn = $("#retractFormCancelBtn"),
            $retractFormId = $("#retractFormId"),
            $retractFormPreparedBy = $("#retractFormPreparedBy"),
            $retractFormSubmissionDate = $("#retractFormSubmissionDate"),
            $retractFormAssignedApprover = $("#retractFormAssignedApprover");

        var statuses = [
            {
                "RefName": "All",
                "DisplayName": "All"
            },
            {
                "RefName": "Trade Settlement (Part A)",
                "DisplayName": "A - Equity"
            },
            {
                "RefName": "Trade Settlement (Part B)",
                "DisplayName": "B - Bond, CP, Notes/Papers, Coupon"
            },
            {
                "RefName": "Trade Settlement (Part C)",
                "DisplayName": "C - REPO"
            },
            {
                "RefName": "Trade Settlement (Part D)",
                "DisplayName": "D - MTM, FX"
            },
            {
                "RefName": "Trade Settlement (Part E)",
                "DisplayName": "E - ALTID"
            },
            {
                "RefName": "Trade Settlement (Part F)",
                "DisplayName": "F - Fees"
            },
            {
                "RefName": "Trade Settlement (Part G)",
                "DisplayName": "G - Contribution"
            },
            {
                "RefName": "Trade Settlement (Part H)",
                "DisplayName": "H - Others"
            }
        ];

        var formStatus = ["All Statuses", "Approved", "Draft", "Pending Approval", "Rejected", "Pending Approval (Resubmission)", "Rejected"];

        var referenceUrl = {
            loadGrid1: window.location.origin + "/api/issd/ts/home/grid1",
            loadGrid2: window.location.origin + "/api/issd/ts/home/grid2",
            retractForm: window.location.origin + "/api/issd/ts/home/retractForm",

            printPart: window.location.origin + "/api/issd/ts/generatePart",
            printConsolidated: window.location.origin + "/api/issd/ts/generateConsolidated",
            viewPrinted: window.location.origin + "/issd/TradeSettlement/Download/",
            viewConsolidated: window.location.origin + "/issd/TradeSettlement/ConsolidatedView",
            viewNew: window.location.origin + "/issd/TradeSettlement/New/",
            viewEdit: window.location.origin + "/issd/TradeSettlement/Edit/",
            viewForm: window.location.origin + "/issd/TradeSettlement/View/"
        }
        
        $issdGrid = $("#issdGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.loadGrid1
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
                    dataField: "formType",
                    caption: "Form Type",
                    groupIndex: 0
                },
                {
                    dataField: "formDate",
                    caption: "Settlement Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy",
                    sortIndex: 0,
                    sortOrder: "desc"
                },
                {
                    dataField: "currency",
                    caption: "Currency"
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
                                window.location.href = referenceUrl.viewEdit + e.row.data.id;
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

                                    app.toast("Form delete...", "warning", 2000);

                                    $.ajax({
                                        type: "delete",
                                        url: window.location.origin + "/api/issd/TradeSettlement/",
                                        data: data,
                                        success: function (data) {
                                            app.toast("Form deleted", "success", 2000);

                                            $issdGrid.refresh();
                                            $grid2.refresh();
                                        },
                                        error: function (jqXHR, textStatus, errorThrown) {
                                            app.alertErrorJqXhr(jqXHR);
                                        }
                                    });
                                    e.event.preventDefault();
                                }
                            }
                        },
                        {
                            hint: "View Form",
                            icon: "fa fa-eye",
                            cssClass: "dx-datagrid-command-btn",
                            onClick: function (e) {
                                app.openInNewTab(referenceUrl.viewForm + e.row.data.id);
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
                                app.toast("Generating export file...");

                                var data = {
                                    formId: e.row.data.id,
                                    isExportAsExcel: true
                                };

                                $.ajax({
                                    type: "POST",
                                    url: referenceUrl.printPart,
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        app.toast("File generated", "success", 3000);

                                        var url = referenceUrl.viewPrinted + JSON.parse(data);
                                        window.location = url;
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        app.alertErrorJqXhr(jqXHR);
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
                            visible: function (e) {
                                return e.row.data.enablePrint;
                            },
                            onClick: function (e) {
                                app.toast("Generating export file...");

                                var data = {
                                    formId: e.row.data.id,
                                    isExportAsExcel: false
                                };

                                $.ajax({
                                    type: "POST",
                                    url: referenceUrl.printPart,
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        app.toast("File generated", "success", 3000);

                                        var url = referenceUrl.viewPrinted + JSON.parse(data);
                                        window.location = url;
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        app.alertErrorJqXhr(jqXHR);
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
                    if (e.data.isRejected) {
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

        $filterStatusSb = $("#filterStatusSb").dxSelectBox({
            dataSource: formStatus,
            value: formStatus[0],
            onValueChanged: function (data) {
                if (data.value == "All Statuses") {
                    if ($issdGridFilterBtn.option("value") !== "All") {
                        $issdGrid.filter(["!", ["formStatus", "=", data.value]], "and", ["formType", "=", $issdGridFilterBtn.option("value")]);
                    } else {
                        $issdGrid.filter(["!", ["formStatus", "=", data.value]]);
                    }

                } else {
                    if ($issdGridFilterBtn.option("value") !== "All") {
                        $issdGrid.filter(["formStatus", "=", data.value], "and", ["formType", "=", $issdGridFilterBtn.option("value")]);
                    } else {
                        $issdGrid.filter(["formStatus", "=", data.value]);
                    }
                }
            }
        }).dxSelectBox("instance");

        $issdGridFilterBtn = $("#filterformBtn").dxSelectBox({
            dataSource: statuses,
            value: statuses[0].RefName,
            displayExpr: "DisplayName",
            valueExpr: "RefName",
            onValueChanged: function (data) {
                if (data.value == "All") {
                    if ($filterStatusSb.option("value") !== "All Statuses") {
                        $issdGrid.filter(["!", ["formType", "=", data.value]], "and", ["formStatus", "=", $filterStatusSb.option("value")]);
                    } else {
                        $issdGrid.filter(["!", ["formType", "=", data.value]]);
                    }
                } else {
                    if ($filterStatusSb.option("value") !== "All Statuses") {
                        $issdGrid.filter(["formType", "=", data.value], "and", ["formStatus", "=", $filterStatusSb.option("value")]);
                    } else {
                        $issdGrid.filter(["formType", "=", data.value]);
                    }
                }
            }
        }).dxSelectBox("instance");

        $clearFilterBtn = $("#clearFilterGrid1Btn").dxButton({
            onClick() {
                $issdGrid.clearFilter();
                $issdGridFilterBtn.option("value", statuses[0].RefName);
                $filterStatusSb.option("value", formStatus[0]);
            }
        });

        $newFormBtn = $("#newFormBtn").dxDropDownButton({
            items: [
                "A - Equity",
                "B - Bond, CP, Notes/Papers, Coupon",
                "C - REPO",
                "D - MTM, FX",
                "E - ALTID",
                "F - Fees",
                "G - Contribution",
                "H - Others"
            ],
            onItemClick: function (e) {
                switch (e.itemData) {
                    case "A - Equity":
                        window.location = referenceUrl.viewNew + "3";
                        return;
                    case "B - Bond, CP, Notes/Papers, Coupon":
                        window.location = referenceUrl.viewNew + "4";
                        return;
                    case "C - REPO":
                        window.location = referenceUrl.viewNew + "5";
                        return;
                    case "D - MTM, FX":
                        window.location = referenceUrl.viewNew + "6";
                        return;
                    case "E - ALTID":
                        window.location = referenceUrl.viewNew + "7";
                        return;
                    case "F - Fees":
                        window.location = referenceUrl.viewNew + "8";
                        return;
                    case "G - Contribution":
                        window.location = referenceUrl.viewNew + "9";
                        return;
                    case "H - Others":
                        window.location = referenceUrl.viewNew + "10";
                        return;
                    default:
                        alert("Invalid Selection");
                }
            }
        });

        // #region Grid 2

        $grid2 = $("#consolidatedTradeSettlementGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                loadUrl: referenceUrl.loadGrid2
            }),
            columns: [
                {
                    dataField: "formDate",
                    caption: "Settlement Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy",
                    sortIndex: 0,
                    sortOrder: "desc"
                },
                {
                    dataField: "currency",
                    caption: "Currency"
                },
                {
                    dataField: "approvedDate",
                    caption: "Approved Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm"
                },
                {
                    caption: "Actions",
                    type: "buttons",
                    width: 130,
                    buttons: [
                        {
                            hint: "View Form",
                            icon: "fa fa-eye",
                            cssClass: "dx-datagrid-command-btn",
                            onClick: function (e) {
                                app.openInNewTab(
                                    referenceUrl.viewConsolidated + "?settlementDateEpoch=" + moment(e.row.data.formDate).unix() + "&currency=" + e.row.data.currency);

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
                                app.toast("Generating export file...");

                                var data = {
                                    settlementDate: moment(e.row.data.formDate).unix(),
                                    currency: e.row.data.currency,
                                    isExportAsExcel: true
                                };

                                $.ajax({
                                    type: "POST",
                                    url: referenceUrl.printConsolidated,
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        app.toast("File generated", "success", 3000);

                                        var url = referenceUrl.viewPrinted + JSON.parse(data);
                                        window.location = url;
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        app.alertErrorJqXhr(jqXHR);
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
                            visible: function (e) {
                                return e.row.data.enablePrint;
                            },
                            onClick: function (e) {
                                app.toast("Generating export file...");

                                var data = {
                                    settlementDate: moment(e.row.data.formDate).unix(),
                                    currency: e.row.data.currency,
                                    isExportAsExcel: false
                                };

                                $.ajax({
                                    type: "POST",
                                    url: referenceUrl.printConsolidated,
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        app.toast("File generated", "success", 3000);

                                        var url = referenceUrl.viewPrinted + JSON.parse(data);
                                        window.location = url;
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        app.alertErrorJqXhr(jqXHR);
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
            groupPanel: {
                visible: true
            },
            showBorders: true,
            showColumnLines: true,
            showRowLines: true,
            sorting: {
                mode: "multiple"
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

        $todayApprovedFilterBtn.dxButton({
            onClick: function (e) {
                $grid2.filter([
                    ["approvedDate", ">=", moment().startOf("day").toDate()],
                    "and",
                    ["approvedDate", "<", moment().add(1, "days").toDate()]
                ]);
            }
        });

        $audFilterBtn.dxButton({
            onClick: function (e) {
                $grid2.filter([
                    ["currency", "=", "AUD"]
                ]);
            }
        });

        $eurFilterBtn.dxButton({
            onClick: function (e) {
                $grid2.filter([
                    ["currency", "=", "EUR"]
                ]);
            }
        });

        $gbpFilterBtn.dxButton({
            onClick: function (e) {
                $grid2.filter([
                    ["currency", "=", "GBP"]
                ]);
            }
        });

        $myrFilterBtn.dxButton({
            onClick: function (e) {
                $grid2.filter([
                    ["currency", "=", "MYR"]
                ]);
            }
        });

        $usdFilterBtn.dxButton({
            onClick: function (e) {
                $grid2.filter([
                    ["currency", "=", "USD"]
                ]);
            }
        });

        $clearFilterGrid2Btn.dxButton({
            onClick: function (e) {
                $grid2.clearFilter();
            }
        });

        // #endregion

        // #region Modal Retract Submission

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
                        app.toast("Form status retracted/withdrawal success", "success", 3000);
                        $retractSubmissionModal.modal('hide');
                        window.location.href = window.location.href;
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        app.alertErrorJqXhr(jqXHR);
                        $retractSubmissionModal.modal('hide');
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

        // #endregion 

    });
}(window.jQuery, window, document));