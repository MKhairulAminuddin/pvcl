(function ($, window, document) {

    $(function () {
        //#region Variable Definition

        var $inflowTabpanel,
            $inflowDepositGrid,
            $inflowMmiGrid,

            $outflowTabpanel,
            $outflowDepositGrid,
            $outflowMmiGrid,

            $treasuryForm,
            $selectApproverModal = $("#selectApproverModal"),
            $submitForApprovalModalBtn,
            
            $currencySelectBox,
            $tradeDate,
            $approverDropdown,
            $approvalNotes,

            isSaveAsDraft = false;

        var referenceUrl = {
            dsMaturity: window.location.origin + "/api/fid/Treasury/EdwMaturity/",
            dsBankCounterParty: window.location.origin + "/api/fid/Treasury/EdwBankCounterParty/",
            dsIssuer: window.location.origin + "/api/fid/Treasury/EdwIssuer/",

            dsApproverList: window.location.origin + "/api/common/approverList/treasury",

            postNewFormRequest: window.location.origin + "/api/fid/Treasury/New",
            postNewFormResponse: window.location.origin + "/fid/Treasury/View/"
        };
        
        //#endregion

        //#region Data Source & Functions
        
        var dsBankCounterParty = DevExpress.data.AspNet.createStore({
            key: "reference",
            loadUrl: referenceUrl.dsBankCounterParty
        });

        var dsIssuer = DevExpress.data.AspNet.createStore({
            key: "reference",
            loadUrl: referenceUrl.dsBankCounterParty
        });

        var dsMaturity = function (tradeDateEpoch, currency) {
            return $.ajax({
                url: referenceUrl.dsMaturity + "/" + moment(tradeDateEpoch).unix() + "/" + currency,
                type: "get"
            });
        };

        var populateDwData = function(tradeDate, currency) {
            if (tradeDate && currency) {
                $.when(
                    dsMaturity(tradeDate, currency)
                    )
                    .done(function (data1) {
                        $inflowDepositGrid.option("dataSource", data1.data);
                        $inflowDepositGrid.repaint();
                        
                    })
                    .always(function (dataOrjqXHR, textStatus, jqXHRorErrorThrown) {
                        //tradeSettlement.toast("Data Updated", "info");
                        app.toast("Data Updated", "info");
                    })
                    .then(function () {

                    });
            } else {
                dxGridUtils.clearGrid($inflowDepositGrid);
            }
        }

        function postData(isDraft) {
            var data = {
                currency: $currencySelectBox.option("value"),
                tradeDate: moment($tradeDate.option("value")).unix(),
                
                inflowDeposit: $inflowDepositGrid.getDataSource().items(),
                outflowDeposit: $outflowDepositGrid.getDataSource().items(),

                inflowMoneyMarket: $inflowMmiGrid.getDataSource().items(),
                outflowMoneyMarket: $outflowMmiGrid.getDataSource().items(),

                approver: (isDraft) ? null : $approverDropdown.option("value"),
                approvalNotes: (isDraft) ? null : $approvalNotes.option("value")
            };

            return $.ajax({
                data: JSON.stringify(data),
                dataType: "json",
                url: referenceUrl.postNewFormRequest,
                method: "post",
                success: function (response) {
                    window.location.href = referenceUrl.postNewFormResponse + response;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    app.alertError(errorThrown + ": " + jqXHR.responseJSON);
                },
                complete: function (data) {
                    
                }
            });
        }

        var availableMaturityDates;
        
        function calendarMarkerTemplate(data) {
            var cssClass = "";
            
            $.each(availableMaturityDates, function (_, item) {
                if (data.date.getDate() === item.day && data.date.getMonth() === item.month-1) {
                    cssClass = "markDate";
                    return false;
                }
            });
            
            return "<span class='" + cssClass + "'>" + data.text + "</span>";
        }

        var loadCalendarMarkers = function() {
            $.ajax({
                contentType: "application/json",
                url: window.location.origin + "/api/fid/Treasury/EdwMaturity/AvailableMaturity",
                method: "get",
                success: function (response) {
                    availableMaturityDates = response;
                    $tradeDate.option("calendarOptions",
                        {
                            cellTemplate: calendarMarkerTemplate
                        });
                },
                error: function(jqXHR, textStatus, errorThrown) {
                    $("#error_container").bs_alert(errorThrown + ": " + jqXHR.responseJSON);
                },
                complete: function(data) {

                }
            });
        }

        //#endregion
        
        //#region Other Widgets

        $tradeDate = $("#tradeDate").dxDateBox({
            type: "date",
            displayFormat: "dd/MM/yyyy",
            value: new Date(),
            calendarOptions: {
                cellTemplate: calendarMarkerTemplate
            },
            onValueChanged: function (data) {

                $.ajax({
                    contentType: "application/json",
                    url: window.location.origin +
                        "/api/fid/Treasury/EdwMaturity/AvailableMaturity/" +
                        moment(data.value).unix(),
                    method: "get",
                    success: function(response) {
                        $currencySelectBox.option("dataSource", response);
                    },
                    error: function(jqXHR, textStatus, errorThrown) {
                        $("#error_container").bs_alert(errorThrown + ": " + jqXHR.responseJSON);
                    },
                    complete: function(data) {

                    }
                });
            }
        }).dxDateBox("instance");

        $currencySelectBox = $("#currency").dxSelectBox({
            dataSource: ["MYR", "USD"],
            placeHolder: "Currency..",
        }).dxSelectBox("instance");
        
        $inflowTabpanel = $("#inflowTabpanel").dxTabPanel({
            dataSource: [
                { titleId: "tab1", title: "Deposit", template: "inflowDepositTab" },
                { titleId: "tab2", title: "Money Market Instruments", template: "inflowMmiTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });

        $outflowTabpanel = $("#outflowTabpanel").dxTabPanel({
            dataSource: [
                { titleId: "tab1", title: "Deposit", template: "outflowDepositTab" },
                { titleId: "tab2", title: "Money Market Instruments", template: "outflowMmiTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });

        $approverDropdown = $("#approverDropdown").dxSelectBox({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.dsApproverList
            }),
            displayExpr: "displayName",
            valueExpr: "username",
            searchEnabled: true,
            itemTemplate: function(data) {
                return "<div class='active-directory-dropdown'>" +
                    "<p class='active-directory-title'>" +
                    data.displayName +
                    "</p>" +
                    "<p class='active-directory-subtitle'>" +
                    data.title +
                    ", " +
                    data.department +
                    "</p>" +
                    "<p class='active-directory-subtitle'>" +
                    data.email +
                    "</p>" +
                    "</div>";
            }
        }).dxSelectBox("instance");

        $approvalNotes = $("#approvalNotes").dxTextArea({
            height: 90
        }).dxTextArea("instance");
        
        //#endregion
        
        // #region Data Grid

        $inflowDepositGrid = $("#inflowDepositGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "dealer",
                    caption: "Dealer",
                    allowEditing: false,
                    calculateCellValue: function (rowData) {
                        rowData.dealer = window.currentUser;
                        return window.currentUser;
                    }
                },
                {
                    dataField: "bank",
                    caption: "Bank",
                    lookup: {
                        dataSource: dsBankCounterParty,
                        valueExpr: "name",
                        displayExpr: "name"
                    },
                    allowEditing: false
                },
                {
                    dataField: "valueDate",
                    caption: "Value Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    editorOptions: {
                        placeholder: "dd/MM/yyyy",
                        showClearButton: true
                    },
                    allowEditing: false
                },
                {
                    dataField: "maturityDate",
                    caption: "Maturity Date (T)",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    editorOptions: {
                        placeholder: "dd/MM/yyyy",
                        showClearButton: true
                    },
                    allowEditing: false
                },
                {
                    dataField: "principal",
                    caption: "Principal",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "tenor",
                    caption: "Tenor (day)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 0
                    },
                    allowEditing: false
                },
                {
                    dataField: "ratePercent",
                    caption: "Rate (%)",
                    dataType: "number",
                    format: "#.00 '%'",
                    allowEditing: false
                },
                {
                    dataField: "intProfitReceivable",
                    caption: "Interest/Profit Receivable",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "principalIntProfitReceivable",
                    caption: "Principal + Interest/Profit Receivable",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    allowEditing: false
                },
                {
                    dataField: "assetType",
                    caption: "Asset Type",
                    lookup: {
                        dataSource: treasury.dsAssetType
                    },
                    allowEditing: false,
                },
                {
                    dataField: "repoTag",
                    caption: "REPO tag",
                },
                {
                    dataField: "contactPerson",
                    caption: "Contact Person",
                },
                {
                    dataField: "notes",
                    caption: "Notes",
                    lookup: {
                        dataSource: treasury.dsNotes
                    }
                },
            ],
            summary: {
                totalItems: [
                    {
                        column: "principal",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 0
                        }
                    },
                    {
                        column: "principalIntProfitReceivable",
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
            },
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            wordWrapEnabled: true
        }).dxDataGrid("instance");

        $outflowDepositGrid = $("#outflowDepositGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "dealer",
                    caption: "Dealer",
                    calculateCellValue: function (rowData) {
                        rowData.dealer = window.currentUser;
                        return window.currentUser;
                    },
                    allowEditing: false
                },
                {
                    dataField: "bank",
                    caption: "Bank",
                    lookup: {
                        dataSource: dsBankCounterParty,
                        valueExpr: "name",
                        displayExpr: "name"
                    }
                },
                {
                    dataField: "valueDate",
                    caption: "Value Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    editorOptions: {
                        placeholder: "dd/MM/yyyy",
                        showClearButton: true
                    }
                },
                {
                    dataField: "maturityDate",
                    caption: "Maturity Date (T)",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    editorOptions: {
                        placeholder: "dd/MM/yyyy",
                        showClearButton: true
                    }
                },
                {
                    dataField: "principal",
                    caption: "Principal",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "tenor",
                    caption: "Tenor (day)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 0
                    },
                    calculateCellValue: function (rowData) {
                        rowData.tenor = treasury.tenor(rowData.maturityDate, rowData.valueDate);
                        return rowData.tenor;
                    },
                    allowEditing: false
                },
                {
                    dataField: "ratePercent",
                    caption: "Rate (%)",
                    dataType: "number",
                    format: "#.00 '%'"
                },
                {
                    dataField: "intProfitReceivable",
                    caption: "Interest/Profit Receivable",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    calculateCellValue: function (rowData) {
                        var rate = (parseFloat(rowData.ratePercent * 100) || 0);
                        var principal = (parseFloat(rowData.principal) || 0);

                        var tenor = 0;
                        if ($currencySelectBox.option("value") == "USD" ||
                            $currencySelectBox.option("value") == "AUD" ||
                            $currencySelectBox.option("value") == "EUR") {
                            tenor = (parseFloat(treasury.tenor(rowData.maturityDate, rowData.valueDate) / 360 * 100) || 0);
                        } else {
                            tenor = (parseFloat(treasury.tenor(rowData.maturityDate, rowData.valueDate) / 365 * 100) || 0);
                        }

                        rowData.intProfitReceivable = (principal * tenor * rate);

                        return rowData.intProfitReceivable;
                    },
                    allowEditing: false
                },
                {
                    dataField: "principalIntProfitReceivable",
                    caption: "Principal + Interest/Profit Receivable",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    calculateCellValue: function (rowData) {
                        var rate = (parseFloat(rowData.ratePercent * 100) || 0);
                        var principal = (parseFloat(rowData.principal) || 0);

                        var tenor = 0;
                        if ($currencySelectBox.option("value") == "USD" ||
                            $currencySelectBox.option("value") == "AUD" ||
                            $currencySelectBox.option("value") == "EUR") {
                            tenor = (parseFloat(treasury.tenor(rowData.maturityDate, rowData.valueDate) / 360 * 100) || 0);
                        } else {
                            tenor = (parseFloat(treasury.tenor(rowData.maturityDate, rowData.valueDate) / 365 * 100) || 0);
                        }

                        var intProfitReceivable = (principal * tenor * rate);

                        rowData.principalIntProfitReceivable = principal + intProfitReceivable;

                        return rowData.principalIntProfitReceivable;
                    },
                    allowEditing: false
                },
                {
                    dataField: "assetType",
                    caption: "Asset Type",
                    lookup: {
                        dataSource: treasury.dsAssetType
                    }
                },
                {
                    dataField: "repoTag",
                    caption: "REPO tag",
                },
                {
                    dataField: "contactPerson",
                    caption: "Contact Person",
                },
                {
                    dataField: "notes",
                    caption: "Notes",
                    lookup: {
                        dataSource: treasury.dsNotes
                    }
                }
            ],
            summary: {
                totalItems: [
                    {
                        column: "principal",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 0
                        }
                    },
                    {
                        column: "principalIntProfitReceivable",
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
                allowAdding: true
            },
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            wordWrapEnabled: true
        }).dxDataGrid("instance");

        $inflowMmiGrid = $("#inflowMmiGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "dealer",
                    caption: "Dealer",
                    allowEditing: false,
                    calculateCellValue: function (rowData) {
                        rowData.dealer = window.currentUser;
                        return window.currentUser;
                    }
                },
                {
                    dataField: "issuer",
                    caption: "Issuer",
                    lookup: {
                        dataSource: dsIssuer,
                        valueExpr: "name",
                        displayExpr: "name"
                    }
                },
                {
                    dataField: "productType",
                    caption: "Product Type",
                    lookup: {
                        dataSource: treasury.dsProductType
                    }
                },
                {
                    dataField: "counterParty",
                    caption: "Counterparty",
                    lookup: {
                        dataSource: dsBankCounterParty,
                        valueExpr: "name",
                        displayExpr: "name"
                    }
                },
                {
                    dataField: "valueDate",
                    caption: "Value Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    editorOptions: {
                        placeholder: "dd/MM/yyyy",
                        showClearButton: true
                    }
                },
                {
                    dataField: "maturityDate",
                    caption: "Maturity Date (T)",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    editorOptions: {
                        placeholder: "dd/MM/yyyy",
                        showClearButton: true
                    }
                },
                {
                    dataField: "holdingDayTenor",
                    caption: "Holding Period / Tenor (days)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 0
                    },
                    calculateCellValue: function (rowData) {
                        rowData.holdingDayTenor = treasury.tenor(rowData.maturityDate, rowData.valueDate);
                        return rowData.holdingDayTenor;
                    },
                    allowEditing: false
                },
                {
                    dataField: "nominal",
                    caption: "Nominal",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "sellPurchaseRateYield",
                    caption: "Sell Rate / Yield (%)",
                    dataType: "number",
                    format: "#.00 '%'"
                },
                {
                    dataField: "price",
                    caption: "Price",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    calculateCellValue: function (rowData) {
                        rowData.price = treasury.inflow_price(
                            rowData.productType,
                            rowData.nominal,
                            rowData.sellPurchaseRateYield,
                            treasury.tenor(rowData.maturityDate, rowData.valueDate)
                        );
                        return rowData.price;
                    },
                    allowEditing: false
                },
                {
                    dataField: "purchaseProceeds",
                    caption: "Purchase Proceeds",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "intDividendReceivable",
                    caption: "Interest/Dividend Receivable",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    calculateCellValue: function (rowData) {
                        rowData.intDividendReceivable = treasury.inflow_intDiv(
                            rowData.productType,
                            rowData.nominal,
                            rowData.sellPurchaseRateYield,
                            treasury.tenor(rowData.maturityDate, rowData.valueDate),
                            rowData.purchaseProceeds
                        );
                        return rowData.intDividendReceivable;
                    },
                    allowEditing: false
                },
                {
                    dataField: "proceeds",
                    caption: "Proceeds",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    calculateCellValue: function (rowData) {
                        rowData.proceeds = treasury.inflow_proceeds(
                            rowData.productType,
                            rowData.nominal,
                            rowData.sellPurchaseRateYield,
                            treasury.tenor(rowData.maturityDate, rowData.valueDate)
                        );
                        return rowData.proceeds;
                    },
                    allowEditing: false
                },
                {
                    dataField: "certNoStockCode",
                    caption: "Certificate No. / Stock Code"
                }
            ],
            summary: {
                totalItems: [
                    {
                        column: "nominal",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 0
                        }
                    },
                    {
                        column: "intDividendReceivable",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "proceeds",
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
                allowAdding: true
            },
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            wordWrapEnabled: true
        }).dxDataGrid("instance");

        $outflowMmiGrid = $("#outflowMmiGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "dealer",
                    caption: "Dealer",
                    allowEditing: false,
                    calculateCellValue: function (rowData) {
                        rowData.dealer = window.currentUser;
                        return window.currentUser;
                    }
                },
                {
                    dataField: "issuer",
                    caption: "Issuer",
                    lookup: {
                        dataSource: dsIssuer,
                        valueExpr: "name",
                        displayExpr: "name"
                    }
                },
                {
                    dataField: "productType",
                    caption: "Product Type",
                    lookup: {
                        dataSource: treasury.dsProductType
                    }
                },
                {
                    dataField: "counterParty",
                    caption: "Counterparty",
                    lookup: {
                        dataSource: dsBankCounterParty,
                        valueExpr: "name",
                        displayExpr: "name"
                    }
                },
                {
                    dataField: "valueDate",
                    caption: "Value Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    editorOptions: {
                        placeholder: "dd/MM/yyyy",
                        showClearButton: true
                    }
                },
                {
                    dataField: "maturityDate",
                    caption: "Maturity Date (T)",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    editorOptions: {
                        placeholder: "dd/MM/yyyy",
                        showClearButton: true
                    }
                },
                {
                    dataField: "holdingDayTenor",
                    caption: "Holding Period / Tenor (days)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 0
                    },
                    calculateCellValue: function (rowData) {
                        rowData.holdingDayTenor = treasury.tenor(rowData.maturityDate, rowData.valueDate);
                        return rowData.holdingDayTenor;
                    },
                    allowEditing: false
                },
                {
                    dataField: "nominal",
                    caption: "Nominal",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "sellPurchaseRateYield",
                    caption: "Purchase Rate / Yield (%)",
                    dataType: "number",
                    format: "#.00 '%'"
                },
                {
                    dataField: "price",
                    caption: "Price",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    calculateCellValue: function (rowData) {
                        rowData.price = treasury.outflow_price(
                            rowData.productType,
                            rowData.nominal,
                            rowData.sellPurchaseRateYield,
                            treasury.tenor(rowData.maturityDate, rowData.valueDate)
                        );
                        return rowData.price;
                    },
                    allowEditing: false
                },
                {
                    dataField: "intDividendReceivable",
                    caption: "Interest/Dividend Receivable",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    calculateCellValue: function (rowData) {
                        rowData.intDividendReceivable = treasury.outflow_intDiv(
                            rowData.productType,
                            rowData.nominal,
                            rowData.sellPurchaseRateYield,
                            treasury.tenor(rowData.maturityDate, rowData.valueDate)
                        );
                        return rowData.intDividendReceivable;
                    },
                    allowEditing: false
                },
                {
                    dataField: "proceeds",
                    caption: "Proceeds",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    calculateCellValue: function (rowData) {
                        rowData.proceeds = treasury.outflow_proceeds(
                            rowData.productType,
                            rowData.nominal,
                            rowData.sellPurchaseRateYield,
                            treasury.tenor(rowData.maturityDate, rowData.valueDate)
                        );
                        return rowData.proceeds;
                    },
                    allowEditing: false
                },
                {
                    dataField: "certNoStockCode",
                    caption: "Certificate No. / Stock Code"
                }
            ],
            summary: {
                totalItems: [
                    {
                        column: "nominal",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 0
                        }
                    },
                    {
                        column: "intDividendReceivable",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "proceeds",
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
                allowAdding: true
            },
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            wordWrapEnabled: true
        }).dxDataGrid("instance");
        
        // #endregion Data Grid
 
        //#region Events & Invocations

        $tradeDate.on("valueChanged", function (data) {
            populateDwData(data.value, $currencySelectBox.option("value"));
        });

        $currencySelectBox.on("valueChanged", function (data) {
            populateDwData($tradeDate.option("value"), data.value);
        });

        $("#saveAsDraftBtn").dxButton({
            onClick: function (e) {
                app.saveAllGrids($inflowDepositGrid, $outflowDepositGrid, $inflowMmiGrid, $outflowMmiGrid);

                setTimeout(function () {
                    postData(true);
                }, 1000);
            }
        });

        $treasuryForm = $("#treasuryForm").on("submit",
            function (e) {
                e.preventDefault();

                app.saveAllGrids($inflowDepositGrid, $outflowDepositGrid, $inflowMmiGrid, $outflowMmiGrid);

                /*if (moment().subtract(1, "days").isAfter($tradeDate.option("value"))) {
                    alert("T-n only available for viewing..");
                }
                else {
                    $selectApproverModal.modal('show');
                }*/

                $selectApproverModal.modal("show");
                
            });

        $submitForApprovalModalBtn = $("#submitForApprovalModalBtn").on({
            "click": function (e) {
                app.saveAllGrids($inflowDepositGrid, $outflowDepositGrid, $inflowMmiGrid, $outflowMmiGrid);

                setTimeout(function () {
                    postData(false);
                }, 1000);
                e.preventDefault();
            }
        });
        
        loadCalendarMarkers();

        //#endregion
    });
}(window.jQuery, window, document));