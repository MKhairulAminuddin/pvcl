(function ($, window, document) {

    $(function () {
        //#region Variable Definition
        
        ts.setSideMenuItemActive("/issd/TradeSettlement");
        
        var $tabpanel,
            $equityGrid,
            
            $approverDropdown,
            $approvalNotes,

            $settlementDateBox,
            $currencySelectBox,
            $edwAvailable,

            $saveAsDraftBtn,
            $submitForApprovalBtn,

            $selectApproverModal = $('#selectApproverModal'),
            $submitForApprovalModalBtn,

            $tradeSettlementForm,
            isSaveAsDraft,
            formTypeId = 3;

        var referenceUrl = {
            postNewFormRequest: window.location.origin + "/api/issd/TradeSettlement/New",
            postNewFormResponse: window.location.origin + "/issd/TradeSettlement/PartA/View/",
            dsEdwAvailability: window.location.origin + "/api/issd/ts/EdwAvailability/a"
        };
        
        //#endregion

        //#region Data Source & Functions
        
        function populateDwData(categoryType, settlementDate, currency) {
            if (categoryType == "equity") {
                $.when(
                        ts.dsTradeItemEdw("EQUITY", settlementDate, currency)
                    )
                    .done(function (data1) {
                        $equityGrid.option("dataSource", []);
                        $equityGrid.option("dataSource", data1.data);
                        $equityGrid.repaint();

                        app.toastEdwCount(data1.data, "EQUITY");
                    })
                    .always(function (dataOrjqXHR, textStatus, jqXHRorErrorThrown) {

                    })
                    .then(function () {

                    });
            }

            ts.defineTabBadgeNumbers([
                { titleId: "titleBadge1", dxDataGrid: $equityGrid }
            ]);
        }

        var dsEdwAvailability = function (tradeDateEpoch, currency) {
            return $.ajax({
                url: referenceUrl.dsEdwAvailability + "/" + moment(tradeDateEpoch).unix() + "/" + currency,
                type: "get"
            });
        };

        var checkDwDataAvailability = function (settlementDate, currency) {
            app.clearAllGrid($equityGrid);
            ts.defineTabBadgeNumbers([
                { titleId: "titleBadge1", dxDataGrid: $equityGrid }
            ]);

            if (settlementDate && currency) {
                $.when(
                        dsEdwAvailability(settlementDate, currency)
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

        function postData(isDraft) {
            if (isDraft) {
                app.toast("Saving....", "info", 3000);
            } else {
                app.toast("Submitting for approval....", "info", 3000);
            }
            
            var data = {
                currency: $currencySelectBox.option("value"),
                settlementDateEpoch: moment($settlementDateBox.option("value")).unix(),
                formType: formTypeId,
                isSaveAsDraft: isDraft,
                
                equity: $equityGrid.getDataSource().items(),

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
                    $selectApproverModal.modal("hide");
                }
            });
        }

        //#endregion
        
        //#region Other Widgets

        $settlementDateBox = $("#settlementDateBox").dxDateBox(ts.settlementDateBox).dxValidator({
            validationRules: [
                {
                    type: "required",
                    message: "Settlement Date is required"
                }
            ]
        }).dxDateBox("instance");

        $currencySelectBox = $("#currencySelectBox").dxSelectBox(ts.currencySelectBox)
            .dxValidator({
                validationRules: [
                    {
                        type: "required",
                        message: "Currency is required"
                    }
                ]
            })
            .dxSelectBox("instance");

        $edwAvailable = $("#edwAvailable").dxList({
            activeStateEnabled: false,
            focusStateEnabled: false,
            itemTemplate: function (data, index) {
                var result = $("<div>");

                $("<div>").text(data.name + " × " + data.numbers).appendTo(result);
                $("<a>").append("<i class='fa fa-download'></i> Populate").on("dxclick", function (e) {

                    populateDwData(data.categoryType, $settlementDateBox.option("value"), $currencySelectBox.option("value"));

                    e.stopPropagation();
                }).appendTo(result);

                return result;
            }
        }).dxList("instance");

        $tabpanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { titleId: "titleBadge1", title: "Equity", template: "equityTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });

        $approverDropdown = $("#approverDropdown").dxSelectBox(ts.submitApproverSelectBox).dxSelectBox("instance");

        $approvalNotes = $("#approvalNotes").dxTextArea(ts.submitApprovalNotesTextArea).dxTextArea("instance");
        
        //#endregion
        
        // #region Data Grid
        
        $equityGrid = $("#equityGrid").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    caption: "#",
                    cellTemplate: function (cellElement, cellInfo) {
                        cellElement.text(cellInfo.row.rowIndex + 1 );
                    },
                    allowEditing: false,
                    width: "30px"
                },
                {
                    dataField: "instrumentCode",
                    caption: "Equity"
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN"
                },
                {
                    dataField: "maturity",
                    caption: "Maturity (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "sales",
                    caption: "Sales (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "purchase",
                    caption: "Purchase (-)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
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
                allowDeleting: true,
                allowAdding: true
            },
            showBorders: true,
            showRowLines: true,
            showColumnLines: true,
            paging: {
                enabled: false
            }
        }).dxDataGrid("instance");
        
        // #endregion Data Grid
 
        //#region Events

        $settlementDateBox.on("valueChanged", function (data) {
            checkDwDataAvailability(data.value, $currencySelectBox.option("value"));
        });

        $currencySelectBox.on("valueChanged", function (data) {
            checkDwDataAvailability($settlementDateBox.option("value"), data.value);
        });


        $saveAsDraftBtn = $("#saveAsDraftBtn").on({
            "click": function (e) {
                isSaveAsDraft = true;
            }
        });

        $submitForApprovalBtn = $("#submitForApprovalBtn").on({
            "click": function (e) {
                isSaveAsDraft = false;
            }
        });

        $tradeSettlementForm = $("#tradeSettlementForm").on("submit",
            function (e) {
                ts.saveAllGrids($equityGrid);

                if (isSaveAsDraft) {
                    setTimeout(function () {
                        postData(true);
                    }, 1000);
                }
                else {
                    $selectApproverModal.modal('show');
                }
                
                e.preventDefault();
            });

        $submitForApprovalModalBtn = $("#submitForApprovalModalBtn").on({
            "click": function (e) {
                ts.saveAllGrids($equityGrid);

                if ($approverDropdown.option("value") != null) {

                    app.toast("Submitting for approval....", "info", 3000);
                    setTimeout(function() {
                            postData(false);
                        },
                        1000);
                } else {
                    alert("Please select an approver");
                }
                
                e.preventDefault();
            }
        });
        
        //#endregion
    });
}(window.jQuery, window, document));