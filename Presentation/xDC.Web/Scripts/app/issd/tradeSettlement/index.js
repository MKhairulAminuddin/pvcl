(function ($, window, document) {

    $(function () {

        var $issdGrid, $issdGridFilterBtn, $newFormBtn, $consolidatedTradeSettlementGrid;

        var statuses = [
            "All",
            "A (Equity)",
            "B (Bond, CP, Notes/Papers, Coupon)",
            "C (REPO)",
            "D (MTM, FX)",
            "E (ALTID)",
            "F (Fees)",
            "G (Contribution)",
            "H (Others)"
        ];
        
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
                                return (!e.row.data.isPendingApproval);
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
                                    
                                    default:
                                        alert("Invalid selection!");
                                }
                                
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Delete Draft",
                            icon: "fa fa-trash-o",
                            cssClass: "dx-datagrid-command-btn text-red",
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
                                            $("#error_container").bs_success("Draft deleted");
                                            $issdGrid.refresh();
                                        },
                                        fail: function (jqXHR, textStatus, errorThrown) {
                                            $("#error_container").bs_alert(textStatus + ": " + errorThrown);
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
                            onClick: function (e) {
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
                                        $("#error_container").bs_alert(textStatus + ": " + errorThrown);
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
                                        $("#error_container").bs_alert(textStatus + ": " + errorThrown);
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
                    if (e.data.isFormPendingMyApproval) {
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
            headerFilter: {
                visible: true
            }
        }).dxDataGrid("instance");

        $issdGridFilterBtn = $("#filterformBtn").dxSelectBox({
            dataSource: statuses,
            value: statuses[0],
            onValueChanged: function (data) {
                if (data.value == "All")
                    $issdGrid.clearFilter();
                else
                    $issdGrid.filter(["formType", "=", data.value]);
            }
        });

        $newFormBtn = $("#newFormBtn").dxDropDownButton({
            text: "New Trade Settlement",
            icon: "plus",
            type: "normal",
            stylingMode: "contained",
            dropDownOptions: {
                width: 230
            },
            items: [
                "A (Equity)",
                "B (Bond, CP, Notes/Papers, Coupon)",
                "C (REPO)",
                "D (MTM, FX)",
                "E (ALTID)",
                "F (Fees)",
                "G (Contribution)",
                "H (Others)"
            ],
            onItemClick: function (e) {
                switch (e.itemData) {
                    case "A (Equity)":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartA/New";
                        return;
                    case "B (Bond, CP, Notes/Papers, Coupon)":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartB/New";
                        return;
                    case "C (REPO)":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartC/New";
                        return;
                    case "D (MTM, FX)":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartD/New";
                        return;
                    case "E (ALTID)":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartE/New";
                        return;
                    case "F (Fees)":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartF/New";
                        return;
                    case "G (Contribution)":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartG/New";
                        return;
                    case "H (Others)":
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
                    sortOrder: "desc"
                },
                {
                    dataField: "currency",
                    caption: "Currency",
                    groupIndex: 0
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
                                        $("#error_container").bs_alert(textStatus + ": " + errorThrown);
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
                                        $("#error_container").bs_alert(textStatus + ": " + errorThrown);
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
                pageSize: 50
            },
            pager: {
                visible: true,
                allowedPageSizes: [20, 50, 'all'],
                showPageSizeSelector: true,
                showInfo: true,
                showNavigationButtons: true
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