﻿(function ($, window, document) {
    $(function () {
        //#region Variable Definition

        var $tabpanel,
            $grid,
            $dateSelectionBtn,
            $printBtn,
            $showPendingAssignmentBtn = $("#showPendingAssignmentBtn"),
            $clearFilterBtn = $("#clearFilterBtn"),
            $showTodayBtn = $("#showTodayBtn");

        var referenceUrl = {

            loadTcaTagging: window.location.origin + "/api/fid/FcaTagging",
            editTcaTaggingPage: window.location.origin + "/fid/FcaTagging/Edit/"

        };

        //#endregion

        //#region Data Source & Functions


        //#endregion

        //#region Other Widgets

        $showPendingAssignmentBtn.dxButton({
            onClick: function (e) {
                $grid.filter([
                    ["countPendingFees", ">", 0],
                    "or",
                    ["countPendingAltid", ">", 0],
                    "or",
                    ["countPendingOthers", ">", 0],
                    "or",
                    ["countUnclassifiedBond", ">", 0],
                    "or",
                    ["countUnclassifiedCoupon", ">", 0]
                ]);
            }
        });

        $showTodayBtn.dxButton({
            onClick: function (e) {
                $grid.filter([
                    ["settlementDate", ">", moment().startOf("day")]
                ]);
            }
        });

        $clearFilterBtn.dxButton({
            onClick: function (e) {
                $grid.clearFilter();
            }
        });

        //#endregion

        // #region DataGrid

        $grid = $("#grid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                //key: "formId",
                loadUrl: referenceUrl.loadTcaTagging
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
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingEquity + " x Equity").appendTo(container);
                        }
                        if (options.data.countPendingBond > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingBond + " x Bond").appendTo(container);
                        }
                        if (options.data.countPendingCp > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingCp + " x CP").appendTo(container);
                        }
                        if (options.data.countPendingNotesPapers > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingNotesPapers + " x Notes & Papers").appendTo(container);
                        }
                        if (options.data.countPendingRepo > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingRepo + " x Repo").appendTo(container);
                        }
                        if (options.data.countPendingCoupon > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingCoupon + " x Coupon").appendTo(container);
                        }
                        if (options.data.countPendingFees > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingFees + " x Fees").appendTo(container);
                        }
                        if (options.data.countPendingMtm > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingMtm + " x MTM").appendTo(container);
                        }
                        if (options.data.countPendingFx > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingFx + " x FX").appendTo(container);
                        }
                        if (options.data.countPendingContribution > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingContribution + " x Contribution").appendTo(container);
                        }
                        if (options.data.countPendingAltid > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingAltid + " x ALTID").appendTo(container);
                        }
                        if (options.data.countPendingOthers > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingOthers + " x Others").appendTo(container);
                        }

                        if (options.data.countUnclassifiedBond > 0) {
                            $("<span />").addClass("label label-primary").css("margin-left", "2px").text(options.data.countUnclassifiedBond + " x Unclassified Bond").appendTo(container);
                        }
                        if (options.data.countUnclassifiedCoupon > 0) {
                            $("<span />").addClass("label label-primary").css("margin-left", "2px").text(options.data.countUnclassifiedCoupon + " x Unclassified Coupon").appendTo(container);
                        }

                    }
                },
                {
                    caption: "Actions",
                    type: "buttons",
                    buttons: [
                        {
                            hint: "Open Form",
                            icon: "fa fa-external-link",
                            onClick: function (e) {
                                window.location.href = referenceUrl.editTcaTaggingPage + moment(e.row.data.settlementDate).unix() + "/" + e.row.data.currency;
                                e.event.preventDefault();
                            }
                        }
                    ]
                }
            ],
            showBorders: true,
            showColumnLines: true,
            showRowLines: true,
            groupPanel: {
                visible: true
            },
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

        // #endregion DataGrid

        //#region Events


        //#endregion

        //#region Immediate Invocation function


        //#endregion
    });
}(window.jQuery, window, document));