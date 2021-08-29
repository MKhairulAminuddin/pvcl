(function ($, window, document) {

    $(function () {
        var $inflowFundsGrid, $printBtn, $tbFormId, $tbFormStatus, $workflowGrid, $loadPanel;

        $("#approveBtn").on({
            "click": function (e) {
                cutOffTimeChecker();
                $('#approvalNoteModal').modal('show');

                e.preventDefault();
            }
        });

        $("#rejectBtn").on({
            "click": function (e) {
                cutOffTimeChecker();
                $('#rejectionNoteModal').modal('show');

                e.preventDefault();
            }
        });

        $("#viewWorkflowBtn").on({
            "click": function (e) {
                $('#viewWorkflowModal').modal('show');

                e.preventDefault();
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
            onItemClick: function (e) {
                if (e.itemData.id == 1) {
                    $loadPanel.show();

                    var data = {
                        id: app.getUrlParameter("id"),
                        isExportAsExcel: true
                    };

                    $.ajax({
                        type: "POST",
                        url: '/amsd/PrintInflowFund',
                        data: data,
                        dataType: "text",
                        success: function (data) {
                            var url = '/amsd/GetPrintInflowFund?id=' + data;
                            window.location = url;
                        },
                        fail: function (jqXHR, textStatus, errorThrown) {
                            $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                        },
                        complete: function (data) {
                            $loadPanel.hide();
                        }
                    });
                    e.event.preventDefault();
                } else {
                    $loadPanel.show();

                    var data = {
                        id: app.getUrlParameter("id"),
                        isExportAsExcel: false
                    };

                    $.ajax({
                        type: "POST",
                        url: '/amsd/PrintInflowFund',
                        data: data,
                        dataType: "text",
                        success: function (data) {
                            var url = '/amsd/GetPrintInflowFund?id=' + data;
                            window.location = url;
                        },
                        fail: function (jqXHR, textStatus, errorThrown) {
                            $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                        },
                        complete: function (data) {
                            $loadPanel.hide();
                        }
                    });
                    e.event.preventDefault();
                }
                
            },
            displayExpr: "name",
            keyExpr: "id",
            items: [
                { id: 1, name: "Excel Workbook (*.xlsx)", icon: "fa fa-file-excel-o" },
                { id: 4, name: "PDF", icon: "fa fa-file-pdf-o"}
            ]
        }).dxDropDownButton("instance");
        
        $inflowFundsGrid = $("#inflowFundsGrid1").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: window.location.origin + "/api/amsd/GetInflowFunds?id=" + app.getUrlParameter('id')
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
                loadUrl: window.location.origin + "/api/common/GetWorkflow?id=" + app.getUrlParameter('id')
            }),
            columns: [
                {
                    dataField: "requestBy",
                    caption: "Requested By"
                },
                {
                    dataField: "requestTo",
                    caption: "Requested To"
                },
                {
                    dataField: "startDate",
                    caption: "Start Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm"
                },
                {
                    dataField: "endDate",
                    caption: "End Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy HH:mm"
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
        
        $("#approveFormBtn").on({
            "click": function (e) {
                $loadPanel.option("position", { of: "#approvalNoteModalContainer" });
                $loadPanel.show();

                var data = {
                    approvalNote: $("#approvalNoteTextBox").dxTextArea("instance").option('value'),
                    approvalStatus: true,
                    formId: app.getUrlParameter('id')
                };

                $.ajax({
                    data: data,
                    dataType: 'json',
                    url: window.location.origin + '/api/amsd/InflowFundsFormApproval',
                    method: 'post',
                    success: function (data) {
                        window.location.href = window.location.origin + "/amsd/inflowfund/view?id=" + data;
                    },
                    fail: function (jqXHR, textStatus, errorThrown) {
                        $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                    },
                    complete: function (data) {
                        $loadPanel.hide();
                    }
                });

                e.preventDefault();
            }
        });

        $("#rejectFormBtn").on({
            "click": function (e) {
                $loadPanel.option("position", { of: "#rejectionNoteModalContainer" });
                $loadPanel.show();

                var data = {
                    approvalNote: $("#rejectionNoteTextBox").dxTextArea("instance").option('value'),
                    approvalStatus: false,
                    formId: app.getUrlParameter('id')
                };

                $.ajax({
                    data: data,
                    dataType: 'json',
                    url: window.location.origin + '/api/amsd/InflowFundsFormApproval',
                    method: 'post',
                    success: function (data) {
                        window.location.href = window.location.origin + "/amsd/inflowfund/view?id=" + data;
                    },
                    fail: function (jqXHR, textStatus, errorThrown) {
                        $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                    },
                    complete: function (data) {
                        $loadPanel.hide();
                    }
                });

                e.preventDefault();
            }
        });
        
        function cutOffTimeChecker() {
            $.ajax({
                dataType: 'json',
                url: window.location.origin + '/api/amsd/IsViolatedCutOffTime',
                method: 'get',
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

    });
}(window.jQuery, window, document));