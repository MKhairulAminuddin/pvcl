(function ($, window, document) {

    $(function () {
        var fundTypeStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: window.location.origin + "/api/common/GetInflowFundsFundType"
        });

        var bankStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: window.location.origin + "/api/common/GetInflowFundsBank"
        });

        var approverStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: window.location.origin + "/api/common/GetApproverAmsdInflowFunds"
        });
        

        var $inflowFundsGrid, $approverDropdown, $historyBtn, $submitBtn, $tbFormId, $tbFormStatus, $approvalNotes;

        var referenceUrl = {

        };

        var postData = function() {
            var data = {
                amsdInflowFunds: $inflowFundsGrid.getDataSource().items(),
                approver: $approverDropdown.option('value'),
                approvalNotes: $approvalNotes.option("value")
            };

            $.ajax({
                data: data,
                dataType: 'json',
                url: window.location.origin + '/api/amsd/InflowFund/New',
                method: 'post',
                success: function (data) {
                    window.location.href = window.location.origin + "/amsd/InflowFund/View/" + data;
                },
                fail: function (jqXHR, textStatus, errorThrown) {
                    app.alertError(textStatus + ': ' + errorThrown);
                },
                complete: function (data) {

                }
            });
        }


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
        

        $("#submitForApprovalBtn").on({
            "click": function (e) {
                $inflowFundsGrid.saveEditData().then(function () {
                    
                    if (jQuery.isEmptyObject($inflowFundsGrid.getDataSource().items())) {
                        $("#error_container").bs_warning("Please key in at least one item.");
                    } else {
                        cutOffTimeChecker();
                        $('#selectApproverModal').modal('show');
                    }

                });
                
                e.preventDefault();
            }
        });

        $("#adminEditSaveChangesBtn").on({
            "click": function (e) {
                $inflowFundsGrid.saveEditData().then(function () {

                    if (jQuery.isEmptyObject($inflowFundsGrid.getDataSource().items())) {
                        $("#error_container").bs_warning("Please key in at least one item.");
                    } else {
                        $("#error_container").bs_success("Changes saved!");
                    }

                });

                e.preventDefault();
            }
        });
        

        $("#submitForApprovalModalBtn").on({
            "click": function (e) {

                postData();

                e.preventDefault();
            }
        });
        
        $("#saveAsDraftBtn").on({
            "click": function (e) {
                $inflowFundsGrid.saveEditData().then(function () {

                    if (jQuery.isEmptyObject($inflowFundsGrid.getDataSource().items())) {
                        $("#error_container").bs_warning("Please key in at least one item.");
                    }
                    else {
                        postData();
                    }

                });
                
                e.preventDefault();
            }
        });
        
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
            onRowUpdated: function (e) {
                console.log(e);
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
        
    });
}(window.jQuery, window, document));