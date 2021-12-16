(function ($, window, document) {

    $(function () {
        //#region Variable Definition

        var $inflowTabpanel,
            $inflowDepositGrid,
            $inflowMmiGrid,

            $printBtn,


            $outflowTabpanel,
            $outflowDepositGrid,
            $outflowMmiGrid,

            $currencySelectBox,
            $tradeDate,

            $viewWorkflowBtn,
            $workflowGrid,
            $auditTrailGrid,
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
            

            postNewFormRequest: window.location.origin + "/api/fid/Treasury/New",
            postNewFormResponse: window.location.origin + "/fid/Treasury",

            printRequest: window.location.origin + "/fid/Print",
            printResponse: window.location.origin + "/fid/Printed/"
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
                        url: window.location.origin + "/api/common/reassignApprover",
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
                    id: app.getUrlId(),
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



        //#endregion

        // #region Data Grid

        $inflowDepositGrid = $("#inflowDepositGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "dealer",
                    caption: "Dealer"
                },
                {
                    dataField: "bank",
                    caption: "Bank"
                },
                {
                    dataField: "valueDate",
                    caption: "Value Date",
                    dataType: "date",
                    format: "dd/MM/yyyy"
                },
                {
                    dataField: "maturityDate",
                    caption: "Maturity Date (T)",
                    dataType: "date",
                    format: "dd/MM/yyyy"
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
                    }
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
                    calculateDisplayValue: function (rowData) {
                        if (rowData.intProfitReceivable >= 0) {
                            return rowData.intProfitReceivable.toFixed(2.5);
                        } else {
                            return rowData.intProfitReceivable;
                        }
                    }
                },
                {
                    dataField: "principalIntProfitReceivable",
                    caption: "Principal + Interest/Profit Receivable",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    calculateDisplayValue: function (rowData) {
                        if (rowData.principalIntProfitReceivable >= 0) {
                            return rowData.principalIntProfitReceivable.toFixed(2.5);

                        } else {
                            return rowData.principalIntProfitReceivable;
                        }
                    }
                },
                {
                    dataField: "assetType",
                    caption: "Asset Type",
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
                    caption: "Notes"
                },
                {
                    dataField: "fcaAccount",
                    caption: "FCA"
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
            wordWrapEnabled: true
        }).dxDataGrid("instance");

        $outflowDepositGrid = $("#outflowDepositGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "dealer",
                    caption: "Dealer"
                },
                {
                    dataField: "bank",
                    caption: "Bank"
                },
                {
                    dataField: "valueDate",
                    caption: "Value Date",
                    dataType: "date",
                    format: "dd/MM/yyyy"
                },
                {
                    dataField: "maturityDate",
                    caption: "Maturity Date (T)",
                    dataType: "date",
                    format: "dd/MM/yyyy"
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
                    }
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
                    calculateDisplayValue: function (rowData) {
                        if (rowData.intProfitReceivable >= 0) {
                            return rowData.intProfitReceivable.toFixed(2.5);
                        } else {
                            return rowData.intProfitReceivable;
                        }
                    }
                },
                {
                    dataField: "principalIntProfitReceivable",
                    caption: "Principal + Interest/Profit Receivable",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    },
                    calculateDisplayValue: function (rowData) {
                        if (rowData.principalIntProfitReceivable >= 0) {
                            return rowData.principalIntProfitReceivable.toFixed(2.5);

                        } else {
                            return rowData.principalIntProfitReceivable;
                        }
                    },
                },
                {
                    dataField: "assetType",
                    caption: "Asset Type"
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
                    caption: "Notes"
                },
                {
                    dataField: "fcaAccount",
                    caption: "FCA"
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
            wordWrapEnabled: true
        }).dxDataGrid("instance");

        $inflowMmiGrid = $("#inflowMmiGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "dealer",
                    caption: "Dealer"
                },
                {
                    dataField: "issuer",
                    caption: "Issuer"
                },
                {
                    dataField: "productType",
                    caption: "Product Type"
                },
                {
                    dataField: "counterParty",
                    caption: "Counterparty"
                },
                {
                    dataField: "valueDate",
                    caption: "Value Date",
                    dataType: "date",
                    format: "dd/MM/yyyy"
                },
                {
                    dataField: "maturityDate",
                    caption: "Maturity Date (T)",
                    dataType: "date",
                    format: "dd/MM/yyyy"
                },
                {
                    dataField: "holdingDayTenor",
                    caption: "Holding Period / Tenor (days)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 0
                    }
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
                    allowEditing: false
                },
                {
                    dataField: "proceeds",
                    caption: "Proceeds",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "certNoStockCode",
                    caption: "Certificate No. / Stock Code"
                },
                {
                    dataField: "fcaAccount",
                    caption: "FCA"
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
            editing: {
                mode: "batch",
                allowUpdating: false,
                allowDeleting: false,
                allowAdding: false
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
                    caption: "Dealer"
                },
                {
                    dataField: "issuer",
                    caption: "Issuer"
                },
                {
                    dataField: "productType",
                    caption: "Product Type"
                },
                {
                    dataField: "counterParty",
                    caption: "Counterparty"
                },
                {
                    dataField: "valueDate",
                    caption: "Value Date",
                    dataType: "date",
                    format: "dd/MM/yyyy"
                },
                {
                    dataField: "maturityDate",
                    caption: "Maturity Date (T)",
                    dataType: "date",
                    format: "dd/MM/yyyy"
                },
                {
                    dataField: "holdingDayTenor",
                    caption: "Holding Period / Tenor (days)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 0
                    }
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
                    dataType: "number"
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
                },
                {
                    dataField: "proceeds",
                    caption: "Proceeds",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "certNoStockCode",
                    caption: "Certificate No. / Stock Code"
                },
                {
                    dataField: "fcaAccount",
                    caption: "FCA"
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
            editing: {
                mode: "batch",
                allowUpdating: false,
                allowDeleting: false,
                allowAdding: false
            },
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
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