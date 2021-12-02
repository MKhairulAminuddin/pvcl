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
            dsMaturity: window.location.origin + "/api/fid/Treasury/EdwMaturity/",
            dsBankCounterParty: window.location.origin + "/api/fid/Treasury/EdwBankCounterParty/",
            dsIssuer: window.location.origin + "/api/fid/Treasury/EdwIssuer/",

            dsInflowDeposit: window.location.origin + "/api/fid/Treasury/inflow/deposit/",
            dsOutflowDeposit: window.location.origin + "/api/fid/Treasury/outflow/deposit/",

            dsInflowMmi: window.location.origin + "/api/fid/Treasury/inflow/mmi/",
            dsOutflowMmi: window.location.origin + "/api/fid/Treasury/outflow/mmi/",

            dsApproverList: window.location.origin + "/api/common/approverList/treasury",

            dsFcaAccount: window.location.origin + "/api/fid/TcaTagging/FcaAccount",

            postNewFormRequest: window.location.origin + "/api/fid/Treasury/Edit",
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

        var dsBankCounterParty = DevExpress.data.AspNet.createStore({
            key: "reference",
            loadUrl: referenceUrl.dsBankCounterParty
        });

        var dsIssuer = DevExpress.data.AspNet.createStore({
            key: "reference",
            loadUrl: referenceUrl.dsIssuer
        });

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

        var dsInflowDeposit = function () {
            return $.ajax({
                url: referenceUrl.dsInflowDeposit + window.location.pathname.split("/").pop(),
                type: "get"
            });
        };

        var dsOutflowDeposit = function () {
            return $.ajax({
                url: referenceUrl.dsOutflowDeposit + window.location.pathname.split("/").pop(),
                type: "get"
            });
        };

        var dsInflowMmi = function () {
            return $.ajax({
                url: referenceUrl.dsInflowMmi + window.location.pathname.split("/").pop(),
                type: "get"
            });
        };

        var dsOutflowMmi = function () {
            return $.ajax({
                url: referenceUrl.dsOutflowMmi + window.location.pathname.split("/").pop(),
                type: "get"
            });
        };

        var populateData = function (tradeDate, currency) {
            $.when(
                    dsInflowDeposit(),
                    dsOutflowDeposit(),
                    dsInflowMmi(),
                    dsOutflowMmi()
                    )
                .done(function (inflowDepo, outflowDepo, inflowMmi, outflowMmi) {
                    $inflowDepositGrid.option("dataSource", inflowDepo[0].data);
                    $inflowDepositGrid.repaint();

                    $outflowDepositGrid.option("dataSource", outflowDepo[0].data);
                    $outflowDepositGrid.repaint();

                    $inflowMmiGrid.option("dataSource", inflowMmi[0].data);
                    $inflowMmiGrid.repaint();

                    $outflowMmiGrid.option("dataSource", outflowMmi[0].data);
                    $outflowMmiGrid.repaint();
                })
                .always(function (dataOrjqXHR, textStatus, jqXHRorErrorThrown) {
                    
                })
                .then(function () {

                });
        }

        function postData(isDraft) {
            if (isDraft) {
                app.toast("Saving....", "info", 3000);
            } else {
                app.toast("Submitting for approval....", "info", 3000);
            }
            
            var data = {
                id: app.getUrlId(),
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

        $currencySelectBox = $("#currencySelectBox").dxTextBox("instance");
        
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
            itemTemplate: function (data) {
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
                    caption: "#",
                    cellTemplate: function (cellElement, cellInfo) {
                        cellElement.text(cellInfo.row.rowIndex + 1);
                    },
                    allowEditing: false,
                    width: "30px"
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

                        return Number(rowData.intProfitReceivable);
                    },
                    calculateDisplayValue: function (rowData) {
                        if (rowData.intProfitReceivable >= 0) {
                            return treasury.toFixed(rowData.intProfitReceivable, 2);
                        } else {
                            return rowData.intProfitReceivable;
                        }
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
                        return Number(rowData.principalIntProfitReceivable);
                    },
                    calculateDisplayValue: function (rowData) {
                        if (rowData.principalIntProfitReceivable >= 0) {
                            return treasury.toFixed(rowData.principalIntProfitReceivable, 2);

                        } else {
                            return rowData.principalIntProfitReceivable;
                        }
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
                    caption: "REPO tag"
                },
                {
                    dataField: "contactPerson",
                    caption: "Contact Person"
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
                    dataField: "fcaAccount",
                    width: "150px",
                    caption: "FCA",
                    lookup: {
                        dataSource: dsAccountLookup,
                        valueExpr: "name",
                        displayExpr: "name",
                        allowClearing: true
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
                        name: "customSummary1",
                        summaryType: "custom",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "principalIntProfitReceivable",
                        name: "customSummary1",
                        summaryType: "custom",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ],
                calculateCustomSummary: function (options) {
                    // Calculating "customSummary1"
                    if (options.name == "customSummary1") {
                        switch (options.summaryProcess) {
                            case "start":
                                // Initializing "totalValue" here
                                options.totalValue = 0;

                                break;
                            case "calculate":
                                // Modifying "totalValue" here
                                options.totalValue += Number(treasury.toFixed(options.value, 2));

                                break;
                            case "finalize":
                                // Assigning the final value to "totalValue" here
                                break;
                        }
                    }

                }
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
            onToolbarPreparing: function (e) {
                var toolbarItems = e.toolbarOptions.items;
                toolbarItems.push({
                    widget: "dxButton",
                    options: {
                        icon: "fa fa-trash",
                        hint: "Remove records",
                        onClick: function () {
                            $inflowDepositGrid.option("dataSource", []);
                        }
                    },
                    location: "after"
                });
            },
        }).dxDataGrid("instance");

        $outflowDepositGrid = $("#outflowDepositGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    caption: "#",
                    cellTemplate: function (cellElement, cellInfo) {
                        cellElement.text(cellInfo.row.rowIndex + 1);
                    },
                    allowEditing: false,
                    width: "30px"
                },
                {
                    dataField: "dealer",
                    caption: "Dealer",
                    calculateCellValue: function (rowData) {
                        if (rowData.dealer) {
                            return rowData.dealer;
                        } else {
                            return window.currentUser;
                        }
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
                    calculateDisplayValue: function (rowData) {
                        if (rowData.intProfitReceivable >= 0) {
                            return treasury.toFixed(rowData.intProfitReceivable, 2);
                        } else {
                            return rowData.intProfitReceivable;
                        }
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
                        return Number(rowData.principalIntProfitReceivable);
                    },
                    calculateDisplayValue: function (rowData) {
                        if (rowData.principalIntProfitReceivable >= 0) {
                            return treasury.toFixed(rowData.principalIntProfitReceivable, 2);

                        } else {
                            return rowData.principalIntProfitReceivable;
                        }
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
                        dataSource: treasury.dsNotes(),
                        valueExpr: "value",
                        displayExpr: "value"
                    }
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
                        name: "customSummary1",
                        summaryType: "custom",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "principalIntProfitReceivable",
                        name: "customSummary1",
                        summaryType: "custom",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    }
                ],
                calculateCustomSummary: function (options) {
                    // Calculating "customSummary1"
                    if (options.name == "customSummary1") {
                        switch (options.summaryProcess) {
                            case "start":
                                // Initializing "totalValue" here
                                options.totalValue = 0;

                                break;
                            case "calculate":
                                // Modifying "totalValue" here
                                options.totalValue += Number(treasury.toFixed(options.value, 2));

                                break;
                            case "finalize":
                                // Assigning the final value to "totalValue" here
                                break;
                        }
                    }

                }
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
            onToolbarPreparing: function (e) {
                var toolbarItems = e.toolbarOptions.items;
                toolbarItems.push({
                    widget: "dxButton",
                    options: {
                        icon: "fa fa-trash",
                        hint: "Remove records",
                        onClick: function () {
                            $outflowDepositGrid.option("dataSource", []);
                        }
                    },
                    location: "after"
                });
            }
        }).dxDataGrid("instance");

        $inflowMmiGrid = $("#inflowMmiGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    caption: "#",
                    cellTemplate: function (cellElement, cellInfo) {
                        cellElement.text(cellInfo.row.rowIndex + 1);
                    },
                    allowEditing: false,
                    width: "30px"
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
                        return Number(rowData.intDividendReceivable);
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
                        return Number(rowData.proceeds);
                    },
                    allowEditing: false
                },
                {
                    dataField: "certNoStockCode",
                    caption: "Certificate No. / Stock Code"
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
                        icon: "fa fa-trash",
                        hint: "Remove records",
                        onClick: function () {
                            $inflowMmiGrid.option("dataSource", []);
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
                    caption: "#",
                    cellTemplate: function (cellElement, cellInfo) {
                        cellElement.text(cellInfo.row.rowIndex + 1);
                    },
                    allowEditing: false,
                    width: "30px"
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
                        return Number(rowData.intDividendReceivable);
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
                        return Number(rowData.proceeds);
                    },
                    allowEditing: false
                },
                {
                    dataField: "certNoStockCode",
                    caption: "Certificate No. / Stock Code"
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
                        icon: "fa fa-trash",
                        hint: "Remove records",
                        onClick: function () {
                            $outflowMmiGrid.option("dataSource", []);
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
                app.saveAllGrids($inflowDepositGrid, $outflowDepositGrid, $inflowMmiGrid, $outflowMmiGrid);
                
                $selectApproverModal.modal('show');

                e.preventDefault();
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

        populateData();

        //#endregion
    });
}(window.jQuery, window, document));