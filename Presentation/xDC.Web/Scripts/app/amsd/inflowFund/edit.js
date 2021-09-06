(function ($, window, document) {

    $(function () {

        //#region Variable

        var $inflowFundsGrid,
            $approverDropdown,
            $approvalNotes,
            $historyBtn,
            $submitBtn,
            $tbFormStatus,
            $selectApproverModal;

        $selectApproverModal = $("#selectApproverModal");

        var referenceUrl = {
            loadGrid: window.location.origin + "/api/amsd/GetInflowFunds/" + app.getUrlId(),

            dsFundType: window.location.origin + "/api/common/GetInflowFundsFundType",
            dsBank: window.location.origin + "/api/common/GetInflowFundsBank",
            dsApprover: window.location.origin + "/api/common/GetApproverAmsdInflowFunds",

            formSubmit: window.location.origin + "/api/amsd/InflowFund/Edit/",
        };

        //#endregion 

        //#region  Data Source

        var fundTypeStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: referenceUrl.dsFundType
        });

        var bankStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: referenceUrl.dsBank
        });

        var approverStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: referenceUrl.dsApprover
        });

        var loadData = function() {
            $.ajax({
                dataType: 'json',
                url: referenceUrl.loadGrid,
                method: 'get'
            }).done(function (data) {
                $inflowFundsGrid.option("dataSource", data.data);
                $inflowFundsGrid.refresh();
                
            }).fail(function (jqXHR, textStatus, errorThrown) {
                app.alertError(textStatus + ': ' + errorThrown);
                
            });
        }

        var postData = function () {

            var data = {
                id: app.getUrlId(),
                amsdInflowFunds: $inflowFundsGrid.getDataSource().items(),
                approver: $approverDropdown.option("value"),
                approvalNotes: $approvalNotes.option("value")
            };

            $.ajax({
                data: data,
                dataType: 'json',
                url: window.location.origin + '/api/amsd/InflowFund/Edit/' + app.getUrlId(),
                method: 'post',
                success: function (data) {
                    window.location.href = window.location.origin + "/amsd/inflowfund/view/" + data;

                },
                fail: function (jqXHR, textStatus, errorThrown) {
                    app.alertError(textStatus + ': ' + errorThrown);

                },
                complete: function (data) {

                }
            });
        }

        function cutOffTimeChecker() {
            $.ajax({
                dataType: 'json',
                url: window.location.origin + '/api/amsd/IsViolatedCutOffTime',
                method: 'get',
                success: function (data) {
                    if (data) {
                        $("#cutOffTimeNotify").text("Cut Off Time Violated").addClass("label label-danger");
                    } else {
                        $("#cutOffTimeNotify").text("").removeClass("label label-danger");
                    }
                },
                fail: function (jqXHR, textStatus, errorThrown) {

                },
                complete: function (data) {

                }
            });
        }

        //#endregion 

        //#region Widgets

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
        
        //#region DataGrid

        $inflowFundsGrid = $("#inflowFundsGrid1").dxDataGrid({
            dataSource: [],
            columns: [
                {
                    dataField: "fundType",
                    caption: "Fund Types",
                    lookup: {
                        dataSource: fundTypeStore,
                        valueExpr: "value",
                        displayExpr: "value"
                    }
                },
                {
                    dataField: "bank",
                    caption: "Bank",
                    lookup: {
                        dataSource: bankStore,
                        valueExpr: "value",
                        displayExpr: "value"
                    }
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
            ],
            showBorders: true,
            height: 300,
            editing: {
                mode: "batch",
                allowUpdating: true,
                allowDeleting: true,
                allowAdding: true
            },
            summary: {
                totalItems: [
                    {
                        column: "type",
                        displayFormat: "TOTAL"
                    },
                    {
                        column: "amount",
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

        //#endregion


        $("#submitForApprovalBtn").dxButton("instance").option("onClick",
            function(e) {
                $inflowFundsGrid.saveEditData().then(function() {

                    if (jQuery.isEmptyObject($inflowFundsGrid.getDataSource().items())) {
                        app.alertWarning("Please key in at least one item.");
                    } else {
                        cutOffTimeChecker();
                        $selectApproverModal.modal('show');
                    }

                });

                e.event.preventDefault();
            }
        );
        
        $("#submitForApprovalModalBtn").dxButton("instance").option("onClick",
            function (e) {
                postData();
                e.event.preventDefault();
            }
        );

        $("#saveAsDraftBtn").on({
            "click": function (e) {
                $inflowFundsGrid.saveEditData().then(function () {

                    if (jQuery.isEmptyObject($inflowFundsGrid.getDataSource().items())) {
                        $("#error_container").bs_warning("Please key in at least one item.");
                    }
                    else {
                        $loadPanel.option("position", { of: "#formContainer" });
                        $loadPanel.show();

                        var data;
                        if (app.getUrlParameter('id') != false) {
                            data = {
                                id: app.getUrlParameter('id'),
                                amsdInflowFunds: $inflowFundsGrid.getDataSource().items()
                            };
                        } else {
                            data = {
                                amsdInflowFunds: $inflowFundsGrid.getDataSource().items()
                            };
                        }

                        $.ajax({
                            data: data,
                            dataType: 'json',
                            url: window.location.origin + '/api/amsd/NewInflowFundsFormDraft',
                            method: 'post',
                            success: function (data) {
                                window.location.href = window.location.origin + "/amsd";
                            },
                            fail: function (jqXHR, textStatus, errorThrown) {
                                app.alertError(textStatus + ': ' + errorThrown);
                            },
                            complete: function (data) {
                                $loadPanel.hide();
                            }
                        });
                    }

                });

                e.preventDefault();
            }
        });

        

        loadData();

    });
}(window.jQuery, window, document));