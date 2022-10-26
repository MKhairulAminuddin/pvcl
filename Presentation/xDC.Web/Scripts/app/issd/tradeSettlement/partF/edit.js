(function($, window, document) {

    $(function() {
        //#region Variable Definition

        ts.setSideMenuItemActive("/issd/TradeSettlement");

        var $saveAsDraftBtn = $("#saveAsDraftBtn"),
            $adminEditSaveChangesBtn = $("#adminEditSaveChangesBtn"),

            $feesGrid,

            $approverDropdown,
            $approvalNotes,
            
            isDraft = false,
            isAdminEdit = false,
            formTypeId = 8;

        var referenceUrl = {
            submitEditRequest: window.location.origin + "/api/issd/ts/Edit",
            submitEditResponse: window.location.origin + "/issd/TradeSettlement/PartF/View/",

            submitApprovalRequest: window.location.origin + "/api/issd/ts/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/PartF/View/"
        };

        //#endregion


        //#region Data Source & Functions

        var populateData = function() {
            $.when(ts.dsTradeItem("fees"))
                .done(function (fees) {
                    $feesGrid.option("dataSource", fees.data);
                    $feesGrid.repaint();

                    ts.defineTabBadgeNumbers([
                        { titleId: "titleBadge7", dxDataGrid: $feesGrid }
                    ]);
                })
                .then(function() {
                    console.log("Done load data");
                });
        };

        function postData(isDraft, isAdminEdit) {

            var data = {
                id: ts.getIdFromQueryString,
                formType: formTypeId,
                isSaveAsDraft: isDraft,
                isSaveAdminEdit: isAdminEdit,

                fees: $feesGrid.getDataSource().items(),

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
                ts.saveAllGrids($feesGrid);
                
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
                ts.saveAllGrids($feesGrid);

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
