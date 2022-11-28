(function ($, window, document) {

    $(function () {

        //#region Fields

        var $inflowFundsGrid,
            $printBtn,
            $workflowGrid,
            $auditTrailGrid,
            $approvalNoteModal,
            $rejectionNoteModal,
            $viewWorkflowModal,
            $viewAuditTrailModal,
            $editDraftForm = $("#editDraftForm"),
            $approvalReassignModal = $("#approvalReassignModal");

        $approvalNoteModal = $("#approvalNoteModal");
        $rejectionNoteModal = $("#rejectionNoteModal");
        $viewAuditTrailModal = $("#viewAuditTrailModal");
        $viewWorkflowModal = $("#viewWorkflowModal");

        var referenceUrl = {
            loadWorkflow: window.location.origin + "/api/common/GetWorkflow/1/" + app.getUrlId(),
            loadAuditTrail: window.location.origin + "/api/common/FormAuditTrail/1/" + app.getUrlId(),

            loadGrid: window.location.origin + "/api/amsd/InflowFund/Items/" + app.getUrlId(),

            approvalReassign: window.location.origin + "/api/common/reassignApprover",

            approvalRequest: window.location.origin + "/api/amsd/InflowFund/Approval",
            approvalResponse: window.location.origin + "/amsd/inflowfund/view/",

            printRequest: window.location.origin + "/amsd/inflowfund/Print",
            printResponse: window.location.origin + "/amsd/inflowfund/Printed/",

            editDraftUrl: window.location.origin + "/amsd/inflowfund/Edit/",
        };

        //#endregion
        

        //#region Data Source & Functions

        var dsApproverList = function () {
            return {
                store: DevExpress.data.AspNet.createStore({
                    key: "id",
                    loadUrl: window.location.origin + "/api/common/GetApproverAmsdInflowFunds"
                }),
                paginate: true,
                pageSize: 20
            };
        }

        var postApproval = function(isApproved) {
            var data = {
                approvalNote: (isApproved)
                    ? $("#approvalNoteTextBox").dxTextArea("instance").option("value")
                    : $("#rejectionNoteTextBox").dxTextArea("instance").option("value"),
                approvalStatus: isApproved,
                formId: app.getUrlId()
            };

            $.ajax({
                data: data,
                dataType: "json",
                url: referenceUrl.approvalRequest,
                method: "post",
                success: function (data) {
                    window.location.href = referenceUrl.approvalResponse + data;
                },
                fail: function (jqXHR, textStatus, errorThrown) {
                    app.alertError(textStatus + ": " + errorThrown);
                },
                complete: function (data) {
                    if (isApproved) {
                        $approvalNoteModal.modal("hide");
                    } else {
                        $rejectionNoteModal.modal("hide");
                    }
                }
            });
        }

        //#endregion


        //#region Widgets

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

        $editDraftForm.dxButton({
            onClick: function (e) {
                window.location.href = referenceUrl.editDraftUrl + app.getUrlId();

                e.event.preventDefault();
            }
        }) 

        $("#approvalReassignModalBtn").dxButton({
            onClick: function (e) {
                if ($approverDropdown.option("value") != null) {
                    //reassign
                    app.toast("Reassinging...");

                    var data = {
                        formId: app.getUrlId(),
                        approver: $approverDropdown.option("value"),
                        formType: 1
                    };

                    $.ajax({
                        type: "POST",
                        url: referenceUrl.approvalReassign,
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

        $printBtn = $("#printBtn").dxDropDownButton({
            text: "Print",
            icon: "print",
            type: "normal",
            stylingMode: "contained",
            dropDownOptions: {
                width: 230
            },
            displayExpr: "name",
            keyExpr: "id",
            items: [
                { id: 1, name: "Excel Workbook (*.xlsx)", icon: "fa fa-file-excel-o" },
                { id: 2, name: "PDF", icon: "fa fa-file-pdf-o" }
            ],
            onItemClick: function (e) {
                app.toast("Generating...");

                var data = {
                    id: app.getUrlId(),
                    isExportAsExcel: (e.itemData.id == 1)
                };

                $.ajax({
                    type: "POST",
                    url: referenceUrl.printRequest,
                    data: data,
                    dataType: "text",
                    success: function (data) {
                        var url = referenceUrl.printResponse + data;
                        window.location = url;
                    },
                    fail: function (jqXHR, textStatus, errorThrown) {
                        app.alertError(textStatus + ": " + errorThrown);
                    },
                    complete: function (data) {

                    }
                });

                e.event.preventDefault();
            }
        }).dxDropDownButton("instance");
        
        //#endregion

        //#region DataGrid

        $inflowFundsGrid = $("#inflowFundsGrid1").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.loadGrid
            }),
            columns: [
                {
                    dataField: "fundType",
                    caption: "Fund Types"
                },
                {
                    dataField: "bank",
                    caption: "Bank"
                },
                {
                    dataField: "amount",
                    caption: "Amount",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                }
            ],
            summary: {
                totalItems: [
                    {
                        column: "type",
                        displayFormat: "TOTAL"
                    },
                    {
                        column: "amount",
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

        $inflowFundsGrid.option(dxGridUtils.viewOnlyGridConfig);

        $workflowGrid = $("#workflowGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.loadWorkflow
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
            showBorders: true,
            sorting: {
                mode: "multiple",
                showSortIndexes: true
            },
            wordWrapEnabled: true
        }).dxDataGrid("instance");

        $auditTrailGrid = $("#auditTrailGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.loadAuditTrail
            }),
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

        //#endregion

        //#region Event & Invocation

        $("#reassignBtn").dxButton({
            onClick: function (e) {
                $approvalReassignModal.modal("show");
                e.event.preventDefault();
            }
        });

        $("#approveBtn").on({
            "click": function (e) {
                $approvalNoteModal.modal("show");
                e.preventDefault();
            }
        });

        $("#rejectBtn").on({
            "click": function (e) {
                $rejectionNoteModal.modal("show");
                e.preventDefault();
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
                $viewWorkflowModal.modal("show");
                e.preventDefault();
            }
        });

        $("#approveFormBtn").on({
            "click": function (e) {
                postApproval(true);

                e.preventDefault();
            }
        });

        $("#rejectFormBtn").on({
            "click": function (e) {
                postApproval(false);

                e.preventDefault();
            }
        });

        //#endregion

    });
}(window.jQuery, window, document));