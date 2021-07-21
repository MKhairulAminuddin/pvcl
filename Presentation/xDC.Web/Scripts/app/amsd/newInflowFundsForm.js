(function ($, window, document) {

    $(function () {
        var fundTypeStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: "../api/common/GetInflowFundsFundType"
        });

        var bankStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: "../api/common/GetInflowFundsBank"
        });

        var approverStore = DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: "../api/common/GetApproverAmsdInflowFunds"
        });
        

        var $inflowFundsGrid, $approverDropdown,$historyBtn, $submitBtn, $tbFormId, $tbFormStatus;

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
        
        $("#submitForApprovalBtn").on({
            "click": function (e) {
                if (jQuery.isEmptyObject($inflowFundsGrid.getDataSource().items())) {
                    $("#error_container").bs_warning("Please key in at least one item.");
                } else {
                    $('#selectApproverModal').modal('show');
                }
                
                e.preventDefault();
            }
        });

        $("#submitForApprovalModalBtn").on({
            "click": function (e) {
                if (jQuery.isEmptyObject($inflowFundsGrid.getDataSource().items())) {
                    $("#error_container").bs_warning("Please key in at least one item.");
                }
                else {
                    if (getUrlParameter('id') != false) {
                        data = {
                            id: getUrlParameter('id'),
                            amsdInflowFunds: $inflowFundsGrid.getDataSource().items(),
                            approver: $approverDropdown.option('value')
                        };
                    } else {
                        var data = {
                            amsdInflowFunds: $inflowFundsGrid.getDataSource().items(),
                            approver: $approverDropdown.option('value')
                        };
                    }
                    

                    $.ajax({
                        data: data,
                        dataType: 'json',
                        url: '../api/amsd/NewInflowFundsForm',
                        method: 'post'
                    }).done(function (data) {
                        window.location.href = "../amsd/InflowFundsFormStatus?id=" + data;

                    }).fail(function (jqXHR, textStatus, errorThrown) {
                        $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                    });
                }

                e.preventDefault();
            }
        });



        $("#saveAsDraftBtn").on({
            "click": function (e) {


                if (jQuery.isEmptyObject($inflowFundsGrid.getDataSource().items())) {
                    $("#error_container").bs_warning("Please key in at least one item.");
                }
                else {
                    var data;
                    if (getUrlParameter('id') != false) {
                        data = {
                            id: getUrlParameter('id'),
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
                        url: '../api/amsd/NewInflowFundsFormDraft',
                        method: 'post'
                    }).done(function (data) {
                        //window.location.href = "../amsd/InflowFundsFormStatus?id=" + data;
                        window.location.href = "../amsd";

                    }).fail(function (jqXHR, textStatus, errorThrown) {
                        $("#error_container").bs_alert(textStatus + ': ' + errorThrown);
                    });
                }

                e.preventDefault();
            }
        });

        if (getUrlParameter('id') != false) {
            $inflowFundsGrid = $("#inflowFundsGrid1").dxDataGrid({
                dataSource: DevExpress.data.AspNet.createStore({
                    key: "id",
                    loadUrl: "../api/amsd/GetInflowFunds?id=" + getUrlParameter('id'),
                    insertUrl: "../api/amsd/insertInflowFund",
                    updateUrl: "../api/amsd/updateInflowFund",
                    deleteUrl: "../api/amsd/deleteInflowFund"
                }),
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
                onRowInserting: function (e) {
                    e.data["formId"] = getUrlParameter('id');
                    
                    e.cancel = false;
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
        } else {
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
        }
        
        
    });
}(window.jQuery, window, document));