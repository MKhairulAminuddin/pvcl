(function ($, window, document) {

    $(function () {
        //#region Variable Definition
        
        ts.setSideMenuItemActive("/issd/TradeSettlement");
        
        var $tabpanel,
            $repoGrid,
            
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
            formTypeId = 5;

        var referenceUrl = {
            postNewFormRequest: window.location.origin + "/api/issd/TradeSettlement/New",
            postNewFormResponse: window.location.origin + "/issd/TradeSettlement/PartC/View/",
            dsEdwAvailability: window.location.origin + "/api/issd/ts/EdwAvailability/c"
        };
        
        //#endregion

        //#region Data Source & Functions

        var populateDwData = function (categoryType, settlementDate, currency) {
            if (categoryType == "repo") {
                $.when(
                    ts.dsTradeItemEdw("REPO", settlementDate, currency)
                )
                    .done(function (data1) {
                        $repoGrid.option("dataSource", []);
                        $repoGrid.option("dataSource", data1.data);
                        $repoGrid.repaint();

                        app.toastEdwCount(data1.data, "REPO");
                    })
                    .always(function (dataOrjqXHR, textStatus, jqXHRorErrorThrown) {

                    })
                    .then(function () {

                    });
            }

            ts.defineTabBadgeNumbers([
                { titleId: "titleBadge5", dxDataGrid: $repoGrid }
            ]);
        }

        var dsEdwAvailability = function (tradeDateEpoch, currency) {
            return $.ajax({
                url: referenceUrl.dsEdwAvailability + "/" + moment(tradeDateEpoch).unix() + "/" + currency,
                type: "get"
            });
        };

        var checkDwDataAvailability = function (settlementDate, currency) {
            app.clearAllGrid($repoGrid);
            ts.defineTabBadgeNumbers([
                { titleId: "titleBadge5", dxDataGrid: $repoGrid }
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
            var data = {
                currency: $currencySelectBox.option("value"),
                settlementDateEpoch: moment($settlementDateBox.option("value")).unix(),
                formType: formTypeId,
                isSaveAsDraft: isDraft,
                
                repo: $repoGrid.getDataSource().items(),

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
                { titleId: "titleBadge5", title: "REPO", template: "repoTab" }
            ],
            deferRendering: false,
            itemTitleTemplate: $("#dxPanelTitle"),
            showNavButtons: true
        });

        $approverDropdown = $("#approverDropdown").dxSelectBox(ts.submitApproverSelectBox).dxSelectBox("instance");

        $approvalNotes = $("#approvalNotes").dxTextArea(ts.submitApprovalNotesTextArea).dxTextArea("instance");
        
        //#endregion
        
        // #region Data Grid

        $repoGrid = $("#repoGrid").dxDataGrid({
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
                    dataField: "instrumentCode",
                    caption: "REPO"
                },
                {
                    dataField: "stockCode",
                    caption: "Stock Code/ ISIN"
                },
                {
                    dataField: "firstLeg",
                    caption: "1st Leg (+)",
                    dataType: "number",
                    format: {
                        type: "fixedPoint",
                        precision: 2
                    }
                },
                {
                    dataField: "secondLeg",
                    caption: "2nd Leg (-)",
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
                        column: "firstLeg",
                        summaryType: "sum",
                        displayFormat: "{0}",
                        valueFormat: {
                            type: "fixedPoint",
                            precision: 2
                        }
                    },
                    {
                        column: "secondLeg",
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
                ts.saveAllGrids($repoGrid);

                if (ts.val_isTMinus1($settlementDateBox.option("value"))) {
                    alert("T-n only available for viewing..");
                }
                else {
                    if (isSaveAsDraft) {
                        // new clean draft
                        setTimeout(function () {
                            postData(true);
                        }, 1000);
                    }
                    else {
                        $selectApproverModal.modal('show');
                    }
                }

                e.preventDefault();
            });

        $submitForApprovalModalBtn = $("#submitForApprovalModalBtn").on({
            "click": function (e) {
                ts.saveAllGrids($repoGrid);

                if ($approverDropdown.option("value") != null) {

                    app.toast("Submitting for approval....", "info", 3000);

                    setTimeout(function () {
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