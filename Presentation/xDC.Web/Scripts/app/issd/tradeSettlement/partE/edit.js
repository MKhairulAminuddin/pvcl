﻿(function($, window, document) {

    $(function() {
        //#region Variable Definition

        tradeSettlement.setSideMenuItemActive("/issd/TradeSettlement");

        var $tabpanel,
            $altidGrid,

            $tradeSettlementForm,
            
            $approverDropdown,
            $approvalNotes,
            
            isDraft = false,
            isAdminEdit = false,
            formTypeId = 7;

        var referenceUrl = {
            submitEditRequest: window.location.origin + "/api/issd/TradeSettlement/Edit",
            submitEditResponse: window.location.origin + "/issd/TradeSettlement/PartE/View/",

            submitApprovalRequest: window.location.origin + "/api/issd/TradeSettlement/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/PartE/View/"
        };

        //#endregion


        //#region Data Source & Functions

        var populateData = function() {
            $.when(
                tradeSettlement.dsTradeItem("altid")
                )
                .done(function(data1) {
                    $altidGrid.option("dataSource", data1.data);
                    $altidGrid.repaint();

                    tradeSettlement.defineTabBadgeNumbers([
                        { titleId: "titleBadge11", dxDataGrid: $altidGrid }
                    ]);
                })
                .then(function() {
                    console.log("Done load data");
                });
        };

        function postData(isDraft, isAdminEdit) {

            var data = {
                id: tradeSettlement.getIdFromQueryString,
                formType: formTypeId,
                isSaveAsDraft: isDraft,
                isSaveAdminEdit: isAdminEdit,

                altid: $altidGrid.getDataSource().items(),

                approver: (isDraft) ? null : $approverDropdown.option("value"),
                approvalNotes: (isDraft) ? null : $approvalNotes.option("value"),
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
                { titleId: "titleBadge11", title: "ALTID", template: "altidTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });

        $approverDropdown = $("#approverDropdown").dxSelectBox(tradeSettlement.submitApproverSelectBox).dxSelectBox("instance");

        $approvalNotes = $("#approvalNotes").dxTextArea(tradeSettlement.submitApprovalNotesTextArea).dxTextArea("instance");

        //#endregion

        // #region Data Grid
        
        $altidGrid = $("#altidGrid").dxDataGrid({
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
                    caption: "ALTID Distribution & Drawdown"
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
                    { titleId: "titleBadge11", dxDataGrid: $altidGrid }
                ]);
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
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
                tradeSettlement.saveAllGrids($altidGrid);
                
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
                tradeSettlement.saveAllGrids($altidGrid);
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
