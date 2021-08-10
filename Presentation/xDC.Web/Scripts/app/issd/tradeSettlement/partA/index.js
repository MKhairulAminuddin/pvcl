(function ($, window, document) {

    $(function () {

        var $issdGrid, $loadPanel;

        var referenceUrl = {
            load: window.location.origin + "/api/Issd/GetIssdForm/3",
            edit: window.location.origin + "/issd/TradeSettlement/PartA/Edit/",
            deleteDraft: window.location.origin + "/api/issd/TradeSettlement/",
            view: window.location.origin + "/issd/TradeSettlement/PartA/View/",
            printed: window.location.origin + "/issd/ViewPrinted/",
            print: window.location.origin + "/issd/Print"
        };

        var postPrintRequest = function (formId, exportAsExcel) {
            var data = {
                id: formId,
                isExportAsExcel: exportAsExcel
            };

            return $.ajax({
                type: "POST",
                url: referenceUrl.print,
                data: data,
                dataType: "text",
                success: function (data) {
                    var url = referenceUrl.printed + data;
                    window.location = url;
                },
                fail: function (jqXHR, textStatus, errorThrown) {
                    $("#error_container").bs_alert(textStatus + ": " + errorThrown);
                },
                complete: function (data) {
                    $loadPanel.hide();
                }
            });
        }
        
        $issdGrid = $("#issdGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.load
            }),
            columns: [
                {
                    dataField: "id",
                    caption: "Form ID",
                    width: "100px",
                    alignment: "left",
                    headerFilter: { visible: false },
                    visible: false
                },
                {
                    dataField: "formType",
                    caption: "Form Type"
                },
                {
                    dataField: "settlementDate",
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
                                window.location.href = referenceUrl.edit + e.row.data.id;
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
                                        url: referenceUrl.deleteDraft,
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
                            hint: "Resubmit",
                            icon: "fa fa-repeat",
                            cssClass: "dx-datagrid-command-btn",
                            visible: function (e) {
                                return (e.row.data.isResubmitEnabled);
                            },
                            onClick: function (e) {
                                window.location.href = referenceUrl.edit + e.row.data.id;
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
                                window.location.href = referenceUrl.edit + e.row.data.id;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "View Form",
                            icon: "fa fa-eye",
                            cssClass: "dx-datagrid-command-btn",
                            onClick: function (e) {
                                window.location.href = referenceUrl.view + e.row.data.id;
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
                                
                                postPrintRequest(e.row.data.id, true);
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

                                postPrintRequest(e.row.data.id, false);
                                e.event.preventDefault();
                            }
                        }
                    ]
                }
            ]
        }).dxDataGrid("instance");

        $issdGrid.option(dxGridUtils.viewOnlyGridConfig);

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