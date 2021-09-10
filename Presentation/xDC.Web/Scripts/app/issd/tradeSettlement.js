var tradeSettlement = (function () {
    var _tradeSettlement = {};

    var _getIdFromQueryString = window.location.pathname.split("/").pop();

    _tradeSettlement.myFunction = function () {
        return null;
    }

    _tradeSettlement.mvc = {
        printed: window.location.origin + "/issd/ViewPrinted/",
        print: window.location.origin + "/issd/Print"
    };

    _tradeSettlement.api = {
        loadWorkflowHistory: function(formTypeId) {
            return window.location.origin + "/api/common/GetWorkflow/" + formTypeId + "/" + _getIdFromQueryString;
        },
        loadApprover: window.location.origin + "/api/common/GetTradeSettlementApprover",
        loadCurrencies: window.location.origin + "/api/common/GetTradeSettlementCurrencies",
        loadTradeItemEdw: function(instrumentType, settlementDate, currency) {
            return window.location.origin +
                "/api/issd/GetTradeSettlementFromEdw/" +
                instrumentType +
                "/" +
                moment(settlementDate).unix() +
                "/" +
                currency;
        },
        loadOpeningBalanceEdw: function(settlementDate, currency) {
            return window.location.origin +
                "/api/issd/GetOpeningBalanceEdw/" +
                moment(settlementDate).unix() +
                "/" +
                currency;
        },
        loadTradeItem: function (instrumentType) {
            return window.location.origin + "/api/issd/TradeSettlement/TradeItem/" + _getIdFromQueryString + "/" + instrumentType;
        },
        loadOpeningBalance: function () {
            return window.location.origin + "/api/issd/GetBalance/" + _getIdFromQueryString;
        },

        loadApprovedTrades: function () {
            return window.location.origin + "/api/issd/ts/approvedTrades/" +
                app.getUrlParameter("settlementDateEpoch") +
                "/" +
                app.getUrlParameter("currency");
        },
        loadApprovedTradeItems: function (formId, instrumentType) {
            return window.location.origin +
                "/api/issd/TradeSettlement/TradeItem/" +
                formId +
                "/" +
                instrumentType;
        },
        loadTradeItemConsolidated: function (instrumentType) {
            return window.location.origin +
                "/api/issd/TradeSettlement/TradeItemConsolidated/" +
                instrumentType +
                "/" +
                app.getUrlParameter("settlementDateEpoch") +
                "/" +
                app.getUrlParameter("currency");
        },
        loadOpeningBalanceConsolidated: function () {
            return window.location.origin + "/api/issd/GetBalanceConsolidated/" +
                app.getUrlParameter("settlementDateEpoch") +
                "/" +
                app.getUrlParameter("currency");
        },
    };



    _tradeSettlement.dsTradeItemEdw = function(instrumentType, settlementDate, currency) {
        return $.ajax({
            url: _tradeSettlement.api.loadTradeItemEdw(instrumentType, settlementDate, currency),
            type: "get"
        });
    };

    _tradeSettlement.dsOpeningBalanceEdw = function (settlementDate, currency) {
        return $.ajax({
            url: _tradeSettlement.api.loadOpeningBalanceEdw(settlementDate, currency),
            type: "get"
        });
    };

    _tradeSettlement.dsTradeItem = function (instrumentType) {
        return $.ajax({
            url: _tradeSettlement.api.loadTradeItem(instrumentType),
            type: "get"
        });
    }

    _tradeSettlement.dsOpeningBalance = function () {
        return $.ajax({
            url: _tradeSettlement.api.loadOpeningBalance(),
            type: "get"
        });
    }

    _tradeSettlement.dsApprovedTradeItems = function (formId, instrumentType) {
        return DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: _tradeSettlement.api.loadApprovedTradeItems(formId, instrumentType)
        });
    }

    _tradeSettlement.dsTradeItemConsolidated = function (instrumentType) {
        return $.ajax({
            url: _tradeSettlement.api.loadTradeItemConsolidated(instrumentType),
            type: "get"
        });
    }

    _tradeSettlement.dsOpeningBalanceConsolidated = function () {
        return $.ajax({
            url: _tradeSettlement.api.loadOpeningBalanceConsolidated(),
            type: "get"
        });
    }

    _tradeSettlement.dsCurrency = function () {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: _tradeSettlement.api.loadCurrencies
            }),
            paginate: true,
            pageSize: 20
        };
    }

    _tradeSettlement.dsWorflowInformation = function (formTypeId) {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: _tradeSettlement.api.loadWorkflowHistory(formTypeId)
            }),
            paginate: true,
            pageSize: 20
        };
    }

    _tradeSettlement.submitApproverSelectBox = {
        dataSource:function() {
            return {
                store: DevExpress.data.AspNet.createStore({
                    key: "id",
                    loadUrl: _tradeSettlement.api.loadApprover
                }),
                paginate: true,
                pageSize: 20
            };
        },
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
    };

    _tradeSettlement.submitApprovalNotesTextArea = {
        height: 90
    };

    _tradeSettlement.settlementDateBox = {
        type: "date",
        displayFormat: "dd/MM/yyyy",
        value: new Date()
    };

    _tradeSettlement.settlementDateBoxReadOnly = {
        type: "date",
        displayFormat: "dd/MM/yyyy",
        readOnly: true
    };

    _tradeSettlement.currencySelectBox = {
        dataSource: _tradeSettlement.dsCurrency(),
        displayExpr: "value",
        valueExpr: "value",
        placeHolder: "Currency.."
    };

    _tradeSettlement.currencySelectBoxReadOnly = {
        dataSource: _tradeSettlement.dsCurrency(),
        displayExpr: "value",
        valueExpr: "value",
        placeHolder: "Currency..",
        readOnly: true
    };

    _tradeSettlement.toast = function(message, toastType) {
        var top = 0;
        var lastOffset = $(".dx-toast-content").last().offset();
        if (lastOffset != null) {
            top = lastOffset.top;
        }
        if (top <= 0)
            top = 30;
        else {
            top = window.innerHeight - top;
            top -= 20;
        }
        window.DevExpress.ui.notify({
            message: message,
            type: toastType,
            displayTime: 1000,
            height: "auto",
            width: "auto",
            closeOnClick: false,
            hoverStateEnabled: false,
            minWidth: 500,
            position: {
                my: "top right",
                at: "top right",
                of: "#toast-container",
                offset: "0 -" + top
            }
        });
    };

    _tradeSettlement.requestPrintForm = function (formType, formId, isExcel) {
        switch (formType) {
            case "TradeSettlement":
                return $.ajax({
                    type: "POST",
                    url: _tradeSettlement.mvc.print,
                    data: {
                        id: formId,
                        isExportAsExcel: isExcel
                    },
                    dataType: "text",
                    success: function (data) {
                        var url = _tradeSettlement.mvc.printed + data;
                        window.location = url;
                    },
                    fail: function (jqXHR, textStatus, errorThrown) {
                        app.alertError(textStatus + ": " + errorThrown);
                    },
                    complete: function (data) {

                    }
                });

            default:
                return console.log("Print Failed! Invalid Form");
        }
    }

    _tradeSettlement.printBtnWidgetSettingConsolidated = {
        text: "Print",
        icon: "print",
        type: "normal",
        stylingMode: "contained",
        dropDownOptions: {
            width: 230
        },
        items: [
            "Excel Workbook",
            "PDF"
        ],
        onItemClick: function (e) {
            if (e.itemData == "Excel Workbook") {
                var request = {
                    settlementDate: app.getUrlParameter("settlementDateEpoch"),
                    currency: app.getUrlParameter("currency"),
                    isExportAsExcel: true
                };

                $.ajax({
                    type: "POST",
                    url: "/issd/PrintConsolidated",
                    data: request,
                    dataType: "text",
                    success: function (data) {
                        var url = "/issd/ViewPrinted/" + data;
                        window.location = url;
                    },
                    fail: function (jqXHR, textStatus, errorThrown) {
                        app.alertError(textStatus + ": " + errorThrown);
                    },
                    complete: function (data) {

                    }
                });
                e.event.preventDefault();
            } else {
                var request = {
                    settlementDate: app.getUrlParameter("settlementDateEpoch"),
                    currency: app.getUrlParameter("currency"),
                    isExportAsExcel: false
                };

                $.ajax({
                    type: "POST",
                    url: "/issd/PrintConsolidated",
                    data: request,
                    dataType: "text",
                    success: function (data) {
                        var url = "/issd/ViewPrinted/" + data;
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
        }
    };

    _tradeSettlement.printBtnWidgetSetting = {
        text: "Print",
        icon: "print",
        type: "normal",
        stylingMode: "contained",
        dropDownOptions: {
            width: 230
        },
        items: [
            "Excel Workbook",
            "PDF"
        ],
        onItemClick: function(e) {
            if (e.itemData == "Excel Workbook") {
                _tradeSettlement.requestPrintForm("TradeSettlement", _getIdFromQueryString, true);
                e.event.preventDefault();
            } else {
                _tradeSettlement.requestPrintForm("TradeSettlement", _getIdFromQueryString, false);
                e.event.preventDefault();
            }
        }
    };
    
    _tradeSettlement.setSideMenuItemActive = function(url) {
        $("ul.treeview-menu a[href='" + url + "']").parentsUntil(".sidebar-menu > .treeview-menu")
            .addClass('active');
    };

    _tradeSettlement.tabBadgeItemCount = function (countTagId, gridInstance) {
        if (gridInstance != undefined) {
            var count = gridInstance.getDataSource().items().length;

            if (count > 0) {
                $("#" + countTagId).addClass("label label-danger").css("margin-left", "4px").text(count);
            } else {
                $("#" + countTagId).removeClass("label label-danger").css("margin-left", "0").text("");
            }
        } else {
            $("#" + countTagId).removeClass("label label-danger").css("margin-left", "0").text("");
        }
        
    };

    _tradeSettlement.defineTabBadgeNumbers = function(obj) {
        setTimeout(function() {
                obj.forEach((item) => {
                    _tradeSettlement.tabBadgeItemCount(item.titleId, item.dxDataGrid);
                });
            },
            1000);
    };

    _tradeSettlement.saveAllGrids = function() {
        for (var i = 0; i < arguments.length; i++) {
            arguments[i].saveEditData();
        }
    };

    _tradeSettlement.val_isTMinus1 = function(settlementDate) {
        if (moment().subtract(1, "days").isAfter(settlementDate)) {
            return true;
        } else {
            return false;
        }
    }

    _tradeSettlement.getIdFromQueryString = _getIdFromQueryString;



    return _tradeSettlement;
}());