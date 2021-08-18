(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        
        var $tabpanel,
            $grid,
            $dateSelectionBtn,
            $printBtn;

        var referenceUrl = {
            adminEdit: window.location.origin + "/issd/TradeSettlement/PartA/Edit/",

            submitApprovalRequest: window.location.origin + "/api/issd/TradeSettlement/Approval",
            submitApprovalResponse: window.location.origin + "/issd/TradeSettlement/PartA/View/"
        };

        //#endregion

        //#region Data Source & Functions
        

        //#endregion

        //#region Other Widgets

        $dateSelectionBtn = $("#dateSelectionBtn").dxDateBox({
            type: "date",
            displayFormat: "dd/MM/yyyy",
            value: new Date(),
            
        }).dxDateBox("instance");
        

        //#endregion

        // #region DataGrid
        
        $grid = $("#grid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "currency",
                    caption: "Currency",
                    groupIndex: 0
                },
                {
                    dataField: "account",
                    caption: "Account"
                },
                {
                    dataField: "openingBalance",
                    caption: "Opening Balance",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "totalInflow",
                    headerCellTemplate: function (container) {
                        container.append($("<div><strong>Total Inflow</strong><br/>(including Deposit Maturity Only)</div>"));
                    },
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "totalOutflow",
                    headerCellTemplate: function (container) {
                        container.append($("<div><strong>Total Outflow</strong><br/>(excluding MM investment value = T)</div>"));
                    },
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "net",
                    headerCellTemplate: function (container) {
                        container.append($("<div><strong>Net</strong><br/>(including Deposit maturity)<br/><small>* available funds for MM investment</small></div>"));
                    },
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
            ],
            summary: {
                groupItems: [
                    {
                        column: "openingBalance",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "totalInflow",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "totalOutflow",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "net",
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
        
        // #endregion DataGrid

        //#region Events

        $("#viewWorkflowBtn").on({
            "click": function (e) {
                $('#viewWorkflowModal').modal('show');

                e.preventDefault();
            }
        });

        $("#adminEditBtn").on({
            "click": function (e) {
                window.location.href = referenceUrl.adminEdit + tradeSettlement.getIdFromQueryString;
                e.preventDefault();
            }
        });

        $("#approveBtn").on({
            "click": function (e) {
                $("#approvalNoteModal").modal("show");
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
        

        //#endregion
    });
}(window.jQuery, window, document));