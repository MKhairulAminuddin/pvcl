﻿(function ($, window, document) {

    $(function () {

        var $issdGrid, $issdGridFilterBtn, $newFormBtn, $consolidatedTradeSettlementGrid, $filterStatusSb, $clearFilterBtn;

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
        
        $issdGrid = $("#issdGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: window.location.origin + "/api/issd/TradeSettlement"
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
                    width: 110,
                    buttons: [
                        {
                            hint: "Edit",
                            icon: "fa fa-pencil-square",
                            cssClass: "dx-datagrid-command-btn",
                            visible: function (e) {
                                return e.row.data.enableEdit;
                            },
                            onClick: function (e) {
                                switch (e.row.data.formType) {
                                    case "Trade Settlement (Part A)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartA/Edit/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part B)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartB/Edit/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part C)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartC/Edit/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part D)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartD/Edit/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part E)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartE/Edit/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part F)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartF/Edit/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part G)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartG/Edit/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part H)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartH/Edit/" + e.row.data.id;
                                        return;
                                    
                                    default:
                                        alert("Invalid selection!");
                                }
                                
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
                                        url: window.location.origin + "/api/issd/TradeSettlement/",
                                        data: data,
                                        success: function (data) {
                                            app.toast("Form delete...", "warning", 2000);

                                            $issdGrid.refresh();
                                            $consolidatedTradeSettlementGrid.refresh();
                                        },
                                        fail: function (jqXHR, textStatus, errorThrown) {
                                            app.alertError(textStatus + ": " + errorThrown);

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
                                switch (e.row.data.formType) {
                                    case "Trade Settlement (Part A)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartA/View/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part B)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartB/View/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part C)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartC/View/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part D)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartD/View/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part E)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartE/View/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part F)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartF/View/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part G)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartG/View/" + e.row.data.id;
                                        return;
                                    case "Trade Settlement (Part H)":
                                        window.location.href = window.location.origin + "/issd/TradeSettlement/PartH/View/" + e.row.data.id;
                                        return;

                                    default:
                                        alert("Invalid selection!");
                                }
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
                                    id: e.row.data.id,
                                    isExportAsExcel: true
                                };

                                $.ajax({
                                    type: "POST",
                                    url: "/issd/Print",
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        var url = "/issd/ViewPrinted/" + data;
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
                            visible: function (e) {
                                return e.row.data.enablePrint;
                            },
                            onClick: function (e) {
                                app.toast("Generating...");
                                var data = {
                                    id: e.row.data.id,
                                    isExportAsExcel: false
                                };

                                $.ajax({
                                    type: "POST",
                                    url: "/issd/Print",
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        var url = "/issd/ViewPrinted/" + data;
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
            },
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: "xDC_TS_Grid_02"
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

        $clearFilterBtn = $("#clearFilterBtn").dxButton({
            icon: "refresh",
            hint: "Clear filter",
            onClick() {
                $issdGrid.clearFilter();
                $issdGridFilterBtn.option("value", statuses[0].RefName);
                $filterStatusSb.option("value", formStatus[0]);
            }
        }).dxButton("instance");

        $newFormBtn = $("#newFormBtn").dxDropDownButton({
            text: "New Trade Settlement",
            icon: "plus",
            type: "normal",
            stylingMode: "contained",
            dropDownOptions: {
                width: 230
            },
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
                        window.location = window.location.origin + "/issd/TradeSettlement/PartA/New";
                        return;
                    case "B - Bond, CP, Notes/Papers, Coupon":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartB/New";
                        return;
                    case "C - REPO":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartC/New";
                        return;
                    case "D - MTM, FX":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartD/New";
                        return;
                    case "E - ALTID":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartE/New";
                        return;
                    case "F - Fees":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartF/New";
                        return;
                    case "G - Contribution":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartG/New";
                        return;
                    case "H - Others":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartH/New";
                        return;
                    default:
                        alert("Invalid Selection");
                }
            }
        }).dxDropDownButton("instance");

        $consolidatedTradeSettlementGrid = $("#consolidatedTradeSettlementGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: window.location.origin + "/api/issd/TradeSettlement/Approved"
            }),
            columns: [
                {
                    dataField: "formDate",
                    caption: "Settlement Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy",
                    sortIndex: 0,
                    sortOrder: "desc",
                    groupIndex: 0
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
                    width: 110,
                    buttons: [
                        {
                            hint: "View Form",
                            icon: "fa fa-eye",
                            cssClass: "dx-datagrid-command-btn",
                            onClick: function (e) {
                                window.location.href = window.location.origin +
                                    "/issd/TradeSettlement/View/" +
                                    "?settlementDateEpoch=" +
                                    moment(e.row.data.formDate).unix() +
                                    "&currency=" +
                                    e.row.data.currency;

                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Download as Excel",
                            icon: "fa fa-file-excel-o",
                            cssClass: "dx-datagrid-command-btn text-green",
                            visible: function (e) {
                                return (!e.row.data.isDraft);
                            },
                            onClick: function (e) {
                                app.toast("Generating...");
                                var data = {
                                    settlementDate: moment(e.row.data.formDate).unix(),
                                    currency: e.row.data.currency,
                                    isExportAsExcel: true
                                };

                                $.ajax({
                                    type: "POST",
                                    url: "/issd/PrintConsolidated",
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        var url = "/issd/ViewPrinted/" + data;
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
                            visible: function (e) {
                                return (!e.row.data.isDraft);
                            },
                            onClick: function (e) {
                                app.toast("Generating...");
                                var data = {
                                    settlementDate: moment(e.row.data.formDate).unix(),
                                    currency: e.row.data.currency,
                                    isExportAsExcel: false
                                };

                                $.ajax({
                                    type: "POST",
                                    url: "/issd/PrintConsolidated",
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        var url = "/issd/ViewPrinted/" + data;
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
            },
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: "xDC_TS_ApprovedGrid_02"
            }
        }).dxDataGrid("instance");

        $("#submissionBox").boxWidget({
            animationSpeed: 500,
            collapseIcon: "dx-icon-chevrondown",
            expandIcon: "dx-icon-chevronprev"
        });

        $("#approvedBox").boxWidget({
            animationSpeed: 500,
            collapseIcon: "dx-icon-chevrondown",
            expandIcon: "dx-icon-chevronprev"
        });
    });
}(window.jQuery, window, document));