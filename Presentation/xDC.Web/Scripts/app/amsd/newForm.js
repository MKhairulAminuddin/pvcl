(function ($, window, document) {

    $(function () {
        var inflowFundsTypes = ["Funds Received", "Maturity", "Proceed", "Interest Received"];
        var inflowFundsBank = ["RHB Bank -01", "RHB Bank -02", "RHB Bank -03", "BNM -01", "BA", "CP Matured", " Cagamas Int", "Loan SPnb", "Bon Int"];
        
        var $inflowFundsGrid, $historyBtn, $cancelBtn, $tbFormId, $tbFormStatus;


        $tbFormId = $("#tbFormId").dxTextBox({
            disabled: true
        }).dxTextBox("instance");

        $tbFormStatus = $("#tbFormStatus").dxTextBox({
            value: "New",
            disabled: true
        }).dxTextBox("instance");;

        $historyBtn = $("#historyBtn").dxButton({
            type: "default",
            icon: "detailslayout"
        }).dxButton("instance");

        $inflowFundsGrid = $("#inflowFundsGrid1").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "fundTypes",
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
        }).dxDataGrid("instance");;
        
        $("#amsd-form").submit(function (event) {
            $inflowFundsGrid.getDataSource().store().load().done(function(items) {
                $("input[name='inflowFundsInput']").val(JSON.stringify(items));
            });

            console.log($inflowFundsGrid.getDataSource());

            if (confirm("You sure to submit?")) {
                alert("Thanks!");
            }
            event.preventDefault();
        });

        $cancelBtn = $("#cancelBtn").click(function (event) {
            window.location.replace("../amsd");
            event.preventDefault();
        });
    });
}(window.jQuery, window, document));