$(function () {
    var url = window.location;
    // for sidebar menu entirely but not cover treeview
    $('ul.sidebar-menu a').filter(function () {
        return this.href != url;
    }).parent().removeClass('active');

    // for sidebar menu entirely but not cover treeview
    $('ul.sidebar-menu a').filter(function () {
        return this.href == url;
    }).parent().addClass('active');
    
    // for treeview
    $('ul.treeview-menu a').filter(function () {
        return this.href == url;
    }).parentsUntil(".sidebar-menu > .treeview-menu").addClass('active');




    
});


// alert extension

(function ($) {
    $.fn.extend({
        bs_alert: function (message, title) {
            var cls = 'alert-danger';
            var html = '<div class="alert ' + cls + ' alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>';
            if (typeof title === 'undefined' || title === '') {
                title = "<i class='fa fa-ban' aria-hidden='true'></i>";
            }
            html += title + ' <span>' + message + '</span></div>';
            $(this).html(html);
        },
        bs_warning: function (message, title) {
            var cls = 'alert-warning';
            var html = '<div class="alert ' + cls + ' alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>';
            if (typeof title === 'undefined' || title === '') {
                title = "<i class='fa fa-exclamation-triangle' aria-hidden='true'></i>";
            }
            html += title + ' <span>' + message + '</span></div>';
            $(this).html(html);
        },
        bs_info: function (message, title) {
            var cls = 'alert-info';
            var html = '<div class="alert ' + cls + ' alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>';
            if (typeof title === 'undefined' || title === '') {
                title = "<i class='fa fa-info-circle' aria-hidden='true'></i>";
            }
            html += title + ' <span>' + message + '</span></div>';
            $(this).html(html);
        },
        bs_success: function (message, title) {
            var cls = 'alert-success';
            var html = '<div class="alert ' + cls + ' alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>';
            if (typeof title === 'undefined' || title === '') {
                title = "<i class='fa fa-check-circle-o' aria-hidden='true'></i>";
            }
            html += title + ' <span>' + message + '</span></div>';
            $(this).html(html);
        },
        bs_custom: function (alertClass, alertIcon, message, title) {
            var cls = alertClass;
            var html = '<div class="alert ' + cls + ' alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>';
            if (typeof title === 'undefined' || title === '') {
                title = "<i class='" + alertIcon + "' aria-hidden='true'></i>";
            }
            html += title + ' <span>' + message + '</span></div>';
            $(this).html(html);
        },
        bs_close: function () {
            $('.alert').alert('close');
        }
    });
})(jQuery);


var getUrlParameter = function getUrlParameter(sParam) {
    var sPageURL = window.location.search.substring(1),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            return typeof sParameterName[1] === undefined ? true : decodeURIComponent(sParameterName[1]);
        }
    }
    return false;
};


var toast = function(message, toastType) {
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

(function ($) {
    var KashflowPlugin = function (element) {
        var elem = $(element);
        var obj = this;

        this.getIdFromQueryString = window.location.pathname.split("/").pop();

        this.commonUrl = {
            issdPrinted: window.location.origin + "/issd/ViewPrinted/",
            issdPrint: window.location.origin + "/issd/Print"
        }

        this.requestPrintForm = function (formType, formId, isExcel) {
            var thisScope = this;

            switch (formType) {
                case "TradeSettlement":
                        return $.ajax({
                            type: "POST",
                            url: thisScope.commonUrl.issdPrint,
                            data: {
                                id: formId,
                                isExportAsExcel: isExcel
                            },
                            dataType: "text",
                            success: function (data) {
                                var url = thisScope.commonUrl.issdPrinted + data;
                                window.location = url;
                            },
                            fail: function (jqXHR, textStatus, errorThrown) {
                                $("#error_container").bs_alert(textStatus + ": " + errorThrown);
                            },
                            complete: function (data) {

                            }
                        });

                default:
                    return console.log("Print Failed! Invalid Form");
            }
        }

        this.printBtnWidgetSetting = function () {
            var thisScope = this;
            
            var obj = {
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
                        thisScope.requestPrintForm("TradeSettlement", window.location.pathname.split("/").pop(), true);
                        e.event.preventDefault();
                    } else {
                        thisScope.requestPrintForm("TradeSettlement", window.location.pathname.split("/").pop(), false);
                        e.event.preventDefault();
                    }
                }
            };

            return obj;
        }
        
        this.tabBadgeItemCount = function(countTagId, gridInstance) {
            var count = gridInstance.getDataSource().items().length;

            if (count > 0) {
                $("#" + countTagId).addClass("label label-danger").css("margin-left", "4px").text(count);
            } else {
                $("#" + countTagId).removeClass("label label-danger").css("margin-left", "0").text("");
            }
        }

        this.defineTabBadgeNumbers = function (obj) {
            var thisScope = this;

            setTimeout(function () {
                obj.forEach((item) => {
                    thisScope.tabBadgeItemCount(item.titleId, item.dxDataGrid);
                });
            }, 1000);
        }
        
        this.setSideMenuItemActive = function(url) {
            $("ul.treeview-menu a[href='" + url + "']").parentsUntil(".sidebar-menu > .treeview-menu")
                .addClass('active');
        }

        this.toast = function (message, toastType) {
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
        }
    };



    $.fn.kashflowPlugin = function () {
        return this.each(function () {
            var element = $(this);
            if (element.data('kashflowPlugin')) return;
            var kashflowPlugin = new KashflowPlugin(this);
            element.data('kashflowPlugin', kashflowPlugin);
        });
    };
})(jQuery);