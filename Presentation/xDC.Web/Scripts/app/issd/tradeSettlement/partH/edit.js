﻿(function($, window, document) {

    $(function() {
        //#region Variable Definition

        ts.setSideMenuItemActive("/issd/TradeSettlement");

        var $saveAsDraftBtn = $("#saveAsDraftBtn"),
            $adminEditSaveChangesBtn = $("#adminEditSaveChangesBtn"),
            
            $othersGrid,

            $approverDropdown,
            $approvalNotes,
            
            isDraft = false,
            isAdminEdit = false,
            formTypeId = 10;

        var referenceUrl = {
            submitEditRequest: window.location.origin + "/api/issd/ts/Edit",
            submitEditResponse: window.location.origin + "/issd/TradeSettlement/View/",

            submitApprovalRequest: window.location.origin + "/api/issd/ts/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/View/"
        };

        //#endregion


        //#region Data Source & Functions

        var populateData = function() {
            $.when(ts.dsTradeItem("others"))
                .done(function (others) {
                    $othersGrid.option("dataSource", others.data);
                    $othersGrid.repaint();

                    ts.defineTabBadgeNumbers([
                        { titleId: "titleBadge12", dxDataGrid: $othersGrid }
                    ]);
                })
                .then(function() {
                    
                });
        };

        function postData(isDraft, isAdminEdit) {

            var data = {
                id: ts.getIdFromQueryString,
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

        $approverDropdown = $("#approverDropdown").dxSelectBox(ts.submitApproverSelectBox).dxSelectBox("instance");

        $approvalNotes = $("#approvalNotes").dxTextArea(ts.submitApprovalNotesTextArea).dxTextArea("instance");

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
                    caption: "Others",
                    validationRules: [{ type: 'required' }]
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
                    dataField: "othersType",
                    caption: "Types",
                    lookup: {
                        dataSource: ["Loan", "Property", "Others"]
                    },
                    validationRules: [{ type: 'required' }]
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
                ts.defineTabBadgeNumbers([
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

        $saveAsDraftBtn.dxButton({
            onClick: function (e) {
                isDraft = true;
            }
        });

        $adminEditSaveChangesBtn.dxButton({
            onClick: function (e) {
                isAdminEdit = true;
            }
        });

        $tradeSettlementForm = $("#tradeSettlementForm").on("submit",
            function(e) {
                ts.saveAllGrids($othersGrid);
                
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
                ts.saveAllGrids($othersGrid);

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
