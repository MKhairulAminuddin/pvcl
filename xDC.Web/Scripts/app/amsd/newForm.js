(function ($, window, document) {

    $(function () {
        var inflowFundsTypes = ["Funds Received", "Maturity", "Proceed", "Interest Received"];
        var inflowFundsFunds = ["RHB Bank -01", "RHB Bank -02", "RHB Bank -03", "BNM -01", "BA", "CP Matured", " Cagamas Int", "Loan SPnb", "Bon Int"];

        var inflowFundsGridData = [
            {
                type: "Funds Received",
                fund: "RHB Bank -01",
                amount: 10000256.29
            },
            {
                type: "Funds Received",
                fund: "RHB Bank -02",
                amount: 0
            },
            {
                type: "Funds Received",
                fund: "RHB Bank -03",
                amount: 0
            }
        ];


        var $inflowFundsGrid, $outflowFundsGrid1, $outflowFundsGrid2;


        $("#tbFormId").dxTextBox({
            value: "1",
            disabled: true
        });

        $("#tbFormStatus").dxTextBox({
            value: "New",
            disabled: true
        });

        $("#balance-account").dxNumberBox({
            value: 200000.50,
            showSpinButtons: true,
            showClearButton: true,
        });

        $("#nbTotalClosingBalance").dxNumberBox({
            showClearButton: true,
        });
        

        $inflowFundsGrid = $("#inflowFundsGrid1").dxDataGrid({
            dataSource: inflowFundsGridData,
            columns: [
                {
                    dataField: "type",
                    caption: "Types",
                    lookup: {
                        dataSource: inflowFundsTypes
                    }
                },
                {
                    dataField: "fund",
                    caption: "Funds",
                    lookup: {
                        dataSource: inflowFundsFunds
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
        }).dxDataGrid("instance");;

        $outflowFundsGrid1 = $("#outflowFundsGrid1").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "item",
                    caption: "Items"
                },
                {
                    dataField: "amount",
                    caption: "Amount",
                    dataType: "number"
                }
            ],
            showBorders: true,
            height: 300,
            editing: {
                refreshMode: "reshape",
                mode: "batch",
                allowAdding: true,
                allowUpdating: true,
                allowDeleting: true
            },
            summary: {
                totalItems: [
                    {
                        column: "item",
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
        }).dxDataGrid("instance");;

        $outflowFundsGrid2 = $("#outflowFundsGrid2").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "funds",
                    caption: "Funds"
                },
                {
                    dataField: "amount",
                    caption: "Amount",
                    dataType: "number"
                }
            ],
            showBorders: true,
            height: 300,
            editing: {
                refreshMode: "reshape",
                mode: "batch",
                allowAdding: true,
                allowUpdating: true,
                allowDeleting: true
            },
            summary: {
                totalItems: [
                    {
                        column: "funds",
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

        $("#amsd-form").submit(function (event) {
            if (confirm("You sure to submit?")) {
                alert("Thanks!");
            }
            event.preventDefault();
        });

        $("#backBtn").click(function (event) {
            window.location.replace("./AMSD-Home.html");
            event.preventDefault();
        });
    });
}(window.jQuery, window, document));