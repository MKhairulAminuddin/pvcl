(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        
        ts.setSideMenuItemActive("/issd/TradeSettlement");

        var $tabpanel,
            $repoGrid,

            $workflowGrid,
            $auditTrailGrid,
            $tradeSettlementForm,
            $currencySelectBox,
            $approverDropdown,
            $printBtn,
            $approvalReassignModal = $("#approvalReassignModal"),
            $viewAuditTrailModal = $("#viewAuditTrailModal");

        var referenceUrl = {
            adminEdit: window.location.origin + "/issd/TradeSettlement/PartF/Edit/",

            submitApprovalRequest: window.location.origin + "/api/issd/ts/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/PartC/View/"
        };

        //#endregion

        //#region Data Source & Functions

        var dsApproverList = function () {
            return {
                store: DevExpress.data.AspNet.createStore({
                    key: "id",
                    loadUrl: window.location.origin + "/api/common/GetTradeSettlementApprover"
                }),
                paginate: true,
                pageSize: 20
            };
        }

        var populateData = function () {
            $.when(
                    ts.dsTradeItem("repo")
                )
                .done(function (repo) {
                    $repoGrid.option("dataSource", repo.data);
                    $repoGrid.repaint();

                    ts.defineTabBadgeNumbers([
                        { titleId: "titleBadge5", dxDataGrid: $repoGrid }
                    ]);
                })
                .then(function () {
                    console.log("Done load data");
                });
        };

        var submitApprovalRequest = function (isToApprove) {
            var data = {
                approvalNote: (isToApprove)
                    ? $("#approvalNoteTextBox").dxTextArea("instance").option("value")
                    : $("#rejectionNoteTextBox").dxTextArea("instance").option("value"),
                approvalStatus: isToApprove,
                formId: ts.getIdFromQueryString
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

        $approverDropdown = $("#newApproverDropdown").dxSelectBox({
            dataSource: dsApproverList(),
            displayExpr: "displayName",
            valueExpr: "username",
            searchEnabled: true,
            itemTemplate: function (data) {
                return "<div class='active-directory-dropdown'>" +
                    "<p class='active-directory-title'>" + data.displayName + "</p>" +
                    "<p class='active-directory-subtitle'>" + data.title + ", " + data.department + "</p>" +
                    "<p class='active-directory-subtitle'>" + data.email + "</p>" +
                    "</div>";
            }
        }).dxSelectBox("instance");

        $("#approvalReassignModalBtn").dxButton({
            onClick: function (e) {
                if ($approverDropdown.option("value") != null) {
                    //reassign
                    app.toast("Reassinging...");

                    var data = {
                        formId: app.getUrlId(),
                        approver: $approverDropdown.option("value"),
                        formType: 2
                    };

                    $.ajax({
                        type: "POST",
                        url: ts.api.reassignApprover,
                        data: data,
                        dataType: "text",
                        success: function (data) {
                            setTimeout(function () {
                                app.toast("Reassigned to new approver", "success");
                                location.reload();
                            }, 2000);
                        },
                        fail: function (jqXHR, textStatus, errorThrown) {
                            app.alertError("Reassignment failed..");
                        },
                        complete: function (data) {
                            $approverDropdown.option("value", "");
                            $approvalReassignModal.modal("hide");
                        }
                    });

                } else {
                    alert("Please select one approver to reassign to.");
                }
            }
        });
        
        $printBtn = $("#printBtn").dxDropDownButton(ts.printBtnWidgetSetting).dxDropDownButton("instance");
        
        $tabpanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { titleId: "titleBadge5", title: "REPO", template: "repoTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });
        
        //#endregion
        
        // #region DataGrid

        $repoGrid = $("#repoGrid").dxDataGrid({
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
                    dataField: "instrumentCode",
                    caption: "REPO",
                    allowEditing: false
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN",
                    allowEditing: false
                },
                {
                    dataField: "firstLeg",
                    caption: "1st Leg (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "secondLeg",
                    caption: "2nd Leg (-)",
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
                        column: "firstLeg",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "secondLeg",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ]
            },
            paging: {
                enabled: false
            }
        }).dxDataGrid("instance");
        
        $workflowGrid = $("#workflowGrid").dxDataGrid({
            dataSource: ts.dsWorflowInformation(5),
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

        $auditTrailGrid = $("#auditTrailGrid").dxDataGrid({
            dataSource: ts.dsAuditTrail(5),
            columns: [
                {
                    dataField: "actionType",
                    caption: "Action"
                },
                {
                    dataField: "formType",
                    caption: "Form Type",
                    visible: false
                },
                {
                    dataField: "formDate",
                    caption: "Form Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    visible: false
                },
                {
                    dataField: "modifiedOn",
                    caption: "Performed On",
                    dataType: "datetime",
                    format: "dd/MM/yyyy hh:mm:ss",
                    sortIndex: 0,
                    sortOrder: "desc"
                },
                {
                    dataField: "modifiedBy",
                    caption: "User ID"
                },
                {
                    dataField: "remarks",
                    caption: "Remarks"
                },
                {
                    dataField: "valueBefore",
                    caption: "Value Before"
                },
                {
                    dataField: "valueAfter",
                    caption: "Value After"
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

        $("#reassignBtn").dxButton({
            onClick: function (e) {
                $approvalReassignModal.modal("show");
                e.event.preventDefault();
            }
        });

        $("#viewAuditTrailBtn").on({
            "click": function (e) {
                $viewAuditTrailModal.modal("show");
                e.preventDefault();
            }
        });

        $("#viewWorkflowBtn").on({
            "click": function (e) {
                $('#viewWorkflowModal').modal('show');

                e.preventDefault();
            }
        });

        $("#adminEditBtn").on({
            "click": function (e) {
                window.location.href = referenceUrl.adminEdit + ts.getIdFromQueryString;
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