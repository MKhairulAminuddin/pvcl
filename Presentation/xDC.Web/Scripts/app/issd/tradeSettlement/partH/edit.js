﻿(function($, window, document) {

    $(function() {
        //#region Variable Definition

        tradeSettlement.setSideMenuItemActive("/issd/TradeSettlement");

        var $tabpanel,
            
            $othersGrid,

            $tradeSettlementForm,
            
            $approverDropdown,
            $approvalNotes,
            
            isDraft = false,
            isAdminEdit = false,
            formTypeId = 10;

        var referenceUrl = {
            submitEditRequest: window.location.origin + "/api/issd/TradeSettlement/Edit",
            submitEditResponse: window.location.origin + "/issd/TradeSettlement/PartH/View/",

            submitApprovalRequest: window.location.origin + "/api/issd/TradeSettlement/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/PartH/View/"
        };

        //#endregion


        //#region Data Source & Functions

        var populateData = function() {
            $.when(tradeSettlement.dsTradeItem("others"))
                .done(function (others) {
                    $othersGrid.option("dataSource", others.data);
                    $othersGrid.repaint();

                    tradeSettlement.defineTabBadgeNumbers([
                        { titleId: "titleBadge12", dxDataGrid: $othersGrid }
                    ]);
                })
                .then(function() {
                    
                });
        };

        function postData(isDraft, isAdminEdit) {

            var data = {
                id: tradeSettlement.getIdFromQueryString,
                formType: formTypeId,
                isSaveAsDraft: isDraft,
                isSaveAdminEdit: isAdminEdit,
                
                others: $othersGrid.getDataSource().items(),

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
                { titleId: "titleBadge12", title: "Others", template: "othersTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });

        $approverDropdown = $("#approverDropdown").dxSelectBox(tradeSettlement.submitApproverSelectBox).dxSelectBox("instance");

        $approvalNotes = $("#approvalNotes").dxTextArea(tradeSettlement.submitApprovalNotesTextArea).dxTextArea("instance");

        //#endregion

        // #region Data Grid

        $othersGrid = $("#othersGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    caption: "#",
                    cellTemplate: function (cellElement, cellInfo) {
                        cellElement.text(cellInfo.row.rowIndex + 1);
                    },
                    allowEditing: false,
                    width: "30px"
                },
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
                    caption: "Others"
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
                    },
                    {
                        column: "amountMinus",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ]
            },
            onSaved: function () {
                tradeSettlement.defineTabBadgeNumbers([
                    { titleId: "titleBadge12", dxDataGrid: $othersGrid }
                ]);
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            },
            paging: {
                enabled: false
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
                tradeSettlement.saveAllGrids($othersGrid);
                
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
                tradeSettlement.saveAllGrids($othersGrid);

                if ($approverDropdown.option("value") != null) {
                    setTimeout(function () { postData(false, false); },
                        1000);
                } else {
                    alert("Please select an approver");
                }

                e.preventDefault();
            }
        });
        
        //#endregion

        //#region Immediate Invocation function

        populateData();

        //#endregion

    });
}(window.jQuery, window, document));