(function ($, window, document) {

    $(function () {
        var $inflowFundsGrid, $printBtn, $tbFormId, $tbFormStatus;

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

        $printBtn = $("#printBtn").dxDropDownButton({
            text: "Print",
            icon: "print",
            type: "normal",
            stylingMode: "contained",
            dropDownOptions: {
                width: 230
            },
            onItemClick: function (e) {
                if (e.itemData == "Excel Workbook (*.xlsx)") {
                    DevExpress.ui.notify("Download " + e.itemData, "success", 600);

                    var data = {
                        id: getUrlParameter("id")
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
            items: [
                "Excel Workbook (*.xlsx)",
                "PDF"
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