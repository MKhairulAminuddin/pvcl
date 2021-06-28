(function ($, window, document) {

    $(function () {
        var inflowFundsTypes = ["Funds Received", "Maturity", "Proceed", "Interest Received"];
        var inflowFundsBank = ["RHB Bank -01", "RHB Bank -02", "RHB Bank -03", "BNM -01", "BA", "CP Matured", " Cagamas Int", "Loan SPnb", "Bon Int"];
        
        var $inflowFundsGrid, $historyBtn, $cancelBtn, $submitBtn, $tbFormId, $tbFormStatus;


        $tbFormId = $("#tbFormId").dxTextBox({
            disabled: true
        }).dxTextBox("instance");

        $tbFormStatus = $("#tbFormStatus").dxTextBox({
            value: "New",
            disabled: true
        }).dxTextBox("instance");

        $historyBtn = $("#historyBtn").dxButton({
            type: "default",
            icon: "detailslayout"
        }).dxButton("instance");

        $cancelBtn = $("#cancelBtn").dxButton({
            type: "normal",
            text: "Cancel",
            onClick: function (event) {
                window.location.replace("../amsd");
                event.preventDefault();
            }
        }).dxButton("instance");
        
        $submitBtn = $("#submitBtn").dxButton({
            type: "default",
            text: "Submit",
            useSubmitBehavior: false,
            onClick: function () {
                if (jQuery.isEmptyObject($inflowFundsGrid.getDataSource().items())) {
                    $("#error_container").bs_warning("Please key in at least one item.");
                }
                else
                {
                    var data = {
                        formType: $("input[name='formType']").val(),
                        amsdInflowFunds: $inflowFundsGrid.getDataSource().items()
                    };

                    $.ajax({
                        data: data,
                        dataType: 'json',
                        url: '../api/amsd/NewInflowFundsForm',
                        method: 'post'
                    }).done(function(data) {
                        window.location.href = "../amsd/ViewInflowFundsForm?id=" + data;

                    }).fail(function(jqXHR, textStatus, errorThrown) {
                        $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                    });
                }
            }
        }).dxButton("instance");

        $inflowFundsGrid = $("#inflowFundsGrid1").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "fundType",
                    caption: "Fund Types",
                    lookup: {
                        dataSource: inflowFundsTypes
                    }
                },
                {
                    dataField: "bank",
                    caption: "Bank",
                    lookup: {
                        dataSource: inflowFundsBank
                    }
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
            showBorders: true,
            height: 300,
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            },
            onRowUpdated: function(e) {
                console.log(e);
            },
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
        
        
    });
}(window.jQuery, window, document));