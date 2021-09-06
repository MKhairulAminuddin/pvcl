﻿(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        
        tradeSettlement.setSideMenuItemActive("/issd/TradeSettlement");

        var $tabpanel,
            $bondGrid,
            $cpGrid,
            $notesPaperGrid,
            $repoGrid,
            $couponGrid,

            $workflowGrid,

            $tradeSettlementForm,
            $currencySelectBox,
            $approverDropdown,
            $printBtn;

        var referenceUrl = {
            adminEdit: window.location.origin + "/issd/TradeSettlement/PartB/Edit/",

            submitApprovalRequest: window.location.origin + "/api/issd/TradeSettlement/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/PartB/View/"
        };

        //#endregion

        //#region Data Source & Functions
        
        var populateData = function () {
            $.when(
                    tradeSettlement.dsTradeItem("bond"),
                    tradeSettlement.dsTradeItem("cp"),
                    tradeSettlement.dsTradeItem("notesPaper"),
                    tradeSettlement.dsTradeItem("coupon")
                )
                .done(function (bond, cp, notesPaper, coupon) {
                    $bondGrid.option("dataSource", bond[0].data);
                    $bondGrid.repaint();

                    $cpGrid.option("dataSource", cp[0].data);
                    $cpGrid.repaint();

                    $notesPaperGrid.option("dataSource", notesPaper[0].data);
                    $notesPaperGrid.repaint();

                    $couponGrid.option("dataSource", coupon[0].data);
                    $couponGrid.repaint();

                    tradeSettlement.defineTabBadgeNumbers([
                        { titleId: "titleBadge2", dxDataGrid: $bondGrid },
                        { titleId: "titleBadge3", dxDataGrid: $cpGrid },
                        { titleId: "titleBadge4", dxDataGrid: $notesPaperGrid },
                        { titleId: "titleBadge6", dxDataGrid: $couponGrid }
                    ]);
                })
                .then(function () {
                    
                });
        };

        var submitApprovalRequest = function (isToApprove) {
            var data = {
                approvalNote: (isToApprove)
                    ? $("#approvalNoteTextBox").dxTextArea("instance").option("value")
                    : $("#rejectionNoteTextBox").dxTextArea("instance").option("value"),
                approvalStatus: isToApprove,
                formId: tradeSettlement.getIdFromQueryString
            };
            
            $.ajax({
                data: data,
                dataType: "json",
                url: referenceUrl.submitApprovalRequest,
                method: "post",
                error: function (jqXHR, textStatus, errorThrown) {
                    app.alertError(errorThrown + ": " + jqXHR.responseJSON);
                },
                success: function (data) {
                    window.location.href = referenceUrl.submitApprovalResponse + data;
                }
            });
        }

        //#endregion
        
        //#region Other Widgets
        
        $printBtn = $("#printBtn").dxDropDownButton(tradeSettlement.printBtnWidgetSetting).dxDropDownButton("instance");
        
        $tabpanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { titleId: "titleBadge2", title: "Bond", template: "bondTab" },
                { titleId: "titleBadge3", title: "CP", template: "cpTab" },
                { titleId: "titleBadge4", title: "Notes & Papers", template: "notesPaperTab" },
                { titleId: "titleBadge6", title: "Coupon", template: "couponReceivedTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });
        
        //#endregion
        
        // #region DataGrid

        $bondGrid = $("#bondGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "Bond",
                    allowEditing: false
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN",
                    allowEditing: false
                },
                {
                    dataField: "maturity",
                    caption: "Maturity (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "sales",
                    caption: "Sales (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "purchase",
                    caption: "Purchase (-)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "remarks",
                    caption: "Remarks",
                    dataType: "text"
                },
                {
                    dataField: "modifiedBy",
                    caption: "Modified"
                },
                {
                    dataField: "modifiedDate",
                    caption: "Modified Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm a"
                }
            ],
            summary: {
                totalItems: [
                    {
                        column: "instrumentCode",
                        displayFormat: "TOTAL"
                    },
                    {
                        column: "maturity",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "sales",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "purchase",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ]
            }
        }).dxDataGrid("instance");

        $cpGrid = $("#cpGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "CP",
                    allowEditing: false
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN",
                    allowEditing: false
                },
                {
                    dataField: "maturity",
                    caption: "Maturity (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "sales",
                    caption: "Sales (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "purchase",
                    caption: "Purchase (-)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "remarks",
                    caption: "Remarks",
                    dataType: "text"
                },
                {
                    dataField: "modifiedBy",
                    caption: "Modified"
                },
                {
                    dataField: "modifiedDate",
                    caption: "Modified Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm a"
                }
            ],
            summary: {
                totalItems: [
                    {
                        column: "instrumentCode",
                        displayFormat: "TOTAL"
                    },
                    {
                        column: "maturity",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "sales",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "purchase",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ]
            }
        }).dxDataGrid("instance");

        $notesPaperGrid = $("#notesPaperGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "Notes & Papers",
                    allowEditing: false
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN",
                    allowEditing: false
                },
                {
                    dataField: "maturity",
                    caption: "Maturity (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "sales",
                    caption: "Sales (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "purchase",
                    caption: "Purchase (-)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "remarks",
                    caption: "Remarks",
                    dataType: "text"
                },
                {
                    dataField: "modifiedBy",
                    caption: "Modified"
                },
                {
                    dataField: "modifiedDate",
                    caption: "Modified Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm a"
                }
            ],
            summary: {
                totalItems: [
                    {
                        column: "instrumentCode",
                        displayFormat: "TOTAL"
                    },
                    {
                        column: "maturity",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "sales",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "purchase",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ]
            }
        }).dxDataGrid("instance");

        $couponGrid = $("#couponGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "Coupon Received"
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN"
                },
                {
                    dataField: "amountPlus",
                    caption: "Amount (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "remarks",
                    caption: "Remarks",
                    dataType: "text"
                },
                {
                    dataField: "modifiedBy",
                    caption: "Modified"
                },
                {
                    dataField: "modifiedDate",
                    caption: "Modified Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm a"
                }
            ],
            summary: {
                totalItems: [
                    {
                        column: "instrumentCode",
                        displayFormat: "TOTAL"
                    },
                    {
                        column: "amountPlus",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ]
            }
        }).dxDataGrid("instance");

        $workflowGrid = $("#workflowGrid").dxDataGrid({
            dataSource: tradeSettlement.dsWorflowInformation(4),
            columns: [
                {
                    dataField: "recordedDate",
                    caption: "Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy hh:mm a"
                },
                {
                    dataField: "requestBy",
                    caption: "Requested By"
                },
                {
                    dataField: "requestTo",
                    caption: "Requested To"
                },
                {
                    dataField: "workflowStatus",
                    caption: "Workflow Status"
                },
                {
                    dataField: "workflowNotes",
                    caption: "Notes"
                }
            ],
            showRowLines: true,
            rowAlternationEnabled: false,
            showBorders: true,
            sorting: {
                mode: "multiple",
                showSortIndexes: true
            },
            wordWrapEnabled: true
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

        populateData();

        //#endregion
    });
}(window.jQuery, window, document));