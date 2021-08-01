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
}