(function ($, window, document) {

    $(function () {
        var $inflowFundsGrid, $printBtn, $tbFormId, $tbFormStatus, $workflowGrid;

        $("#approveBtn").on({
            "click": function (e) {
                $('#approvalNoteModal').modal('show');

                e.preventDefault();
            }
        });

        $("#rejectBtn").on({
            "click": function (e) {
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
                    DevExpress.ui.notify("Download " + e.itemData, "success", 600);

                    var data = {
                        id: getUrlParameter("id"),
                        isExportAsExcel: true
                    };

                    $.ajax({
                        type: "POST",
                        url: '/amsd/PrintInflowFund',
                        data: data,
                        dataType: "text",
                        success: function(data) {
                            var url = '/amsd/GetPrintInflowFund?id=' + data;
                            window.location = url;
                        }
                    });
                    e.event.preventDefault();
                } else {
                    DevExpress.ui.notify("Download " + e.itemData, "success", 600);

                    var data = {
                        id: getUrlParameter("id"),
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
                loadUrl: "../api/amsd/GetInflowFunds?id=" + getUrlParameter('id')
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
                loadUrl: "../api/common/GetWorkflow?id=" + getUrlParameter('id')
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
            allowColumnReordering: true,
            allowColumnResizing: true,
            sorting: {
                mode: "multiple",
                showSortIndexes: true
            },
            pager: {
                infoText: "Page {0} of {1} ({2} items)",
                showPageSizeSelector: true,
                allowedPageSizes: [10, 20, 50],
                showNavigationButtons: true,
                showInfo: true
            },
            paging: {
                pageSize: 10,
                pageIndex: 0
            },
            wordWrapEnabled: true
        }).dxDataGrid("instance");
        
        $("#approveFormBtn").on({
            "click": function (e) {
                
                var data = {
                    approvalNote: $("#approvalNoteTextBox").dxTextArea("instance").option('value'),
                    approvalStatus: true,
                    formId: getUrlParameter('id')
                };

                $.ajax({
                    data: data,
                    dataType: 'json',
                    url: '../api/amsd/InflowFundsFormApproval',
                    method: 'post'
                }).done(function (data) {
                    window.location.href = "../amsd/InflowFundsFormStatus?id=" + data;

                }).fail(function (jqXHR, textStatus, errorThrown) {
                    $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                });

                e.preventDefault();
            }
        });

        $("#rejectFormBtn").on({
            "click": function (e) {

                var data = {
                    approvalNote: $("#rejectionNoteTextBox").dxTextArea("instance").option('value'),
                    approvalStatus: false,
                    formId: getUrlParameter('id')
                };

                $.ajax({
                    data: data,
                    dataType: 'json',
                    url: '../api/amsd/InflowFundsFormApproval',
                    method: 'post'
                }).done(function (data) {
                    window.location.href = "../amsd/InflowFundsFormStatus?id=" + data;

                }).fail(function (jqXHR, textStatus, errorThrown) {
                    $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                });

                e.preventDefault();
            }
        });



    });
}(window.jQuery, window, document));