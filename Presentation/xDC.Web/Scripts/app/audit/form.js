(function ($, window, document) {

    $(function () {

        //#region Variable Definition

        var $auditFormGrid,
            $dp_fromDate,
            $dp_toDate,
            $tb_formId,
            $dd_formType,
            $dd_userId,
            $dd_actionType,
            $searchBtn;
        

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

        $dp_fromDate = $('#dp-fromDate').dxDateBox({
            type: 'date',
            value: new Date(),
        }).dxDateBox("instance");

        $dp_toDate = $('#dp-toDate').dxDateBox({
            type: 'date',
            value: new Date(),
        }).dxDateBox("instance");

        $tb_formId = $('#tb-formId').dxTextBox({
            placeholder: 'Form Id...'
        }).dxTextBox("instance");

        $dd_formType = $('#dd-formType').dxSelectBox({
            items: ["Trade Settlement", "Treasury", "Inflow Fund"],
            placeholder: 'Form Type',
            showClearButton: true,
        }).dxSelectBox("instance");

        $dd_userId = $('#dd-userId').dxSelectBox({
            items: ["abdulkhaliq.h"],
            placeholder: 'User Id...',
            showClearButton: true,
        }).dxSelectBox("instance");

        $dd_actionType = $('#dd-actionType').dxSelectBox({
            items: ["Create Form", "Edit Record", "Delete Record"],
            placeholder: 'Action Type...',
            showClearButton: true,
        }).dxSelectBox("instance");

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
        
    });
}(window.jQuery, window, document));