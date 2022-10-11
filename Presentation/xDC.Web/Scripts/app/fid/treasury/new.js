﻿(function ($, window, document) {

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
            $valueDate,
            $approverDropdown,
            $approvalNotes,

            $edwAvailable,

            isSaveAsDraft = false;

        var referenceUrl = {
            loadCurrencies: window.location.origin + "/api/common/GetTradeSettlementCurrencies",

            dsMaturity: window.location.origin + "/api/fid/Treasury/EdwMaturity/",
            dsMm: window.location.origin + "/api/fid/Treasury/EdwMmi/",

            dsEdwAvailability: window.location.origin + "/api/fid/Treasury/EdwDataAvailability",

            dsApproverList: window.location.origin + "/api/common/approverList/treasury",

            dsFcaAccount: window.location.origin + "/api/fid/FcaTagging/FcaAccount",

            postNewFormRequest: window.location.origin + "/api/fid/Treasury/New",
            postNewFormResponse: window.location.origin + "/fid/Treasury/View/"
        };
        
        //#endregion

        //#region Data Source & Functions
        var parseDepositArray = function (dataGridData) {
            if (dataGridData.length > 0) {
                var x = dataGridData.map(function (x) {
                    for (key in x) {
                        return {
                            dealer: x.dealer,
                            bank: x.bank,
                            tradeDate: (x.tradeDate instanceof Date) ? x.tradeDate.toISOString() : x.tradeDate,
                            valueDate: (x.valueDate instanceof Date) ? x.valueDate.toISOString() : x.valueDate,
                            maturityDate: (x.maturityDate instanceof Date) ? x.maturityDate.toISOString() : x.maturityDate,
                            principal: x.principal,
                            tenor: x.tenor,
                            ratePercent: x.ratePercent,
                            intProfitReceivable: x.intProfitReceivable,
                            principalIntProfitReceivable: x.principalIntProfitReceivable,
                            assetType: x.assetType,
                            repoTag: x.repoTag,
                            contactPerson: x.contactPerson,
                            notes: x.notes,
                            fcaAccount: x.fcaAccount
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
                            maturityDate: (x.maturityDate instanceof Date) ? x.maturityDate.toISOString() : x.maturityDate,
                            nominal: x.nominal,
                            price: x.price,
                            proceeds: x.proceeds,
                            productType: x.productType,
                            purchaseProceeds: x.purchaseProceeds,
                            sellPurchaseRateYield: x.sellPurchaseRateYield,
                            tradeDate: (x.tradeDate instanceof Date) ? x.tradeDate.toISOString() : x.tradeDate,
                            valueDate: (x.valueDate instanceof Date) ? x.valueDate.toISOString() : x.valueDate,
                            fcaAccount: x.fcaAccount
                        };
                    }
                });

                return x;
            } else {
                return [];
            }
        }

        var dsAccountLookup = function () {
            return {
                store: DevExpress.data.AspNet.createStore({
                    key: "name",
                    loadUrl: referenceUrl.dsFcaAccount
                }),
                paginate: true,
                pageSize: 20
            };
        }
        
        var dsMaturity = function (valueDateEpoch, currency) {
            return $.ajax({
                url: referenceUrl.dsMaturity + "/" + moment(valueDateEpoch).unix() + "/" + currency,
                type: "get"
            });
        };

        var dsMm = function (valueDateEpoch, currency) {
            return $.ajax({
                url: referenceUrl.dsMm + "/" + moment(valueDateEpoch).unix() + "/" + currency,
                type: "get"
            });
        };

        var dsEdwAvailability = function (valueDateEpoch, currency) {
            return $.ajax({
                url: referenceUrl.dsEdwAvailability + "/" + moment(valueDateEpoch).unix() + "/" + currency,
                type: "get"
            });
        };

        var checkDwDataAvailibility = function (valueDate, currency) {
            app.clearAllGrid($inflowDepositGrid, $inflowMmiGrid, $outflowDepositGrid, $outflowMmiGrid);

            if (valueDate && currency) {
                $.when(
                    dsEdwAvailability(valueDate, currency)
                    )
                    .done(function (data1) {
                        $edwAvailable.option("dataSource", data1);
                    })
                    .always(function (dataOrjqXHR, textStatus, jqXHRorErrorThrown) {
                        
                    })
                    .then(function () {

                    });
            } else {
                
            }
        }

        var populateDwData = function (categoryType, valueDate, currency) {
            if (valueDate && currency) {
                if (categoryType == 1) {
                    $.when(
                        dsMaturity(valueDate, currency)
                    )
                        .done(function (data1) {
                            $inflowDepositGrid.option("dataSource", []);
                            $inflowDepositGrid.option("dataSource", data1.data);
                            $inflowDepositGrid.repaint();

                            app.toastEdwCount(data1.data, "Inflow Deposit Maturity");
                        })
                        .always(function (dataOrjqXHR, textStatus, jqXHRorErrorThrown) {

                        })
                        .then(function () {

                        });
                } else {
                    $.when(
                        dsMm(valueDate, currency)
                    )
                        .done(function (data1) {
                            $inflowMmiGrid.option("dataSource", []);
                            $inflowMmiGrid.option("dataSource", data1.data);
                            $inflowMmiGrid.repaint();

                            app.toastEdwCount(data1.data, "Inflow Money Market");
                        })
                        .always(function (dataOrjqXHR, textStatus, jqXHRorErrorThrown) {

                        })
                        .then(function () {

                        });

                }
            } else {
                dxGridUtils.clearGrid($inflowDepositGrid);
            }
        }



        function postData(isDraft) {
            if (isDraft) {
                app.toast("Saving....", "info", 3000);
            } else {
                app.toast("Submitting for approval....", "info", 3000);
            }

            var data = {
                currency: $currencySelectBox.option("value"),
                valueDate: moment($valueDate.option("value")).unix(),
                
                inflowDeposit: parseDepositArray($inflowDepositGrid.getDataSource().items()),
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
        
        //#endregion
        
        //#region Other Widgets

        $valueDate = $("#valueDate").dxDateBox({
            type: "date",
            displayFormat: "dd/MM/yyyy",
            value: new Date()
        }).dxDateBox("instance");

        $currencySelectBox = $("#currency").dxSelectBox({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.loadCurrencies
            }),
            displayExpr: "value",
            valueExpr: "value",
            placeHolder: "Currency..",
            deferRendering: false,
            onValueChanged: function (data) {
                if (data.value == "MYR") {
                    
                }
                $outflowDepositGrid.refresh();
            }
        })
            .dxValidator({ validationRules: [{ type: "required", message: "Currency is required" }] })
            .dxSelectBox("instance");

        $edwAvailable = $("#edwAvailable").dxList({
            activeStateEnabled: false,
            focusStateEnabled: false,
            itemTemplate: function (data, index) {
                var result = $("<div>");

                $("<div>").text(data.name + " × " + data.numbers).appendTo(result);
                $("<a>").append("<i class='fa fa-download'></i> Populate").on("dxclick", function (e) {

                    populateDwData(data.categoryType, $valueDate.option("value"), $currencySelectBox.option("value"));
                    
                    e.stopPropagation();
                }).appendTo(result);

                return result;
            }
        }).dxList("instance");
        
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

        var dxDataGridConfig_Deposit = {
            dataSource: [],
            columns: [
                {
                    caption: "#",
                    cellTemplate: function (cellElement, cellInfo) {
                        cellElement.text(cellInfo.row.rowIndex + 1);
                    },
                    allowEditing: false,
                    width: 50
                },
                {
                    dataField: "dealer",
                    caption: "Dealer",
                    allowEditing: false,
                    calculateCellValue: function (rowData) {
                        if (rowData.dealer) {
                            return rowData.dealer;
                        } else {
                            return window.currentUser;
                        }
                    },
                    width: 110
                },
                {
                    dataField: "bank",
                    caption: "Bank",
                    lookup: {
                        dataSource: treasury.dsBankCounterParty(),
                        valueExpr: "name",
                        displayExpr: "name"
                    },
                    width: 200
                },
                {
                    dataField: "tradeDate",
                    caption: "Trade Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    editorOptions: {
                        placeholder: "dd/MM/yyyy",
                        showClearButton: true
                    },
                    width: 120
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
                    width: 120
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
                    width: 120
                },
                {
                    dataField: "principal",
                    caption: "Principal",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    width: 130
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
                        return Number(rowData.tenor);
                    },
                    allowEditing: false,
                    width: 60
                },
                {
                    dataField: "ratePercent",
                    caption: "Rate (%)",
                    dataType: "number",
                    format: "#.000 '%'",
                    width: 80
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
                        return Number(rowData.intProfitReceivable);
                    },
                    allowEditing: false,
                    width: 130
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
                        return Number(rowData.principalIntProfitReceivable);
                    },
                    allowEditing: false,
                    width: 130
                },
                {
                    dataField: "assetType",
                    caption: "Asset Type",
                    lookup: {
                        dataSource: treasury.dsAssetType(),
                        valueExpr: "value",
                        displayExpr: "value"
                    },
                    width: 120
                },
                {
                    dataField: "repoTag",
                    caption: "REPO tag",
                    width: 100
                },
                {
                    dataField: "contactPerson",
                    caption: "Contact Person",
                    width: 100
                },
                {
                    dataField: "notes",
                    caption: "Notes",
                    lookup: {
                        dataSource: treasury.dsNotes(),
                        valueExpr: "value",
                        displayExpr: "value"
                    },
                    width: 140
                },
                {
                    dataField: "fcaAccount",
                    caption: "FCA",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "name",
                        displayExpr: "name",
                        allowClearing: true
                    },
                    width: 140
                },
                {
                    type: "buttons",
                    width: 110,
                    fixedPosition: "left",
                    fixed: true,
                    buttons: [
                        "edit",
                        "delete",
                        {
                            text: "Copy",
                            onClick: function (e) {
                                e.component.byKey(e.row.key).done((source) => {
                                    var clone = Object.assign({}, source);
                                    clone.id = null;

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
                        column: "tenor",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 0
                        }
                    },
                    {
                        column: "principal",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "intProfitReceivable",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
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
            onInitNewRow: function (e) {
                e.data.tradeDate = new Date();
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
                mode: "multiple",
                showCheckBoxesMode: "none"
            },
            columnFixing: {
                enabled: true,
            },
        };

        $inflowDepositGrid = $("#inflowDepositGrid").dxDataGrid({
            dataSource: [],
            toolbar: {
                items: [
                    {
                        name: "addRowButton",
                        showText: "always",
                        location: "before"
                    },
                    {
                        widget: "dxButton",
                        options: {
                            icon: "fa fa-trash",
                            text: "Remove all rows",
                            onClick: function () {
                                $inflowDepositGrid.option("dataSource", []);
                            }
                        },
                        location: "before"
                    },
                    {
                        widget: "dxButton",
                        options: {
                            icon: "fa fa-clone",
                            text: "Copy to rollover",
                            hint: "Copy selected row into rollover table",
                            onClick: function (e) {

                                if ($inflowDepositGrid.getSelectedRowsData().length > 0) {
                                    $inflowDepositGrid.getSelectedRowsData().forEach(function (i) {
                                        var dataSource = $outflowDepositGrid.getDataSource();
                                        dataSource.store().insert({
                                            id: Math.floor(Math.random() * 99) + 1,
                                            dealer: i.dealer,
                                            bank: i.bank,
                                            tradeDate: i.tradeDate,
                                            valueDate: i.valueDate,
                                            maturityDate: i.maturityDate,
                                            principal: i.principalIntProfitReceivable,
                                            ratePercent: i.ratePercent,
                                            assetType: i.assetType,
                                            repoTag: i.repoTag,
                                            contactPerson: i.contactPerson,
                                            notes: i.notes,
                                            fcaAccount: i.fcaAccount
                                        }).then(function () {
                                            dataSource.reload();
                                        })
                                    });

                                    $outflowDepositGrid.refresh();

                                } else {

                                    app.toast("Please select at least one row to copy over.", "error")

                                }

                                e.event.preventDefault();
                            }
                        },
                        location: "before"
                    }
                ]
            }
        }).dxDataGrid("instance");

        $outflowDepositGrid = $("#outflowDepositGrid").dxDataGrid({
            dataSource: [],
            toolbar: {
                items: [
                    {
                        name: "addRowButton",
                        showText: "always",
                        location: "before"
                    },
                    {
                        widget: "dxButton",
                        options: {
                            icon: "fa fa-trash",
                            text: "Remove all rows",
                            onClick: function () {
                                $outflowDepositGrid.option("dataSource", []);
                            }
                        },
                        location: "before"
                    },
                ]
            }
        }).dxDataGrid("instance");

        $inflowDepositGrid.option(dxDataGridConfig_Deposit);
        $outflowDepositGrid.option(dxDataGridConfig_Deposit);


        var dxDataGridConfig_Mmi = {
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
            onInitNewRow: function (e) {
                e.data.tradeDate = new Date();
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
            columnFixing: {
                enabled: true,
            }
        };


        $inflowMmiGrid = $("#inflowMmiGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    caption: "#",
                    cellTemplate: function (cellElement, cellInfo) {
                        cellElement.text(cellInfo.row.rowIndex + 1);
                    },
                    allowEditing: false,
                    width: 50
                },
                {
                    dataField: "dealer",
                    caption: "Dealer",
                    allowEditing: false,
                    calculateCellValue: function (rowData) {
                        if (rowData.dealer) {
                            return rowData.dealer;
                        } else {
                            return window.currentUser;
                        }
                    },
                    width: 110
                },
                {
                    dataField: "issuer",
                    caption: "Issuer",
                    lookup: {
                        dataSource: treasury.dsIssuer(),
                        valueExpr: "name",
                        displayExpr: "name"
                    },
                    width: 200
                },
                {
                    dataField: "productType",
                    caption: "Product Type",
                    lookup: {
                        dataSource: treasury.dsProductType,
                        valueExpr: "value",
                        displayExpr: "value"
                    },
                    width: 60
                },
                {
                    dataField: "counterParty",
                    caption: "Counterparty",
                    lookup: {
                        dataSource: treasury.dsBankCounterParty(),
                        valueExpr: "name",
                        displayExpr: "name"
                    },
                    width: 100
                },
                {
                    dataField: "tradeDate",
                    caption: "Trade Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    editorOptions: {
                        placeholder: "dd/MM/yyyy",
                        showClearButton: true
                    },
                    width: 120
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
                    width: 120
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
                    width: 120
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
                        return Number(rowData.holdingDayTenor);
                    },
                    allowEditing: false,
                    width: 60
                },
                {
                    dataField: "nominal",
                    caption: "Nominal",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    width: 130
                },
                {
                    dataField: "sellPurchaseRateYield",
                    caption: "Sell Rate / Yield (%)",
                    dataType: "number",
                    format: "#.000 '%'",
                    width: 80
                },
                {
                    dataField: "price",
                    caption: "Price",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 4
                    },
                    calculateCellValue: function (rowData) {
                        rowData.price = treasury.inflow_price(
                            rowData.productType,
                            rowData.nominal,
                            rowData.sellPurchaseRateYield,
                            treasury.tenor(rowData.maturityDate, rowData.valueDate)
                        );
                        return Number(rowData.price);
                    },
                    allowEditing: false,
                    width: 130
                },
                {
                    dataField: "purchaseProceeds",
                    caption: "Purchase Proceeds",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    width: 130
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
                        return Number(rowData.intDividendReceivable);
                    },
                    allowEditing: false,
                    width: 130
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
                        return Number(rowData.proceeds);
                    },
                    allowEditing: false,
                    width: 130
                },
                {
                    dataField: "certNoStockCode",
                    caption: "Certificate No. / Stock Code",
                    width: 100
                },
                {
                    dataField: "fcaAccount",
                    width: "150px",
                    caption: "FCA",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "name",
                        displayExpr: "name",
                        allowClearing: true
                    },
                    width: 125
                },
                {
                    type: "buttons",
                    width: 110,
                    fixedPosition: "left",
                    fixed: true,
                    buttons: [
                        "edit",
                        "delete",
                        {
                            text: "Copy",
                            onClick: function (e) {
                                e.component.byKey(e.row.key).done((source) => {
                                    var clone = Object.assign({}, source);
                                    clone.id = null;

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
                            precision: 2
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
            toolbar: {
                items: [
                    {
                        name: "addRowButton",
                        showText: "always",
                        location: "before"
                    },
                    {
                        widget: "dxButton",
                        options: {
                            icon: "fa fa-trash",
                            text: "Remove all rows",
                            onClick: function () {
                                $inflowMmiGrid.option("dataSource", []);
                            }
                        },
                        location: "before"
                    },
                ]
            }
        }).dxDataGrid("instance");

        $outflowMmiGrid = $("#outflowMmiGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    caption: "#",
                    cellTemplate: function (cellElement, cellInfo) {
                        cellElement.text(cellInfo.row.rowIndex + 1);
                    },
                    allowEditing: false,
                    width: 50
                },
                {
                    dataField: "dealer",
                    caption: "Dealer",
                    allowEditing: false,
                    calculateCellValue: function (rowData) {
                        rowData.dealer = window.currentUser;
                        return window.currentUser;
                    },
                    width: 110
                },
                {
                    dataField: "issuer",
                    caption: "Issuer",
                    lookup: {
                        dataSource: treasury.dsIssuer(),
                        valueExpr: "name",
                        displayExpr: "name"
                    },
                    width: 200
                },
                {
                    dataField: "productType",
                    caption: "Product Type",
                    lookup: {
                        dataSource: treasury.dsProductType,
                        valueExpr: "value",
                        displayExpr: "value"
                    },
                    width: 60
                },
                {
                    dataField: "counterParty",
                    caption: "Counterparty",
                    lookup: {
                        dataSource: treasury.dsBankCounterParty(),
                        valueExpr: "name",
                        displayExpr: "name"
                    },
                    width: 100
                },
                {
                    dataField: "tradeDate",
                    caption: "Trade Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    editorOptions: {
                        placeholder: "dd/MM/yyyy",
                        showClearButton: true
                    },
                    width: 120
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
                    width: 120
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
                    width: 120
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
                        return Number(rowData.holdingDayTenor);
                    },
                    allowEditing: false,
                    width: 60
                },
                {
                    dataField: "nominal",
                    caption: "Nominal",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    width: 130
                },
                {
                    dataField: "sellPurchaseRateYield",
                    caption: "Purchase Rate / Yield (%)",
                    dataType: "number",
                    format: "#.000 '%'",
                    width: 80
                },
                {
                    dataField: "price",
                    caption: "Price",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 4
                    },
                    calculateCellValue: function (rowData) {
                        rowData.price = treasury.outflow_price(
                            rowData.productType,
                            rowData.nominal,
                            rowData.sellPurchaseRateYield,
                            treasury.tenor(rowData.maturityDate, rowData.valueDate)
                        );
                        return Number(rowData.price);
                    },
                    allowEditing: false,
                    width: 130
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
                        return Number(rowData.intDividendReceivable);
                    },
                    allowEditing: false,
                    width: 130
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
                        return Number(rowData.proceeds);
                    },
                    allowEditing: false,
                    width: 130
                },
                {
                    dataField: "certNoStockCode",
                    caption: "Certificate No. / Stock Code",
                    width: 130
                },
                {
                    dataField: "fcaAccount",
                    width: "150px",
                    caption: "FCA",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "name",
                        displayExpr: "name",
                        allowClearing: true
                    },
                    width: 125
                },
                {
                    type: "buttons",
                    width: 110,
                    fixedPosition: "left",
                    fixed: true,
                    buttons: [
                        "edit",
                        "delete",
                        {
                            text: "Copy",
                            onClick: function (e) {
                                e.component.byKey(e.row.key).done((source) => {
                                    var clone = Object.assign({}, source);
                                    clone.id = null;

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
                            precision: 2
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
            toolbar: {
                items: [
                    {
                        name: "addRowButton",
                        showText: "always",
                        location: "before"
                    },
                    {
                        widget: "dxButton",
                        options: {
                            icon: "fa fa-trash",
                            text: "Remove all rows",
                            onClick: function () {
                                $outflowMmiGrid.option("dataSource", []);
                            }
                        },
                        location: "before"
                    },
                ]
            }
        }).dxDataGrid("instance");

        $inflowMmiGrid.option(dxDataGridConfig_Mmi);
        $outflowMmiGrid.option(dxDataGridConfig_Mmi);

        

        // #endregion Data Grid
 
        //#region Events & Invocations

        $valueDate.on("valueChanged", function (data) {
            checkDwDataAvailibility(data.value, $currencySelectBox.option("value"));
        });

        $currencySelectBox.on("valueChanged", function (data) {
            checkDwDataAvailibility($valueDate.option("value"), data.value);
        });

        $currencySelectBox.on("contentReady", function (e) {
            
            $currencySelectBox.option("value", e.component.getDataSource().items()[0].value);
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

        //$currencySelectBox.option("value", $currencySelectBox.getDataSource()[0]);
        

        //#endregion
    });
}(window.jQuery, window, document));