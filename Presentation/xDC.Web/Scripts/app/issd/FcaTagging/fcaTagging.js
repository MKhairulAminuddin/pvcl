(function ($, window, document) {
    $(function () {
        //#region Variable Definition

        app.setSideMenuItemActive("/issd/FcaTagging");

        var $tabpanel,
            $grid,
            $dateSelectionBtn,
            $printBtn;

        var referenceUrl = {

            loadTcaTagging: window.location.origin + "/api/issd/FcaTagging",
            editTcaTaggingPage: window.location.origin + "/issd/FcaTagging/Edit/"

        };

        //#endregion

        //#region Data Source & Functions


        //#endregion

        //#region Other Widgets
        


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
                        if (options.data.countPendingFees > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingFees + " x Fees").appendTo(container);
                        }
                        if (options.data.countPendingAltid > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingAltid + " x ALTID").appendTo(container);
                        }
                        if (options.data.countPendingOthers > 0) {
                            $("<span />").addClass("label label-danger").css("margin-left", "2px").text(options.data.countPendingOthers + " x Others").appendTo(container);
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
            },
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: "xDC_T10_Grid"
            }
        }).dxDataGrid("instance");

        // #endregion DataGrid

        //#region Events
        

        //#endregion

        //#region Immediate Invocation function


        //#endregion
    });
}(window.jQuery, window, document));