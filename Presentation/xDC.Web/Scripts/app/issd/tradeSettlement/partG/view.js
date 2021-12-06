(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        
        ts.setSideMenuItemActive("/issd/TradeSettlement");

        var $tabpanel,
            
            $cnGrid,

            $workflowGrid,

            $tradeSettlementForm,
            $currencySelectBox,
            $approverDropdown,
            $printBtn,
            $cnEmailBtn,
            $approvalReassignModal = $("#approvalReassignModal");

        var referenceUrl = {
            adminEdit: window.location.origin + "/issd/TradeSettlement/PartG/Edit/",

            submitApprovalRequest: window.location.origin + "/api/issd/TradeSettlement/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/PartG/View/",

            generateCnEmail: window.location.origin + "/issd/TradeSettlement/GenerateCnEmail",
            retrieveCnEmail: window.location.origin + "/issd/TradeSettlement/RetrieveCnEmail?referenceId="
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

        var populateData = function() {
            $.when(ts.dsTradeItem("contributionCredited"))
                .done(function(cn) {
                    $cnGrid.option("dataSource", cn.data);
                    $cnGrid.repaint();

                    ts.defineTabBadgeNumbers([
                        { titleId: "titleBadge10", dxDataGrid: $cnGrid }
                    ]);
                })
                .then(function() {
                    
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
            });;
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
                        url: window.location.origin + "/api/common/reassignApprover",
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

        $("#cnEmailBtn").dxButton({
            text: "Query Email",
            icon: "fa fa-paper-plane-o",
            stylingMode: "outlined",
            type: "default",
            onClick: function () {
                var req = {
                    "formId": ts.getIdFromQueryString
                };

                $.ajax({
                    type: "POST",
                    url: referenceUrl.generateCnEmail,
                    data: JSON.stringify(req),
                    contentType: "application/json",
                    dataType: "text",
                    success: function (data) {
                        var url = referenceUrl.retrieveCnEmail + data;
                        window.location = url;
                    }
                });
            }
        });

        $printBtn = $("#printBtn").dxDropDownButton(ts.printBtnWidgetSetting).dxDropDownButton("instance");
        
        $tabpanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { titleId: "titleBadge10", title: "Contribution", template: "contributionCreditedTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });
        
        //#endregion
        
        // #region DataGrid
        
        $cnGrid = $("#contributionCreditedGrid").dxDataGrid({
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
                    caption: "Contribution Credited"
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
            },
            paging: {
                enabled: false
            }
        }).dxDataGrid("instance");
        
        $workflowGrid = $("#workflowGrid").dxDataGrid({
            dataSource: ts.dsWorflowInformation(9),
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

        $("#reassignBtn").dxButton({
            onClick: function (e) {
                $approvalReassignModal.modal("show");
                e.event.preventDefault();
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