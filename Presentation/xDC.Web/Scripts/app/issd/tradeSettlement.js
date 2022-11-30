var ts = (function () {
    var _ts = {};

    var _getIdFromQueryString = window.location.pathname.split("/").pop();

    _ts.myFunction = function () {
        return null;
    }

    _ts.mvc = {
        printed: window.location.origin + "/issd/TradeSettlement/Download/",
        print: window.location.origin + "/issd/Print"
    };

    _ts.api = {
        loadWorkflowHistory: function(formTypeId) {
            return window.location.origin + "/api/common/GetWorkflow/" + formTypeId + "/" + _getIdFromQueryString;
        },
        loadAuditTrail: function (formTypeId) {
            return window.location.origin + "/api/common/FormAuditTrail/" + formTypeId +"/" + _getIdFromQueryString;
        },
        loadApprover: window.location.origin + "/api/common/GetTradeSettlementApprover",
        reassignApprover: window.location.origin + "/api/common/reassignApprover",
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



    _ts.dsTradeItemEdw = function(instrumentType, settlementDate, currency) {
        return $.ajax({
            url: _ts.api.loadTradeItemEdw(instrumentType, settlementDate, currency),
            type: "get"
        });
    };

    _ts.dsOpeningBalanceEdw = function (settlementDate, currency) {
        return $.ajax({
            url: _ts.api.loadOpeningBalanceEdw(settlementDate, currency),
            type: "get"
        });
    };

    _ts.dsTradeItem = function (instrumentType) {
        return $.ajax({
            url: _ts.api.loadTradeItem(instrumentType),
            type: "get"
        });
    }

    _ts.dsOpeningBalance = function () {
        return $.ajax({
            url: _ts.api.loadOpeningBalance(),
            type: "get"
        });
    }

    _ts.dsApprovedTradeItems = function (formId, instrumentType) {
        return DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: _ts.api.loadApprovedTradeItems(formId, instrumentType)
        });
    }

    _ts.dsTradeItemConsolidated = function (instrumentType) {
        return $.ajax({
            url: _ts.api.loadTradeItemConsolidated(instrumentType),
            type: "get"
        });
    }

    _ts.dsOpeningBalanceConsolidated = function () {
        return $.ajax({
            url: _ts.api.loadOpeningBalanceConsolidated(),
            type: "get"
        });
    }

    _ts.dsCurrency = function () {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: _ts.api.loadCurrencies
            }),
            paginate: true,
            pageSize: 20
        };
    }

    _ts.dsWorflowInformation = function (formTypeId) {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: _ts.api.loadWorkflowHistory(formTypeId)
            }),
            paginate: true,
            pageSize: 20
        };
    }

    _ts.dsAuditTrail = function (formTypeId) {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: _ts.api.loadAuditTrail(formTypeId)
            }),
            paginate: true,
            pageSize: 20
        };
    }

    _ts.dsApproverList = function () {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: _ts.api.loadApprover
            }),
            paginate: true,
            pageSize: 20
        };
    }

    _ts.submitApproverSelectBox = {
        dataSource: _ts.dsApproverList(),
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

    _ts.submitApprovalNotesTextArea = {
        height: 90
    };

    _ts.settlementDateBox = {
        type: "date",
        displayFormat: "dd/MM/yyyy",
        value: new Date()
    };

    _ts.settlementDateBoxReadOnly = {
        type: "date",
        displayFormat: "dd/MM/yyyy",
        readOnly: true
    };

    _ts.currencySelectBox = {
        dataSource: _ts.dsCurrency(),
        displayExpr: "value",
        valueExpr: "value",
        placeHolder: "Currency.."
    };

    _ts.currencySelectBoxReadOnly = {
        dataSource: _ts.dsCurrency(),
        displayExpr: "value",
        valueExpr: "value",
        placeHolder: "Currency..",
        readOnly: true
    };

    _ts.toast = function(message, toastType) {
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

    _ts.requestPrintForm = function (formType, formId, isExcel) {
        switch (formType) {
            case "TradeSettlement":
                return $.ajax({
                    type: "POST",
                    url: _ts.mvc.print,
                    data: {
                        id: formId,
                        isExportAsExcel: isExcel
                    },
                    dataType: "text",
                    success: function (data) {
                        var url = _ts.mvc.printed + data;
                        window.location = url;
                    },
                    fail: function (jqXHR, textStatus, errorThrown) {
                        app.toast(textStatus + ": " + errorThrown, "danger");
                    },
                    complete: function (data) {

                    }
                });

            default:
                return console.log("Print Failed! Invalid Form");
        }
    }

    _ts.printBtnWidgetSettingConsolidated = {
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
                        var url = "/issd/TradeSettlement/Download/" + data;
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
                        var url = "/issd/TradeSettlement/Download/" + data;
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

    _ts.printBtnWidgetSetting = {
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
                _ts.requestPrintForm("TradeSettlement", _getIdFromQueryString, true);
                e.event.preventDefault();
            } else {
                _ts.requestPrintForm("TradeSettlement", _getIdFromQueryString, false);
                e.event.preventDefault();
            }
        }
    };
    
    _ts.setSideMenuItemActive = function(url) {
        $("ul.treeview-menu a[href='" + url + "']").parentsUntil(".sidebar-menu > .treeview-menu")
            .addClass('active');
    };

    _ts.tabBadgeItemCount = function (countTagId, gridInstance) {
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

    _ts.defineTabBadgeNumbers = function(obj) {
        setTimeout(function() {
                obj.forEach((item) => {
                    _ts.tabBadgeItemCount(item.titleId, item.dxDataGrid);
                });
            },
            1000);
    };

    _ts.saveAllGrids = function() {
        for (var i = 0; i < arguments.length; i++) {
            arguments[i].saveEditData();
        }
    };

    _ts.val_isTMinus1 = function(settlementDate) {
        if (moment().subtract(1, "days").isAfter(settlementDate)) {
            return true;
        } else {
            return false;
        }
    }

    _ts.getIdFromQueryString = _getIdFromQueryString;
    
    return _ts;
}());