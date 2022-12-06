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

var app = (function() {
    var _app = {};

    _app.tsCouponTypes = ["GOV", "CORP"];
    _app.tsBondTypes = ["GOV", "CORP"];
    
    _app.toast = function (message, toastType = "info", delayTime = 3000) {
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
            displayTime: delayTime,
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

    _app.toastEdwCount = function(data, edwItemType) {
        var countItems = data.length;

        if (countItems > 0) {
            _app.toast(countItems + " " + edwItemType + " records fetched", "info");
        }
    }

    _app.getUrlParameter = function(sParam) {
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

    _app.getUrlId = function() {
        return window.location.pathname.split("/").pop();
    }

    _app.saveAllGrids = function () {
        for (var i = 0; i < arguments.length; i++) {
            arguments[i].saveEditData();
            arguments[i].refresh();
        }
    };

    _app.clearAllGrid = function() {
        for (var i = 0; i < arguments.length; i++) {
            arguments[i].option("dataSource", []);
        }
    }

    _app.clearUserPreference = function () {
        localStorage.removeItem("xDC_TS_Grid_02");
        localStorage.removeItem("xDC_TS_ApprovedGrid_02");
        localStorage.removeItem("xDC_IF_Grid");
        localStorage.removeItem("xDC_Treasury_Grid");
        localStorage.removeItem("xDC_T10_Grid");
        localStorage.removeItem("xDC_Admin_User");
        
        _app.toast("User Preferences cleared. Refresh the page to see the effect.", "info", 2000);
    }
    
    _app.alertError = function (message, title) {
        var alertTag = $("#error_container");
        var cls = 'alert-danger';
        var html = '<div class="alert ' + cls + ' alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>';
        if (typeof title === 'undefined' || title === '') {
            title = "<i class='fa fa-ban' aria-hidden='true'></i>";
        }
        html += title + ' <span>' + message + '</span></div>';
        alertTag.html(html);
    }
    _app.alertErrorJqXhr = function (jqXHR) {
        var alertTag = $("#error_container");
        var cls = 'alert-danger';
        var html = '<div class="alert ' + cls + ' alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>';
        var title = "<i class='fa fa-ban' aria-hidden='true'></i>";
        html += title + '<span>' + jqXHR.status + ' ' + jqXHR.statusText + ': ' + jqXHR.responseText + '</span></div>';
        alertTag.html(html);
    }
    _app.alertWarning = function (message, title) {
        var alertTag = $("#error_container");
        var cls = 'alert-warning';
        var html = '<div class="alert ' + cls + ' alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>';
        if (typeof title === 'undefined' || title === '') {
            title = "<i class='fa fa-exclamation-triangle' aria-hidden='true'></i>";
        }
        html += title + ' <span>' + message + '</span></div>';
        alertTag.html(html);
    }
    _app.alertInfo = function (message, title) {
        var alertTag = $("#error_container");
        var cls = 'alert-info';
        var html = '<div class="alert ' + cls + ' alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>';
        if (typeof title === 'undefined' || title === '') {
            title = "<i class='fa fa-info-circle' aria-hidden='true'></i>";
        }
        html += title + ' <span>' + message + '</span></div>';
        alertTag.html(html);
    }
    _app.alertSuccess = function (message, title) {
        var alertTag = $("#error_container");
        var cls = 'alert-success';
        var html = '<div class="alert ' + cls + ' alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>';
        if (typeof title === 'undefined' || title === '') {
            title = "<i class='fa fa-check-circle-o' aria-hidden='true'></i>";
        }
        html += title + ' <span>' + message + '</span></div>';
        alertTag.html(html);
    }

    _app.setSideMenuItemActive = function (url) {
        $("ul.treeview-menu a[href='" + url + "']").parentsUntil(".sidebar-menu > .treeview-menu")
            .addClass("active");
    };

    _app.generateRandomNumber = function (max, min) {
        arr = [];
        for (i = 0; i < max; i++) {
            x = Math.floor(Math.random() * max) + min;
            if (arr.includes(x) == true) {
                i = i - 1;
            } else {
                if (x > max == false) {
                    arr.push(x);
                }
            }
        }
        return arr;
    }

    _app.openInNewTab = function (url) {
        window.open(url, '_blank').focus();
    }

    return _app;
}());