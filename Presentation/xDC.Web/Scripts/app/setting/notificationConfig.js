(function ($, window, document) {

    $(function () {
        var adUsersStore = DevExpress.data.AspNet.createStore({
            key: "username",
            loadUrl: "../api/common/GetActiveDirectoryUsers"
        });

        $("#contributionEmailList").dxTagBox({
            dataSource: adUsersStore,
            displayExpr: "displayName",
            valueExpr: "email",
            searchEnabled: true,
            itemTemplate: function (data) {
                return "<div class='active-directory-dropdown'>" +
                    "<p class='active-directory-title'>" + data.displayName + "</p>" +
                    "<p class='active-directory-subtitle'>" + data.title + ", " + data.department + "</p>" +
                    "<p class='active-directory-subtitle'>" + data.email + "</p>" +
                    "</div>";
            }
        }).dxTagBox("instance");

    });
}(window.jQuery, window, document));