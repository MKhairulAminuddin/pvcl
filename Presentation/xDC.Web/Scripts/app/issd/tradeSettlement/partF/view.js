﻿(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        
        tradeSettlement.setSideMenuItemActive("/issd/TradeSettlement");

        var $tabpanel,
            $feesGrid,
            $workflowGrid,

            $tradeSettlementForm,
            $currencySelectBox,
            $approverDropdown,
            $printBtn;

        var referenceUrl = {
            adminEdit: window.location.origin + "/issd/TradeSettlement/PartE/Edit/",

            submitApprovalRequest: window.location.origin + "/api/issd/TradeSettlement/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/PartE/View/"
        };

        //#endregion

        //#region Data Source & Functions

        var populateData = function() {
            $.when(tradeSettlement.dsTradeItem("fees"))
                .done(function (fees) {
                    $feesGrid.option("dataSource", fees.data);
                    $feesGrid.repaint();

                    tradeSettlement.defineTabBadgeNumbers([
                        { titleId: "titleBadge7", dxDataGrid: $feesGrid }
                    ]);
                })
                .then(function() {
                    console.log("Done load data");
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
                    $("#error_container").bs_alert(errorThrown + ": " + jqXHR.responseJSON);
                },
                success: function (data) {
                    window.location.href = referenceUrl.submitApprovalResponse + data;
                }
            });;
        }

        //#endregion
        
        //#region Other Widgets
        
        $printBtn = $("#printBtn").dxDropDownButton(tradeSettlement.printBtnWidgetSetting).dxDropDownButton("instance");
        
        $tabpanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { titleId: "titleBadge7", title: "Fees", template: "feesTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });
        
        //#endregion
        
        // #region DataGrid

        $feesGrid = $("#feesGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "id",
                    caption: "Id",
                    visible: false
                },
                {
                    dataField: "formId",
                    caption: "Form Id",
                    visible: false
                },
                {
                    dataField: "instrumentCode",
                    caption: "Fees"
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
                    dataField: "amountMinus",
                    caption: "Amount (-)",
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
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: tradeSettlement.api.loadWorkflowHistory + "/7"
            }),
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