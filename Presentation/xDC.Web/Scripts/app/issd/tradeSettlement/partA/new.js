(function ($, window, document) {

    $(function () {
        DevExpress.setTemplateEngine("underscore");

        var $tabpanel, $equityGrid, $openingBalanceGrid, $tradeSettlementForm, $currencySelectBox,
            $approverDropdown, $approvalNotes, $settlementDateBox, $loadPanel, isSaveAsDraft;

        //#region Data Source

        var approverStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: window.location.origin + "/api/common/GetTradeSettlementApprover"
        });

        var currencyStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: window.location.origin + "/api/common/GetTradeSettlementCurrencies"
        });

        function ds(instrumentType, settlementDate, currency) {
            return $.ajax({
                url: window.location.origin + "/api/issd/GetTradeSettlementFromEdw/" + instrumentType + "/" + moment(settlementDate).unix() + "/" + currency,
                type: "get"
            });
        }

        function dsOpeningBalance(settlementDate, currency) {
            return $.ajax({
                url: window.location.origin + "/api/issd/GetOpeningBalanceEdw/" + moment(settlementDate).unix() + "/" + currency,
                type: "get"
            });
        }

        //#endregion
        
        //#region Other Widgets

        $settlementDateBox = $("#settlementDateBox").dxDateBox({
            type: "date",
            displayFormat: "dd/MM/yyyy",
            value: new Date(),
            //min: moment().subtract(1, 'days'),
            onValueChanged: function (data) {
                populateDwData(data.value, $currencySelectBox.option("value"));
            }
        }).dxValidator({
            validationRules: [
                {
                    type: "required",
                    message: "Settlement Date is required"
                }
            ]
        }).dxDateBox("instance");

        $currencySelectBox =
            $("#currencySelectBox").dxSelectBox({
                dataSource: currencyStore,
                displayExpr: "value",
                valueExpr: "value",
                placeHolder: "Currency..",
                onValueChanged: function (data) {
                    populateDwData($settlementDateBox.option("value"), data.value);
                }
            })
            .dxValidator({
                validationRules: [
                    {
                        type: "required",
                        message: "Currency is required"
                    }
                ]
            })
            .dxSelectBox("instance");

        $tabpanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { titleId: "titleBadge1", title: "Equity", template: "equityTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });

        $approverDropdown = $("#approverDropdown").dxSelectBox({
            dataSource: approverStore,
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

        $approvalNotes = $("#approvalNotes").dxTextArea({
            height: 90
        }).dxTextArea("instance");
        
        //#endregion
        
        // #region Data Grid

        $openingBalanceGrid = $("#openingBalanceGrid").dxDataGrid({
            dataSource: [],
            showColumnHeaders: false,
            showColumnLines: false,
            columns: [
                {
                    dataField: "instrumentType",
                    caption: "Opening Balance"
                },
                {
                    dataField: "currency",
                    caption: "Currency",
                    visible: false
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
            ]
        }).dxDataGrid("instance");

        $equityGrid = $("#equityGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "instrumentCode",
                    caption: "Equity",
                    allowEditing: false
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN",
                    allowEditing: false
                },
                {
                    dataField: "maturity",
                    caption: "Maturity (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "sales",
                    caption: "Sales (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "purchase",
                    caption: "Purchase (-)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "remarks",
                    caption: "Remarks",
                    dataType: "text"
                }
            ],
            summary: {
                totalItems: [
                    {
                        column: "instrumentCode",
                        displayFormat: "TOTAL"
                    },
                    {
                        column: "maturity",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "sales",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "purchase",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ]
            },
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: false,
                allowAdding: false
            }
        }).dxDataGrid("instance");
        

        // #endregion Data Grid

        //#region functions

        function togglePanelTitleItemCount(countTagId, gridInstance) {
            var count = gridInstance.getDataSource().items().length;

            if (count > 0) {
                $("#" + countTagId).addClass("label label-danger").css("margin-left", "4px").text(count);
            } else {
                $("#" + countTagId).removeClass("label label-danger").css("margin-left", "0").text("");
            }
        }

        function togglePanelTitleItemCountOnFetchEdw() {
            togglePanelTitleItemCount("titleBadge1", $equityGrid);
        }

        function populateDwData(settlementDate, currency) {
            if (settlementDate && currency) {
                $.when(
                        ds("EQUITY", settlementDate, currency),
                        dsOpeningBalance(settlementDate, currency)
                    )
                    .done(function (data1, data2) {
                        $equityGrid.option("dataSource", data1[0].data);
                        $equityGrid.repaint();

                        $openingBalanceGrid.option("dataSource", data2[0].data);
                        $openingBalanceGrid.repaint();

                        setTimeout(togglePanelTitleItemCountOnFetchEdw, 500);
                    })
                    .always(function (dataOrjqXHR, textStatus, jqXHRorErrorThrown) {
                        toast("Data Updated", "info");
                    })
                    .then(function () {

                    });
            } else {
                dxGridUtils.clearGrid($equityGrid);
            }
        }

        function saveAllGridEditData() {
            $equityGrid.saveEditData();
        }

        function postData(data) {
            return $.ajax({
                data: data,
                dataType: "json",
                url: window.location.origin + "/api/issd/TradeSettlement/New",
                method: "post",
                success: function (data) {
                    window.location.href = "/issd/TradeSettlement/PartA/View/" + data;
                },
                fail: function (jqXHR, textStatus, errorThrown) {
                    $("#error_container").bs_alert(textStatus + ": " + errorThrown);
                },
                complete: function (data) {
                    $loadPanel.hide();
                }
            });
        }

        //#endregion 

        //#region Events

        $("#saveAsDraftBtn").on({
            "click": function (e) {
                isSaveAsDraft = true;
            }
        });

        $("#submitForApprovalBtn").on({
            "click": function (e) {
                isSaveAsDraft = false;
            }
        });

        $tradeSettlementForm = $("#tradeSettlementForm").on("submit",
            function (e) {
                saveAllGridEditData();

                if (moment().subtract(1, "days").isAfter($settlementDateBox.option("value"))) {
                    alert("T-n only available for viewing..");
                }
                else {
                    if (isSaveAsDraft) {
                        // new clean draft

                        var data = {
                            currency: $currencySelectBox.option("value"),
                            settlementDateEpoch: moment($settlementDateBox.option("value")).unix(),
                            formType: 3,
                            isSaveAsDraft: true,

                            openingBalance: $openingBalanceGrid.getDataSource().items(),
                            equity: $equityGrid.getDataSource().items()
                        };

                        postData(data);
                    }
                    else {
                        $('#selectApproverModal').modal('show');
                    }
                }

                e.preventDefault();
            });

        $("#submitForApprovalModalBtn").on({
            "click": function (e) {
                saveAllGridEditData();

                var data = {
                    currency: $currencySelectBox.option("value"),
                    settlementDateEpoch: moment($settlementDateBox.option("value")).unix(),
                    formType: 3,

                    openingBalance: $openingBalanceGrid.getDataSource().items(),
                    equity: $equityGrid.getDataSource().items(),
                    
                    approver: $approverDropdown.option("value")
                };

                postData(data);

                e.preventDefault();
            }
        });
        
        //#endregion
    });
}(window.jQuery, window, document));