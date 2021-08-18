﻿(function ($, window, document) {
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
            dataSource: DevExpress.data.AspNet.createStore({
                key: "formId",
                loadUrl: window.location.origin + "/api/fid/Ts10AmAccountAssignment"
            }),
            columns: [
                {
                    dataField: "formId",
                    caption: "Form ID",
                    width: "100px",
                    alignment: "left",
                    allowHeaderFiltering: false,
                    visible: false
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
                    caption: "Currency",
                    groupIndex: 0
                },
                {
                    caption: "Pending Assignment",
                    cellTemplate: function (container, options) {

                        if (options.data.countPendingEquity > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingEquity + " Equity").appendTo(container);
                        }
                        if (options.data.countPendingBond > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingBond + " Bond").appendTo(container);
                        }
                        if (options.data.countPendingCp > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingCp + " CP").appendTo(container);
                        }
                        if (options.data.countPendingNotesPapers > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingNotesPapers + " Notes & Papers").appendTo(container);
                        }
                        if (options.data.countPendingRepo > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingRepo + " Repo").appendTo(container);
                        }
                        if (options.data.countPendingCoupon > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingCoupon + " Coupon").appendTo(container);
                        }
                        if (options.data.countPendingFees > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingFees + " Fees").appendTo(container);
                        }
                        if (options.data.countPendingMtm > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingMtm + " MTM").appendTo(container);
                        }
                        if (options.data.countPendingFx > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingFx + " FX").appendTo(container);
                        }
                        if (options.data.countPendingContribution > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingContribution + " Contribution").appendTo(container);
                        }
                        if (options.data.countPendingAltid > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingAltid + " ALTID").appendTo(container);
                        }
                        if (options.data.countPendingOthers > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingOthers + " Others").appendTo(container);
                        }
                        
                    }
                },
                {
                    caption: "Actions",
                    type: "buttons",
                    width: 110,
                    buttons: [
                        {
                            text: "Assign Account",
                            onClick: function (e) {
                                window.location.href = window.location.origin + "/fid/fcaAccountAssignment/Edit/" + e.row.data.formId;
                                e.event.preventDefault();
                            }
                        }
                    ]
                }
            ],
            showBorders: true
        }).dxDataGrid("instance");

        // #endregion DataGrid

        //#region Events
        

        //#endregion

        //#region Immediate Invocation function


        //#endregion
    });
}(window.jQuery, window, document));