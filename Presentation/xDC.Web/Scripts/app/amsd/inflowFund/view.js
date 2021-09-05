(function ($, window, document) {

    $(function () {
        //#region Variables

        var $inflowFundsGrid, $printBtn, $workflowGrid, $approvalNoteModal, $rejectionNoteModal, $viewWorkflowModal;

        $approvalNoteModal = $("#approvalNoteModal");
        $rejectionNoteModal = $("#rejectionNoteModal");
        $viewWorkflowModal = $("#viewWorkflowModal");

        var referenceUrl = {
            loadWorkflow: window.location.origin + "/api/common/GetWorkflow/1/" + app.getUrlId(),
            loadGrid: window.location.origin + "/api/amsd/GetInflowFunds/" + app.getUrlId(),

            checkCutOffTime: window.location.origin + "/api/amsd/IsViolatedCutOffTime",

            approvalRequest: window.location.origin + "/api/amsd/InflowFund/Approval",
            approvalResponse: window.location.origin + "/amsd/inflowfund/view/",

            printRequest: window.location.origin + "/amsd/Print",
            printResponse: window.location.origin + "/amsd/Printed/"
        };

        //#endregion
        

        //#region Data Source & Functions

        var cutOffTimeChecker = function() {
            $.ajax({
                dataType: "json",
                url: referenceUrl.checkCutOffTime,
                method: "get",
                success: function (data) {
                    if (data) {
                        $(".cutOffTimeNotify").text("Cut Off Time Violated").addClass("label label-danger");
                    } else {
                        $(".cutOffTimeNotify").text("").removeClass("label label-danger");
                    }
                },
                fail: function (jqXHR, textStatus, errorThrown) {

                },
                complete: function (data) {

                }
            });
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

        $("#approveBtn").on({
            "click": function (e) {
                cutOffTimeChecker();
                $approvalNoteModal.modal("show");
                e.preventDefault();
            }
        });

        $("#rejectBtn").on({
            "click": function (e) {
                cutOffTimeChecker();
                $rejectionNoteModal.modal("show");
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