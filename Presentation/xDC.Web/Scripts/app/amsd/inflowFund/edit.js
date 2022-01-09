(function ($, window, document) {

    $(function () {

        //#region Variable

        var $inflowFundsGrid,
            $approverDropdown,
            $approvalNotes,
            $tbFormStatus,
            $inflowFundForm,
            isDraft = false,
            isAdminEdit = false;
        
        var referenceUrl = {
            loadGrid: window.location.origin + "/api/amsd/GetInflowFunds/" + app.getUrlId(),

            dsFundType: window.location.origin + "/api/common/GetInflowFundsFundType",
            dsBank: window.location.origin + "/api/common/GetInflowFundsBank",
            dsApprover: window.location.origin + "/api/common/GetApproverAmsdInflowFunds",

            formSubmit: window.location.origin + "/api/amsd/InflowFund/Edit/",
            redirectResponse: window.location.origin + "/amsd/InflowFund/view/",
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

        var postData = function (isDraft, isAdminEdit) {

            var data = {
                id: app.getUrlId(),
                isSaveAsDraft: isDraft,
                isSaveAdminEdit: isAdminEdit,

                amsdInflowFunds: $inflowFundsGrid.getDataSource().items(),
                approver: (isDraft) ? null : $approverDropdown.option("value"),
                approvalNotes: (isDraft) ? null : $approvalNotes.option("value")
            };

            $.ajax({
                data: data,
                dataType: 'json',
                url: referenceUrl.formSubmit + app.getUrlId(),
                method: 'post',
                success: function (data) {
                    window.location.href = referenceUrl.redirectResponse + data;
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
                    },
                    validationRules: [{ type: "required" }]
                },
                {
                    dataField: "bank",
                    caption: "Bank",
                    lookup: {
                        dataSource: bankStore,
                        valueExpr: "value",
                        displayExpr: "value"
                    },
                    validationRules: [{ type: "required" }]
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

        $("#saveAsDraftBtn").on({
            "click": function (e) {
                isDraft = true;
            }
        });

        $("#adminEditSaveChangesBtn").on({
            "click": function (e) {
                isAdminEdit = true;
            }
        });

        $inflowFundForm = $("#inflowFundForm").on("submit",
            function (e) {
                app.saveAllGrids($inflowFundsGrid);

                if (isDraft || isAdminEdit) {
                    setTimeout(function () {
                        postData(isDraft, isAdminEdit);
                    }, 1000);

                } else {
                    $('#selectApproverModal').modal('show');
                }

                e.preventDefault();
            });

        $("#submitForApprovalModalBtn").on({
            "click": function (e) {
                app.saveAllGrids($inflowFundsGrid);

                if ($approverDropdown.option("value") != null) {
                    app.toast("Submitting for approval....", "info", 3000);
                    setTimeout(function () { postData(false, false); },
                        1000);
                } else {
                    alert("Please select an approver");
                }

                e.preventDefault();
            }
        });

        //#region Immediate Invocation function

        loadData();

        //#endregion
        
    });
}(window.jQuery, window, document));