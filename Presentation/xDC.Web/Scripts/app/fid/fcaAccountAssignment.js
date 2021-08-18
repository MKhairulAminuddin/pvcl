(function ($, window, document) {
    $(function () {
        //#region Variable Definition

        var $tabpanel,
            $grid,
            $dateSelectionBtn,
            $printBtn;

        var referenceUrl = {
            adminEdit: window.location.origin + "/issd/TradeSettlement/PartA/Edit/",

            submitApprovalRequest: window.location.origin + "/api/issd/TradeSettlement/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/PartA/View/"
        };

        //#endregion

        //#region Data Source & Functions


        //#endregion

        //#region Other Widgets
        


        //#endregion

        // #region DataGrid

        $grid = $("#grid").dxDataGrid({
            dataSource: [],
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
                    dataField: "formStatus",
                    caption: "Pending Assignment"
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

                                    default:
                                        alert("Invalid selection!");
                                }

                                e.event.preventDefault();
                            }
                        }
                    ]
                }
            ],
        }).dxDataGrid("instance");

        // #endregion DataGrid

        //#region Events

        $("#viewWorkflowBtn").on({
            "click": function (e) {
                $('#viewWorkflowModal').modal('show');

                e.preventDefault();
            }
        });

        $("#adminEditBtn").on({
            "click": function (e) {
                window.location.href = referenceUrl.adminEdit + tradeSettlement.getIdFromQueryString;
                e.preventDefault();
            }
        });

        $("#approveBtn").on({
            "click": function (e) {
                $("#approvalNoteModal").modal("show");
                e.preventDefault();
            }
        });

        $("#approveBtn").on({
            "click": function (e) {
                $("#approvalNoteModal").modal("show");
                e.preventDefault();
            }
        });

        $("#rejectBtn").on({
            "click": function (e) {
                $("#rejectionNoteModal").modal("show");
                e.preventDefault();
            }
        });

        $("#approveFormBtn").on({
            "click": function (e) {
                submitApprovalRequest(true);
                e.preventDefault();
            }
        });

        $("#rejectFormBtn").on({
            "click": function (e) {
                submitApprovalRequest(false);
                e.preventDefault();
            }
        });

        //#endregion

        //#region Immediate Invocation function


        //#endregion
    });
}(window.jQuery, window, document));