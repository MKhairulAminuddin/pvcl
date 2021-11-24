(function ($, window, document) {

    $(function () {

        //#region Variable Definition

        var $auditFormGrid,
            $dp_fromDate,
            $dp_toDate,
            $;

        var statuses = [
            "All",
            "Draft",
            "Pending Approval",
            "Approved",
            "Rejected"
        ];

        var referenceUrl = {
            loadAmsdGrid: window.location.origin + "/api/amsd/inflowfund",

            deleteForm: window.location.origin + "/api/amsd/inflowfund",


            editPageRedirect: window.location.origin + "/amsd/inflowfund/edit/",
            viewPageRedirect: window.location.origin + "/amsd/inflowfund/view/",

            printRequest: window.location.origin + "/amsd/Print",
            printResponse: window.location.origin + "/amsd/Printed/",
        };

        //#endregion

        //#region Data Source & Functions



        //#endregion

        //#region Widgets

        $searchBtn = $("#searchBtn").dxButton({
            text: "Search",
            type: "default",
            icon: "find",
            onClick: function (e) {
                // search function here
            }
        }).dxButton("instance");

        //#endregion

        //#region Events



        //#endregion

        



        $gridFilterDropdown = $("#gridFilterDropdown").dxSelectBox({
            dataSource: statuses,
            value: statuses[0],
            onValueChanged: function (data) {
                if (data.value == "All")
                    $amsdGrid.clearFilter();
                else
                    $amsdGrid.filter(["formStatus", "=", data.value]);
            }
        });
    });
}(window.jQuery, window, document));