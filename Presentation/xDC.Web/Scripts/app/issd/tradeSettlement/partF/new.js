(function ($, window, document) {

    $(function () {
        //#region Variable Definition
        
        ts.setSideMenuItemActive("/issd/TradeSettlement");
        
        var $tabpanel,

            $feesGrid,
            
            $approverDropdown,
            $approvalNotes,

            $settlementDateBox,
            $currencySelectBox,

            $saveAsDraftBtn,
            $submitForApprovalBtn,

            $selectApproverModal = $('#selectApproverModal'),
            $submitForApprovalModalBtn,

            $tradeSettlementForm,
            isSaveAsDraft,
            formTypeId = 8;

        var referenceUrl = {
            postNewFormRequest: window.location.origin + "/api/issd/TradeSettlement/New",
            postNewFormResponse: window.location.origin + "/issd/TradeSettlement/PartF/View/",
        };
        
        //#endregion

        //#region Data Source & Functions

        function postData(isDraft) {
            var data = {
                currency: $currencySelectBox.option("value"),
                settlementDateEpoch: moment($settlementDateBox.option("value")).unix(),
                formType: formTypeId,
                isSaveAsDraft: isDraft,
                
                fees: $feesGrid.getDataSource().items(),

                approver: (isDraft) ? null : $approverDropdown.option("value"),
                approvalNotes: (isDraft) ? null : $approvalNotes.option("value")
            };

            return $.ajax({
                data: data,
                dataType: "json",
                url: referenceUrl.postNewFormRequest,
                method: "post",
                success: function (response) {
                    window.location.href = referenceUrl.postNewFormResponse + response;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    app.alertError(errorThrown + ": " + jqXHR.responseJSON);
                },
                complete: function (data) {
                    
                }
            });
        }

        //#endregion
        
        //#region Other Widgets

        $settlementDateBox = $("#settlementDateBox").dxDateBox(ts.settlementDateBox).dxValidator({
            validationRules: [
                {
                    type: "required",
                    message: "Settlement Date is required"
                }
            ]
        }).dxDateBox("instance");

        $currencySelectBox = $("#currencySelectBox").dxSelectBox(ts.currencySelectBox)
            .dxValidator({
                validationRules: [
                    {
                        type: "required",
                        message: "Currency is required"
                    }
                ]
            })
            .dxSelectBox("instance");

        $tabpanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { titleId: "titleBadge7", title: "Fees", template: "feesTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });

        $approverDropdown = $("#approverDropdown").dxSelectBox(ts.submitApproverSelectBox).dxSelectBox("instance");

        $approvalNotes = $("#approvalNotes").dxTextArea(ts.submitApprovalNotesTextArea).dxTextArea("instance");
        
        //#endregion
        
        // #region Data Grid
        
        $feesGrid = $("#feesGrid").dxDataGrid({
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
            },
            onSaved: function () {
                ts.defineTabBadgeNumbers([
                    { titleId: "titleBadge7", dxDataGrid: $feesGrid }
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
        
        $saveAsDraftBtn = $("#saveAsDraftBtn").on({
            "click": function (e) {
                isSaveAsDraft = true;
            }
        });

        $submitForApprovalBtn = $("#submitForApprovalBtn").on({
            "click": function (e) {
                isSaveAsDraft = false;
            }
        });

        $tradeSettlementForm = $("#tradeSettlementForm").on("submit",
            function (e) {
                ts.saveAllGrids($feesGrid);
                
                if (isSaveAsDraft) {
                    // new clean draft
                    setTimeout(function () {
                        postData(true);
                    }, 1000);
                }
                else {
                    $selectApproverModal.modal('show');
                }

                e.preventDefault();
            });

        $submitForApprovalModalBtn = $("#submitForApprovalModalBtn").on({
            "click": function (e) {
                ts.saveAllGrids($feesGrid);

                if ($approverDropdown.option("value") != null) {

                    app.toast("Submitting for approval....", "info", 3000);

                    setTimeout(function () {
                            postData(false);
                        },
                        1000);
                } else {
                    alert("Please select an approver");
                }

                e.preventDefault();
            }
        });
        
        //#endregion
    });
}(window.jQuery, window, document));