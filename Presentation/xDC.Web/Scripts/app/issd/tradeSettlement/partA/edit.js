﻿(function($, window, document) {

    $(function() {
        //#region Variable Definition

        tradeSettlement.setSideMenuItemActive("/issd/TradeSettlement");

        var $tabpanel,
            $equityGrid,
            $tradeSettlementForm,
            
            $approverDropdown,
            $approvalNotes,
            
            isDraft = false,
            isAdminEdit = false,
            formTypeId = 3;

        var referenceUrl = {
            submitEditRequest: window.location.origin + "/api/issd/TradeSettlement/Edit",
            submitEditResponse: window.location.origin + "/issd/TradeSettlement/PartA/View/",

            submitApprovalRequest: window.location.origin + "/api/issd/TradeSettlement/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/PartA/View/"
        };

        //#endregion


        //#region Data Source & Functions
        
        var populateData = function() {
            $.when(tradeSettlement.dsTradeItem("equity"))
                .done(function (equity) {
                    $equityGrid.option("dataSource", equity.data);
                    $equityGrid.repaint();

                    tradeSettlement.defineTabBadgeNumbers([
                        { titleId: "titleBadge1", dxDataGrid: $equityGrid }
                    ]);
                })
                .then(function() {
                    console.log("Done load data");
                });
        }

        function postData(isDraft, isAdminEdit) {

            var data = {
                id: tradeSettlement.getIdFromQueryString,
                formType: formTypeId,
                isSaveAsDraft: isDraft,
                isSaveAdminEdit: isAdminEdit,

                equity: $equityGrid.getDataSource().items(),

                approver: (isDraft) ? null : $approverDropdown.option("value"),
                approvalNotes: (isDraft) ? null : $approvalNotes.option("value")
            };

            $.ajax({
                data: data,
                dataType: "json",
                url: referenceUrl.submitEditRequest,
                method: "post",
                success: function (data) {
                    window.location.href = referenceUrl.submitEditResponse + data;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    app.alertError(errorThrown + ": " + jqXHR.responseJSON);
                },
                complete: function (data) {

                }
            });
        }
        
        //#endregion

        //#region Widgets
        
        $tabpanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { titleId: "titleBadge1", title: "Equity", template: "equityTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });

        $approverDropdown = $("#approverDropdown").dxSelectBox(tradeSettlement.submitApproverSelectBox).dxSelectBox("instance");

        $approvalNotes = $("#approvalNotes").dxTextArea(tradeSettlement.submitApprovalNotesTextArea).dxTextArea("instance");

        //#endregion

        // #region Data Grid
        
        $equityGrid = $("#equityGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "id",
                    caption: "Id",
                    visible: false,
                    allowEditing: false
                },
                {
                    dataField: "formId",
                    caption: "Form Id",
                    visible: false,
                    allowEditing: false
                },
                {
                    dataField: "instrumentCode",
                    caption: "Equity",
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
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: false,
                allowAdding: false
            }
        }).dxDataGrid("instance");

        // #endregion Data Grid

        //#region Events

        $("#saveAsDraftBtn").on({
            "click": function (e) {
                isDraft = true;
            }
        });

        $("#adminEditSaveChangesBtn").on({
            "click": function (e) {
                isAdminEdit = true;
            }
        });

        $tradeSettlementForm = $("#tradeSettlementForm").on("submit",
            function(e) {
                tradeSettlement.saveAllGrids($equityGrid);
                
                if (isDraft || isAdminEdit) {
                    setTimeout(function() {
                        postData(isDraft, isAdminEdit);
                    }, 1000);

                } else {
                    $('#selectApproverModal').modal('show');
                }

                e.preventDefault();
            });

        $("#submitForApprovalModalBtn").on({
            "click": function (e) {
                tradeSettlement.saveAllGrids($equityGrid);
                setTimeout(function() {
                    postData(false, false);
                }, 1000);

                e.preventDefault();
            }
        });
        
        //#endregion

        //#region Immediate Invocation function

        populateData();

        //#endregion

    });
}(window.jQuery, window, document));
