(function ($, window, document) {

    $(function () {
        //#region Variable Definition

        DevExpress.config({
            forceIsoDateParsing: true
        });

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
            loadCurrencies: window.location.origin + "/api/common/GetTradeSettlementCurrencies",
            dsMaturity: window.location.origin + "/api/fid/Treasury/EdwMaturity/",

            dsApproverList: window.location.origin + "/api/common/approverList/treasury",

            postNewFormRequest: window.location.origin + "/api/fid/Treasury/New",
            postNewFormResponse: window.location.origin + "/fid/Treasury/View/"
        };

        var sampleDataInflowMmi = [
            {
                productType: "NID",
                valueDate: new Date(2020, 11, 17),
                maturityDate: new Date(2021, 0, 20),
                nominal: 10000000.00,
                sellPurchaseRateYield: 2.05,
                purchaseProceeds: 0,
                certNoStockCode: "AMB111210"
            },
            {
                productType: "NIDC",
                valueDate: new Date(2021, 7, 15),
                maturityDate: new Date(2021, 8, 30),
                nominal: 10000000.00,
                sellPurchaseRateYield: 2.050,
                purchaseProceeds: 5000000.00,
                certNoStockCode: "AMB111210"
            },
            {
                productType: "CP",
                valueDate: new Date(2021, 7, 15),
                maturityDate: new Date(2021, 7, 30),
                nominal: 5000000.00,
                sellPurchaseRateYield: 2.000,
                purchaseProceeds: 3000000.00,
                certNoStockCode: "RA86899"
            },
            {
                productType: "ICP",
                valueDate: new Date(2021, 8, 3),
                maturityDate: new Date(2021, 8, 30),
                nominal: 3000000.00,
                sellPurchaseRateYield: 2.150,
                purchaseProceeds: 2000000.00,
                certNoStockCode: "CD88934"
            },
            {
                productType: "BNMN",
                valueDate: new Date(2021, 8, 1),
                maturityDate: new Date(2021, 9, 1),
                nominal: 10000000.00,
                sellPurchaseRateYield: 1.900,
                purchaseProceeds: 5000000.00,
                certNoStockCode: "364578"
            },
            {
                issuer: "BNM",
                productType: "MTB",
                valueDate: new Date(2021, 8, 30),
                maturityDate: new Date(2021, 9, 30),
                nominal: 10000000.00,
                sellPurchaseRateYield: 2.100,
                purchaseProceeds: 5000000.00,
                certNoStockCode: "543989"
            },
            {
                productType: "NIDL",
                valueDate: new Date(2020, 11, 17),
                maturityDate: new Date(2021, 0, 22),
                nominal: 10000000.00,
                sellPurchaseRateYield: 2.050,
                purchaseProceeds: 5000000.00,
                certNoStockCode: "AMB111210"
            }
        ];

        var sampleDataOutflowMmi = [
            {
                productType: "NID",
                valueDate: new Date(2020, 11, 17),
                maturityDate: new Date(2021, 0, 20),
                nominal: 10000000.00,
                sellPurchaseRateYield: 2.03,
                certNoStockCode: "AMB111210"
            },
            {
                productType: "NIDC",
                valueDate: new Date(2021, 6, 29),
                maturityDate: new Date(2021, 7, 30),
                nominal: 10000000.00,
                sellPurchaseRateYield: 2.060,
                certNoStockCode: "10111"
            },
            {
                productType: "NIDC",
                valueDate: new Date(2021, 6, 29),
                maturityDate: new Date(2021, 7, 30),
                nominal: 10000000.00,
                sellPurchaseRateYield: 2.060,
                certNoStockCode: "10112"
            },
            {
                productType: "CP",
                valueDate: new Date(2021, 6, 29),
                maturityDate: new Date(2021, 7, 30),
                nominal: 5000000.00,
                sellPurchaseRateYield: 2.000,
                certNoStockCode: "RA86899"
            },
            {
                productType: "ICP",
                valueDate: new Date(2021, 6, 29),
                maturityDate: new Date(2021, 8, 30),
                nominal: 3000000.00,
                sellPurchaseRateYield: 2.150,
                certNoStockCode: "CD88934"
            },
            {
                issuer: "BA",
                productType: "MTB",
                valueDate: new Date(2021, 6, 29),
                maturityDate: new Date(2021, 9, 1),
                nominal: 500000.00,
                sellPurchaseRateYield: 2.200,
                certNoStockCode: "BA7865"
            },
            {
                productType: "AB-i",
                valueDate: new Date(2021, 6, 29),
                maturityDate: new Date(2021, 9, 30),
                nominal: 1000000.00,
                sellPurchaseRateYield: 2.300,
                certNoStockCode: "BA89644"
            },
            {
                issuer: "BNMN",
                productType: "MTB",
                valueDate: new Date(2021, 6, 29),
                maturityDate: new Date(2021, 9, 1),
                nominal: 10000000.00,
                sellPurchaseRateYield: 2.000,
                certNoStockCode: "364578"
            },
            {
                productType: "MTB",
                valueDate: new Date(2021, 6, 29),
                maturityDate: new Date(2021, 9, 30),
                nominal: 10000000.00,
                sellPurchaseRateYield: 2.150,
                certNoStockCode: "543989"
            },
            {
                productType: "NIDL",
                valueDate: new Date(2020, 10, 17),
                maturityDate: new Date(2022, 0, 22),
                nominal: 10000000.00,
                sellPurchaseRateYield: 2.03,
                certNoStockCode: "AMB111210"
            }
        ];

        var sampleDataOutflowDeposit = [
            {
                maturityDate: new Date(2021, 7, 27),
                valueDate: new Date(2021, 7, 20),
                principal: 33026248.32,
                ratePercent: 0.01,
                assetType: "MMD",
                notes: "New",
                contactPerson: "Zaki"
            },
            {
                maturityDate: new Date(2021, 8, 3),
                valueDate: new Date(2021, 7, 27),
                principal: 23149090.84,
                ratePercent: 0.02,
                assetType: "MMD",
                notes: "New",
                contactPerson: "Zaki"
            }
        ];

        //#endregion

        //#region Data Source & Functions
        var parseDepositArray = function (dataGridData) {
            if (dataGridData.length > 0) {
                var x = dataGridData.map(function (x) {
                    for (key in x) {
                        return {
                            dealer: x.dealer,
                            bank: x.bank,
                            valueDate: x.valueDate.toISOString(),
                            maturityDate: x.maturityDate.toISOString(),
                            principal: x.principal,
                            tenor: x.tenor,
                            ratePercent: x.ratePercent,
                            intProfitReceivable: x.intProfitReceivable,
                            principalIntProfitReceivable: x.principalIntProfitReceivable,
                            assetType: x.assetType,
                            repoTag: x.repoTag,
                            contactPerson: x.contactPerson,
                            notes: x.notes
                        };
                    }
                });

                return x;
            } else {
                return [];
            }
        }

        var parseMmiArray = function (dataGridData) {
            if (dataGridData.length > 0) {
                var x = dataGridData.map(function (x) {
                    for (key in x) {
                        return {
                            certNoStockCode: x.certNoStockCode,
                            counterParty: x.counterParty,
                            dealer: x.dealer,
                            holdingDayTenor: x.holdingDayTenor,
                            intDividendReceivable: x.intDividendReceivable,
                            issuer: x.issuer,
                            maturityDate: x.maturityDate.toISOString(),
                            nominal: x.nominal,
                            price: x.price,
                            proceeds: x.proceeds,
                            productType: x.productType,
                            purchaseProceeds: x.purchaseProceeds,
                            sellPurchaseRateYield: x.sellPurchaseRateYield,
                            valueDate: x.valueDate.toISOString()
                        };
                    }
                });

                return x;
            } else {
                return [];
            }
        }
        
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
                        app.toastEdwCount(data1.data, "inflow deposits");

                    })
                    .always(function (dataOrjqXHR, textStatus, jqXHRorErrorThrown) {
                        
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
                outflowDeposit: parseDepositArray($outflowDepositGrid.getDataSource().items()),

                inflowMoneyMarket: parseMmiArray($inflowMmiGrid.getDataSource().items()),
                outflowMoneyMarket: parseMmiArray($outflowMmiGrid.getDataSource().items()),

                approver: (isDraft) ? null : $approverDropdown.option("value"),
                approvalNotes: (isDraft) ? null : $approvalNotes.option("value")
            };

            return $.ajax({
                data: data,
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
                    app.alertError(errorThrown + ": " + jqXHR.responseJSON);
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
                        
                    },
                    error: function(jqXHR, textStatus, errorThrown) {
                        app.alertError(errorThrown + ": " + jqXHR.responseJSON);
                    },
                    complete: function(data) {

                    }
                });
            }
        }).dxDateBox("instance");

        $currencySelectBox = $("#currency").dxSelectBox({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.loadCurrencies
            }),
            displayExpr: "value",
            valueExpr: "value",
            placeHolder: "Currency..",
            onValueChanged:function(data) {
                $outflowDepositGrid.refresh();
            }
        })
            .dxValidator({ validationRules: [{ type: "required", message: "Currency is required" }] })
            .dxSelectBox("instance");
        
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
                        dataSource: treasury.dsBankCounterParty(),
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
                    format: "#.000 '%'"
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
                        var currency = $currencySelectBox.option("value");
                        var principal = rowData.principal;
                        var tenor = treasury.tenor(rowData.maturityDate, rowData.valueDate);
                        var rate = rowData.ratePercent;

                        rowData.intProfitReceivable = treasury.outflow_depoInt(currency, principal, tenor, rate);

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
                        var currency = $currencySelectBox.option("value");
                        var principal = rowData.principal;
                        var tenor = treasury.tenor(rowData.maturityDate, rowData.valueDate);
                        var rate = rowData.ratePercent;

                        rowData.principalIntProfitReceivable = treasury.outflow_depo_PrincipalInt(currency, principal, tenor, rate);
                        return rowData.principalIntProfitReceivable;
                    },
                    allowEditing: false
                },
                {
                    dataField: "assetType",
                    caption: "Asset Type",
                    lookup: {
                        dataSource: treasury.dsAssetType(),
                        valueExpr: "value",
                        displayExpr: "value"
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
                        dataSource: treasury.dsNotes(),
                        valueExpr: "value",
                        displayExpr: "value"
                    }
                },
                {
                    type: "buttons",
                    width: 110,
                    buttons: [
                        "edit",
                        "delete",
                        {
                            text: "Copy",
                            onClick: function (e) {
                                e.component.byKey(e.row.key).done((source) => {
                                    var clone = Object.assign({}, source);
                                    clone.ID = Math.round(Math.random() * 1000);
                                    e.component
                                        .getDataSource()
                                        .store()
                                        .insert(clone)
                                        .done(() => e.component.refresh());
                                }).then(() => {
                                    app.toast("Copied", "info");
                                });

                                e.event.preventDefault();
                            }
                        }
                    ]
                }
            ],
            summary: {
                recalculateWhileEditing: true,
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
                mode: "row",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            },
            onEditorPreparing: function (e) {
                if (e.parentType == "dataRow" && e.editorName == 'dxSelectBox') {
                    e.editorOptions.itemTemplate = function (data, index, element) {
                        var column = e.component.columnOption(e.dataField);
                        var fieldName = column.lookup.displayExpr;

                        if (data) {
                            $("<div>").css({ "white-space": "normal" }).text(data[fieldName]).appendTo(element);
                            return element;
                        } else {
                            return "item";
                        }

                    };
                    e.editorOptions.onOpened = function (e) { e.component._popup.option("width", 300); };
                }
            },
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            allowColumnReordering: true,
            allowColumnResizing: true,
            wordWrapEnabled: true,
            paging: {
                enabled: false
            },
            selection: {
                mode: "single"
            },
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
                        dataSource: treasury.dsBankCounterParty(),
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
                    format: "#.000 '%'"
                },
                {
                    dataField: "intProfitReceivable",
                    caption: "Interest/Profit Receivable",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 0
                    },
                    calculateCellValue: function (rowData) {
                        var currency = $currencySelectBox.option("value");
                        var principal = rowData.principal;
                        var tenor = treasury.tenor(rowData.maturityDate, rowData.valueDate);
                        var rate = rowData.ratePercent;

                        rowData.intProfitReceivable = treasury.outflow_depoInt(currency, principal, tenor, rate);
                        
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
                        precision: 0
                    },
                    calculateCellValue: function (rowData) {
                        var currency = $currencySelectBox.option("value");
                        var principal = rowData.principal;
                        var tenor = treasury.tenor(rowData.maturityDate, rowData.valueDate);
                        var rate = rowData.ratePercent;

                        rowData.principalIntProfitReceivable = treasury.outflow_depo_PrincipalInt(currency, principal, tenor, rate);
                        return rowData.principalIntProfitReceivable;
                    },
                    allowEditing: false
                },
                {
                    dataField: "assetType",
                    caption: "Asset Type",
                    lookup: {
                        dataSource: treasury.dsAssetType,
                        valueExpr: "value",
                        displayExpr: "value"
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
                        dataSource: treasury.dsNotes,
                        valueExpr: "value",
                        displayExpr: "value"
                    }
                },
                {
                    type: "buttons",
                    width: 110,
                    buttons: [
                        "edit",
                        "delete",
                        {
                            text: "Copy",
                            onClick: function (e) {
                                e.component.byKey(e.row.key).done((source) => {
                                    var clone = Object.assign({}, source);
                                    clone.ID = Math.round(Math.random() * 1000);
                                    e.component
                                        .getDataSource()
                                        .store()
                                        .insert(clone)
                                        .done(() => e.component.refresh());
                                }).then(() => {
                                    app.toast("Copied", "info");
                                });

                                e.event.preventDefault();
                            }
                        }
                    ]
                }
            ],
            summary: {
                recalculateWhileEditing: true,
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
                mode: "row",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            },
            onEditorPreparing: function (e) {
                if (e.parentType == "dataRow" && e.editorName == 'dxSelectBox') {
                    e.editorOptions.itemTemplate = function (data, index, element) {
                        var column = e.component.columnOption(e.dataField);
                        var fieldName = column.lookup.displayExpr;

                        if (data) {
                            $("<div>").css({ "white-space": "normal" }).text(data[fieldName]).appendTo(element);
                            return element;
                        } else {
                            return "item";
                        }

                    };
                    e.editorOptions.onOpened = function (e) { e.component._popup.option("width", 300); };
                }
            },
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            allowColumnReordering: true,
            allowColumnResizing: true,
            wordWrapEnabled: true,
            onToolbarPreparing: function (e) {
                var toolbarItems = e.toolbarOptions.items;
                toolbarItems.push({
                    widget: "dxButton",
                    options: {
                        icon: "gift",
                        hint: "Populate Sample Data",
                        onClick: function () {
                            $outflowDepositGrid.option("dataSource", sampleDataOutflowDeposit);
                            app.toast("Grid Outflow Deposit populated with sample data", "warning", 4000);
                        }
                    },
                    location: "after"
                });
            },
            paging: {
                enabled: false
            }
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
                        dataSource: treasury.dsIssuer(),
                        valueExpr: "name",
                        displayExpr: "name"
                    }
                },
                {
                    dataField: "productType",
                    caption: "Product Type",
                    lookup: {
                        dataSource: treasury.dsProductType,
                        valueExpr: "value",
                        displayExpr: "value"
                    }
                },
                {
                    dataField: "counterParty",
                    caption: "Counterparty",
                    lookup: {
                        dataSource: treasury.dsBankCounterParty(),
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
                        precision: 0
                    }
                },
                {
                    dataField: "sellPurchaseRateYield",
                    caption: "Sell Rate / Yield (%)",
                    dataType: "number",
                    format: "#.000 '%'"
                },
                {
                    dataField: "price",
                    caption: "Price",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 0
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
                        precision: 0
                    }
                },
                {
                    dataField: "intDividendReceivable",
                    caption: "Interest/Dividend Receivable",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 0
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
                        precision: 0
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
                },
                {
                    type: "buttons",
                    width: 110,
                    buttons: [
                        "edit",
                        "delete",
                        {
                            text: "Copy",
                            onClick: function (e) {
                                e.component.byKey(e.row.key).done((source) => {
                                    var clone = Object.assign({}, source);
                                    clone.ID = Math.round(Math.random() * 1000);
                                    e.component
                                        .getDataSource()
                                        .store()
                                        .insert(clone)
                                        .done(() => e.component.refresh());
                                }).then(() => {
                                    app.toast("Copied", "info");
                                });

                                e.event.preventDefault();
                            }
                        }
                    ]
                }
            ],
            summary: {
                recalculateWhileEditing: true,
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
                mode: "row",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            },
            onEditorPreparing: function (e) {
                if (e.parentType == "dataRow" && e.editorName == 'dxSelectBox') {
                    e.editorOptions.itemTemplate = function (data, index, element) {
                        var column = e.component.columnOption(e.dataField);
                        var fieldName = column.lookup.displayExpr;

                        if (data) {
                            $("<div>").css({ "white-space": "normal" }).text(data[fieldName]).appendTo(element);
                            return element;
                        } else {
                            return "item";
                        }
                            
                    };
                    e.editorOptions.onOpened = function (e) { e.component._popup.option("width", 300); };
                }
            },
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            allowColumnReordering: true,
            allowColumnResizing: true,
            wordWrapEnabled: true,
            onToolbarPreparing: function(e) {
                var toolbarItems = e.toolbarOptions.items;
                toolbarItems.push({
                    widget: "dxButton",
                    options: {
                        icon: "gift",
                        hint: "Populate Sample Data",
                        onClick: function () {
                            $inflowMmiGrid.option("dataSource", sampleDataInflowMmi);
                            app.toast("Grid Inflow MMI populated with sample data", "warning", 4000);
                        }
                    },
                    location: "after"
                });
            },
            paging: {
                enabled: false
            }
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
                        dataSource: treasury.dsIssuer(),
                        valueExpr: "name",
                        displayExpr: "name"
                    }
                },
                {
                    dataField: "productType",
                    caption: "Product Type",
                    lookup: {
                        dataSource: treasury.dsProductType,
                        valueExpr: "value",
                        displayExpr: "value"
                    }
                },
                {
                    dataField: "counterParty",
                    caption: "Counterparty",
                    lookup: {
                        dataSource: treasury.dsBankCounterParty(),
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
                    format: "#.000 '%'"
                },
                {
                    dataField: "price",
                    caption: "Price",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 0
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
                },
                {
                    type: "buttons",
                    width: 110,
                    buttons: [
                        "edit",
                        "delete",
                        {
                            text: "Copy",
                            onClick: function (e) {
                                e.component.byKey(e.row.key).done((source) => {
                                    var clone = Object.assign({}, source);
                                    clone.ID = Math.round(Math.random() * 1000);
                                    e.component
                                        .getDataSource()
                                        .store()
                                        .insert(clone)
                                        .done(() => e.component.refresh());
                                }).then(() => {
                                    app.toast("Copied", "info");
                                });

                                e.event.preventDefault();
                            }
                        }
                    ]
                }
            ],
            summary: {
                recalculateWhileEditing: true,
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
                mode: "row",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            },
            onEditorPreparing: function (e) {
                if (e.parentType == "dataRow" && e.editorName == 'dxSelectBox') {
                    e.editorOptions.itemTemplate = function (data, index, element) {
                        var column = e.component.columnOption(e.dataField);
                        var fieldName = column.lookup.displayExpr;

                        if (data) {
                            $("<div>").css({ "white-space": "normal" }).text(data[fieldName]).appendTo(element);
                            return element;
                        } else {
                            return "item";
                        }

                    };
                    e.editorOptions.onOpened = function (e) { e.component._popup.option("width", 300); };
                }
            },
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            allowColumnReordering: true,
            allowColumnResizing: true,
            wordWrapEnabled: true,
            onToolbarPreparing: function (e) {
                var toolbarItems = e.toolbarOptions.items;
                toolbarItems.push({
                    widget: "dxButton",
                    options: {
                        icon: "gift",
                        hint: "Populate Sample Data",
                        onClick: function () {
                            $outflowMmiGrid.option("dataSource", sampleDataOutflowMmi);
                            app.toast("Grid Outflow MMI populated with sample data", "warning", 4000);
                        }
                    },
                    location: "after"
                });
            },
            paging: {
                enabled: false
            }
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