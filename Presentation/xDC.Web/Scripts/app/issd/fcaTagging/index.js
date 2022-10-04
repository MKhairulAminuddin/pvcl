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

            loadFcaTagging: window.location.origin + "/api/issd/FcaTagging",
            viewFcaTaggingPage: window.location.origin + "/issd/FcaTagging/View/",
            editFcaTaggingPage: window.location.origin + "/issd/FcaTagging/Edit/"

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
                    ["settlementDate", ">=", moment().startOf("day").toDate()],
                    "and",
                    ["settlementDate", "<", moment().add(1, "days").toDate()]
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
                loadUrl: referenceUrl.loadFcaTagging
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
                        if (options.data.countPendingFees > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingFees + " x Fees").appendTo(container);
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
                            hint: "Edit Tagging",
                            icon: "fa fa-tag",
                            onClick: function (e) {
                                window.location.href = referenceUrl.editFcaTaggingPage + moment(e.row.data.settlementDate).unix() + "/" + e.row.data.currency;
                                e.event.preventDefault();
                            }
                        },
                        {
                            hint: "View Form",
                            icon: "fa fa-eye",
                            onClick: function (e) {
                                window.location.href = referenceUrl.viewFcaTaggingPage + moment(e.row.data.settlementDate).unix() + "/" + e.row.data.currency;
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