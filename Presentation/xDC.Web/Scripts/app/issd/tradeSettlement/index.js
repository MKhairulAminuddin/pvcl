(function ($, window, document) {

    $(function () {

        var $issdGrid, $newFormBtn;
        
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
                            hint: "Edit Draft",
                            icon: "fa fa-pencil-square",
                            cssClass: "dx-datagrid-command-btn",
                            visible: function (e) {
                                console.log((e.row.data.isDraft && e.row.data.isMeCanEditDraft));
                                return (e.row.data.isDraft && e.row.data.isMeCanEditDraft);
                            },
                            onClick: function (e) {
                                window.location.href = "/issd/TradeSettlement/Edit/" + e.row.data.id;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "Delete Draft",
                            icon: "fa fa-trash-o",
                            cssClass: "dx-datagrid-command-btn text-red",
                            visible: function (e) {
                                return (e.row.data.isDraft && e.row.data.isMeCanEditDraft);
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
                                        url: window.location.origin + '/api/issd/TradeSettlement/',
                                        data: data,
                                        success: function (data) {
                                            $("#error_container").bs_success("Draft deleted");
                                            $issdGrid.refresh();
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
                                window.location.href = "/issd/TradeSettlement/Edit/" + e.row.data.id;
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
                                window.location.href = "/issd/TradeSettlement/Edit" + e.row.data.id;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "View Form",
                            icon: "fa fa-eye",
                            cssClass: "dx-datagrid-command-btn",
                            onClick: function (e) {
                                window.location.href = "/issd/TradeSettlement/View/" + e.row.data.id;
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
                                $loadPanel.option("position", { of: ".content-wrapper" });
                                $loadPanel.show();

                                var data = {
                                    id: e.row.data.id,
                                    isExportAsExcel: true
                                };

                                $.ajax({
                                    type: "POST",
                                    url: '/issd/Print',
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        var url = '/issd/ViewPrinted/' + data;
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
                        },
                        {
                            hint: "Download as PDF",
                            icon: "fa fa-file-pdf-o",
                            cssClass: "dx-datagrid-command-btn text-orange",
                            visible: function (e) {
                                return (!e.row.data.isDraft);
                            },
                            onClick: function (e) {
                                $loadPanel.option("position", { of: ".content-wrapper" });
                                $loadPanel.show();

                                var data = {
                                    id: e.row.data.id,
                                    isExportAsExcel: false
                                };

                                $.ajax({
                                    type: "POST",
                                    url: '/issd/Print',
                                    data: data,
                                    dataType: "text",
                                    success: function (data) {
                                        var url = '/issd/ViewPrinted/' + data;
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
            groupPanel: {
                visible: true
            },
            showBorders: true,
            sorting: {
                mode: "multiple"
            },
            headerFilter: {
                visible: true
            }
        }).dxDataGrid("instance");

        $newFormBtn = $("#newFormBtn").dxDropDownButton({
            text: "New Trade Settlement",
            icon: "plus",
            type: "normal",
            stylingMode: "contained",
            dropDownOptions: {
                width: 230
            },
            items: [
                "Part A (Equity)",
                "Part B (Bond, CP, Notes and Papers, REPO, Coupon)",
                "Part C (MTM, FX)",
                "Part D (ALTID)",
                "Part E (Fees, Contribution, Others)"
            ],
            onItemClick: function (e) {
                switch (e.itemData) {
                    case "Part A (Equity)":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartA/New";
                        return;
                    case "Part B (Bond, CP, Notes and Papers, REPO, Coupon)":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartB/New";
                        return;
                    case "Part C (MTM, FX)":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartC/New";
                        return;
                    case "Part D (ALTID)":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartD/New";
                        return;
                    case "Part E (Fees, Contribution, Others)":
                        window.location = window.location.origin + "/issd/TradeSettlement/PartE/New";
                        return;
                    default:
                        alert("Invalid Selection");
                }
            }
        }).dxDropDownButton("instance");
    });
}(window.jQuery, window, document));