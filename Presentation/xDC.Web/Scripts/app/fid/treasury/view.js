(function ($, window, document) {

    app.setSideMenuItemActive("/Fid/Treasury");

    $(function () {
        //#region Variable Definition

        var $inflowTabpanel,
            $inflowDepositGrid,
            $inflowMmiGrid,

            $outflowDepositGrid,
            $outflowMmiGrid,

            $editDraftBtn = $("#editDraftBtn"),
            
            $approverDropdown,
            $viewWorkflowModal = $("#viewWorkflowModal"),
            $approvalNoteModal = $("#approvalNoteModal"),
            $rejectionNoteModal = $("#rejectionNoteModal"),
            $approvalReassignModal = $("#approvalReassignModal"),
            $viewAuditTrailModal = $("#viewAuditTrailModal");

        var referenceUrl = {
            loadWorkflow: window.location.origin + "/api/common/GetWorkflow/11/" + app.getUrlId(),
            loadAuditTrail: window.location.origin + "/api/common/FormAuditTrail/11/" + app.getUrlId(),

            dsInflowDeposit: window.location.origin + "/api/fid/Treasury/inflow/deposit/",
            dsOutflowDeposit: window.location.origin + "/api/fid/Treasury/outflow/deposit/",

            dsInflowMmi: window.location.origin + "/api/fid/Treasury/inflow/mmi/",
            dsOutflowMmi: window.location.origin + "/api/fid/Treasury/outflow/mmi/",

            approvalRequest: window.location.origin + "/api/fid/Treasury/Approval",
            approvalResponse: window.location.origin + "/fid/Treasury/view/",

            reassignApprover: window.location.origin + "/api/fid/Treasury/Reassign",
            
            postNewFormRequest: window.location.origin + "/api/fid/Treasury/New",
            postNewFormResponse: window.location.origin + "/fid/Treasury",

            printRequest: window.location.origin + "/api/fid/Treasury/GenFile",
            printResponse: window.location.origin + "/fid/Treasury/Download/",

            editForm: window.location.origin + "/fid/Treasury/Edit/"
        };
        
        //#endregion

        //#region Data Source & Functions

        var dsApproverList = function () {
            return {
                store: DevExpress.data.AspNet.createStore({
                    key: "id",
                    loadUrl: window.location.origin + "/api/common/approverList/treasury"
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

        var populateData = function () {
            
                $.when(
                        dsInflowDeposit(),
                        dsOutflowDeposit(),
                        dsInflowMmi(),
                        dsOutflowMmi()
                )
                    .done(function (inflowDeposit, outflowDeposit, inflowMmi, outflowMmi) {
                        $inflowDepositGrid.option("dataSource", inflowDeposit[0].data);
                        $inflowDepositGrid.repaint();

                        $outflowDepositGrid.option("dataSource", outflowDeposit[0].data);
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

        var postApproval = function (isApproved) {
            if (isApproved) {
                app.toast("Submitting approval response....", "info", 3000);
            } else {
                app.toast("Submitting rejection response....", "warning", 3000);
            }

            var data = {
                approvalNote: (isApproved)
                    ? $("#approvalNoteTextBox").dxTextArea("instance").option("value")
                    : $("#rejectionNoteTextBox").dxTextArea("instance").option("value"),
                approvalStatus: isApproved,
                formId: app.getUrlId()
            };

            $.ajax({
                data: data,
                dataType: "json",
                url: referenceUrl.approvalRequest,
                method: "post",
                success: function (data) {
                    window.location.href = referenceUrl.approvalResponse + data;
                },
                fail: function (jqXHR, textStatus, errorThrown) {
                    app.alertError(textStatus + ": " + errorThrown);
                },
                complete: function (data) {
                    if (isApproved) {
                        $approvalNoteModal.modal("hide");
                    } else {
                        $rejectionNoteModal.modal("hide");
                    }
                }
            });
        }
        
        //#endregion

        //#region Other Widgets

        $approverDropdown = $("#newApproverDropdown").dxSelectBox({
            dataSource: dsApproverList(),
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

        $("#approvalReassignModalBtn").dxButton({
            onClick: function (e) {
                if ($approverDropdown.option("value") != null) {
                    //reassign
                    app.toast("Reassinging...");

                    var data = {
                        formId: app.getUrlId(),
                        approver: $approverDropdown.option("value"),
                        formType: 11
                    };

                    $.ajax({
                        type: "POST",
                        url: referenceUrl.reassignApprover,
                        data: data,
                        dataType: "text",
                        success: function (data) {
                            setTimeout(function () {
                                app.toast("Reassigned to new approver", "success");
                                location.reload();
                            }, 2000);
                        },
                        fail: function (jqXHR, textStatus, errorThrown) {
                            app.alertError("Reassignment failed..");
                        },
                        complete: function (data) {
                            $approverDropdown.option("value", "");
                            $approvalReassignModal.modal("hide");
                        }
                    });

                } else {
                    alert("Please select one approver to reassign to.");
                }
            }
        });
        
        $viewWorkflowBtn = $("#viewWorkflowBtn").dxButton({
            stylingMode: "contained",
            text: "Workflow",
            type: "normal",
            icon: "fa fa-cogs",
            onClick: function () {
                $viewWorkflowModal.modal("show");
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
            displayExpr: "name",
            keyExpr: "id",
            items: [
                { id: 1, name: "Excel Workbook (*.xlsx)", icon: "fa fa-file-excel-o" },
                { id: 2, name: "PDF", icon: "fa fa-file-pdf-o" }
            ],
            onItemClick: function (e) {
                app.toast("Generating...");

                var data = {
                    formId: app.getUrlId(),
                    isExportAsExcel: (e.itemData.id == 1)
                };

                $.ajax({
                    type: "POST",
                    url: referenceUrl.printRequest,
                    data: data,
                    dataType: "text",
                    success: function (data) {
                        var url = referenceUrl.printResponse + data;
                        window.location = url;
                    },
                    fail: function (jqXHR, textStatus, errorThrown) {
                        app.alertError(textStatus + ": " + errorThrown);
                    },
                    complete: function (data) {

                    }
                });

                e.event.preventDefault();
            }
        }).dxDropDownButton("instance");
        
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

        $editDraftBtn.dxButton({
            onClick: function (e) {
                console.log("clicked me!");
                window.location = referenceUrl.editForm + app.getUrlId();
                e.event.preventDefault();
            }
        });

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
                    width: 50
                },
                {
                    dataField: "dealer",
                    caption: "Dealer",
                    width: 110
                },
                {
                    dataField: "bank",
                    caption: "Bank",
                    width: 200
                },
                {
                    dataField: "tradeDate",
                    caption: "Trade Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    width: 120
                },
                {
                    dataField: "valueDate",
                    caption: "Value Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    width: 120
                },
                {
                    dataField: "maturityDate",
                    caption: "Maturity Date (T)",
                    dataType: "date",
                    format: "dd/MM/yyyy",
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
                    width: 130
                },
                {
                    dataField: "manualCalc_P_Plus_I",
                    caption: "Manual Calc P+I",
                    dataType: "boolean",
                    width: 80
                },
                {
                    dataField: "assetType",
                    caption: "Asset Type",
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
                    width: 140
                },
                {
                    dataField: "fcaAccount",
                    caption: "FCA",
                    width: 140
                }
            ],
            summary: {
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
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            allowColumnReordering: true,
            allowColumnResizing: true,
            wordWrapEnabled: true
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
                    width: 50
                },
                {
                    dataField: "dealer",
                    caption: "Dealer",
                    width: 110
                },
                {
                    dataField: "bank",
                    caption: "Bank",
                    width: 200
                },
                {
                    dataField: "tradeDate",
                    caption: "Trade Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    width: 120
                },
                {
                    dataField: "valueDate",
                    caption: "Value Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    width: 120
                },
                {
                    dataField: "maturityDate",
                    caption: "Maturity Date (T)",
                    dataType: "date",
                    format: "dd/MM/yyyy",
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
                    width: 130
                },
                {
                    dataField: "assetType",
                    caption: "Asset Type",
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
                    width: 140
                },
                {
                    dataField: "fcaAccount",
                    caption: "FCA",
                    width: 140
                }
            ],
            summary: {
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
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            allowColumnReordering: true,
            allowColumnResizing: true,
            wordWrapEnabled: true
        }).dxDataGrid("instance");

        $inflowMmiGrid = $("#inflowMmiGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    caption: "#",
                    cellTemplate: function (cellElement, cellInfo) {
                        cellElement.text(cellInfo.row.rowIndex + 1);
                    },
                    width: 50
                },
                {
                    dataField: "dealer",
                    caption: "Dealer",
                    width: 110
                },
                {
                    dataField: "issuer",
                    caption: "Issuer",
                    width: 200
                },
                {
                    dataField: "productType",
                    caption: "Product Type",
                    width: 60
                },
                {
                    dataField: "counterParty",
                    caption: "Counterparty",
                    width: 200
                },
                {
                    dataField: "tradeDate",
                    caption: "Trade Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    width: 120
                },
                {
                    dataField: "valueDate",
                    caption: "Value Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    width: 120
                },
                {
                    dataField: "maturityDate",
                    caption: "Maturity Date (T)",
                    dataType: "date",
                    format: "dd/MM/yyyy",
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
                    width: 125
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
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            allowColumnReordering: true,
            allowColumnResizing: true,
            wordWrapEnabled: true
        }).dxDataGrid("instance");

        $outflowMmiGrid = $("#outflowMmiGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    caption: "#",
                    cellTemplate: function (cellElement, cellInfo) {
                        cellElement.text(cellInfo.row.rowIndex + 1);
                    },
                    width: 50
                },
                {
                    dataField: "dealer",
                    caption: "Dealer",
                    width: 110
                },
                {
                    dataField: "issuer",
                    caption: "Issuer",
                    width: 200
                },
                {
                    dataField: "productType",
                    caption: "Product Type",
                    width: 60
                },
                {
                    dataField: "counterParty",
                    caption: "Counterparty",
                    width: 200
                },
                {
                    dataField: "tradeDate",
                    caption: "Trade Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    width: 120
                },
                {
                    dataField: "valueDate",
                    caption: "Value Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    width: 120
                },
                {
                    dataField: "maturityDate",
                    caption: "Maturity Date (T)",
                    dataType: "date",
                    format: "dd/MM/yyyy",
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
                    width: 125
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
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            allowColumnReordering: true,
            allowColumnResizing: true,
            wordWrapEnabled: true
        }).dxDataGrid("instance");

        $workflowGrid = $("#workflowGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.loadWorkflow
            }),
            columns: [
                {
                    dataField: "recordedDate",
                    caption: "Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy hh:mm a"
                },
                {
                    dataField: "requestBy",
                    caption: "Requested By"
                },
                {
                    dataField: "requestTo",
                    caption: "Requested To"
                },
                {
                    dataField: "workflowStatus",
                    caption: "Workflow Status"
                },
                {
                    dataField: "workflowNotes",
                    caption: "Notes"
                },
                {
                    dataField: "fcaAccount",
                    caption: "FCA"
                }
            ],
            showRowLines: true,
            rowAlternationEnabled: false,
            showBorders: true,
            sorting: {
                mode: "multiple",
                showSortIndexes: true
            },
            wordWrapEnabled: true
        }).dxDataGrid("instance");

        $auditTrailGrid = $("#auditTrailGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: referenceUrl.loadAuditTrail
            }),
            columns: [
                {
                    dataField: "actionType",
                    caption: "Action"
                },
                {
                    dataField: "formType",
                    caption: "Form Type",
                    visible: false
                },
                {
                    dataField: "formDate",
                    caption: "Form Date",
                    dataType: "date",
                    format: "dd/MM/yyyy",
                    visible: false
                },
                {
                    dataField: "modifiedOn",
                    caption: "Performed On",
                    dataType: "datetime",
                    format: "dd/MM/yyyy hh:mm:ss",
                    sortIndex: 0,
                    sortOrder: "desc"
                },
                {
                    dataField: "modifiedBy",
                    caption: "User ID"
                },
                {
                    dataField: "remarks",
                    caption: "Remarks"
                },
                {
                    dataField: "valueBefore",
                    caption: "Value Before"
                },
                {
                    dataField: "valueAfter",
                    caption: "Value After"
                }
            ],
            showRowLines: true,
            rowAlternationEnabled: false,
            showBorders: true,
            sorting: {
                mode: "multiple",
                showSortIndexes: true
            },
            wordWrapEnabled: true
        }).dxDataGrid("instance");

        // #endregion Data Grid

        //#region Events & Invocations

        $("#reassignBtn").dxButton({
            onClick: function (e) {
                $approvalReassignModal.modal("show");
                e.event.preventDefault();
            }
        });
        
        $("#viewAuditTrailBtn").on({
            "click": function (e) {
                $viewAuditTrailModal.modal("show");
                e.preventDefault();
            }
        });
        
        $("#submitForApprovalBtn").dxButton({
            onClick: function (e) {
                e.event.preventDefault();
                
            }
        });

        $("#approveBtn").on({
            "click": function (e) {
                $approvalNoteModal.modal("show");
                e.preventDefault();
            }
        });

        $("#rejectBtn").on({
            "click": function (e) {
                $rejectionNoteModal.modal("show");
                e.preventDefault();
            }
        });

        $("#approveFormBtn").on({
            "click": function (e) {
                postApproval(true);

                e.preventDefault();
            }
        });

        $("#rejectFormBtn").on({
            "click": function (e) {
                postApproval(false);

                e.preventDefault();
            }
        });




        setTimeout(function() {
                populateData();
            },
            1000);

        //#endregion
    });
}(window.jQuery, window, document));