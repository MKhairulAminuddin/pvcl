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
            $searchBtn,
            $dg_auditForm;
        

        var referenceUrl = {
            loadAuditForm: window.location.origin + "/api/audit/auditForm/",

            printRequest: window.location.origin + "/amsd/Print",
            printResponse: window.location.origin + "/amsd/Printed/",
        };

        //#endregion

        //#region Data Source & Functions

        var loadData = function (fromDate, toDate, formId, formType, userId, actionType) {
            formId = formId == "" ? null : formId;
            return $.ajax({
                url: referenceUrl.loadAuditForm + moment(fromDate).unix() + "/" + moment(toDate).unix() + "/" + formId + "/" + formType + "/" + userId + "/" + actionType,
                type: "get"
            });
        }

        var loadDataToGrid = function(){
            $.when(
                loadData(
                    $dp_fromDate.option("value"),
                    $dp_toDate.option("value"),
                    $tb_formId.option("value"),
                    $dd_formType.option("value"),
                    $dd_userId.option("value"),
                    $dd_actionType.option("value"),
                )
            )
                .done(function (grid1) {
                    if (grid1.data.length === 0) {
                        dxGridUtils.clearGrid($dg_auditForm);

                    } else {
                        $dg_auditForm.option("dataSource", grid1.data);
                        $dg_auditForm.repaint();
                    }
                });
        }

        //#endregion

        //#region Widgets

        $dp_fromDate = $('#dp-fromDate').dxDateBox({
            type: 'date',
            value: moment().add("days", -7),
            displayFormat: "dd/MM/yyyy"
        }).dxDateBox("instance");

        $dp_toDate = $('#dp-toDate').dxDateBox({
            type: 'date',
            value: new Date(),
            displayFormat: "dd/MM/yyyy"
        }).dxDateBox("instance");

        $tb_formId = $('#tb-formId').dxTextBox({
            placeholder: 'Form Id...'
        }).dxTextBox("instance");

        $dd_formType = $('#dd-formType').dxSelectBox({
            items: [
                "Treasury",
                "Inflow Fund",
                "Trade Settlement (Part A)",
                "Trade Settlement (Part B)",
                "Trade Settlement (Part C)",
                "Trade Settlement (Part D)",
                "Trade Settlement (Part E)",
                "Trade Settlement (Part F)",
                "Trade Settlement (Part G)",
                "Trade Settlement (Part H)"
            ],
            placeholder: 'Form Type',
            showClearButton: true,
        }).dxSelectBox("instance");

        $dd_userId = $('#dd-userId').dxSelectBox({
            items: ["abdulkhaliq.h"],
            placeholder: 'User Id...',
            showClearButton: true,
        }).dxSelectBox("instance");

        $dd_actionType = $('#dd-actionType').dxSelectBox({
            items: [
                "CREATE",
                "MODIFY",
                "DELETE",
                "APPROVE",
                "REJECT",
                "REASSIGN",
                "REQUEST APPROVAL"
            ],
            placeholder: 'Action Type...',
            showClearButton: true,
        }).dxSelectBox("instance");

        $searchBtn = $("#searchBtn").dxButton({
            text: "Search",
            type: "default",
            icon: "find",
            onClick: function (e) {
                // search function here
                loadDataToGrid();
            }
        }).dxButton("instance");


        $dg_auditForm = $("#dg-auditForm").dxDataGrid({
            columns: [
                {
                    dataField: "formId",
                    caption: "Form ID"
                },
                {
                    dataField: "actionType",
                    caption: "Action"
                },
                {
                    dataField: "modifiedOn",
                    caption: "Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy hh:mm:ss",
                    sortIndex: 0,
                    sortOrder: "desc"
                },
                {
                    dataField: "modifiedBy",
                    caption: "User ID"
                },
                {
                    dataField: "remarks",
                    caption: "Remarks"
                },
                {
                    dataField: "valueBefore",
                    caption: "Value Before"
                },
                {
                    dataField: "valueAfter",
                    caption: "Value After"
                }
            ],
            showBorders: true,
            showColumnLines: true,
            showRowLines: true,
            sorting: {
                mode: "multiple"
            },
            searchPanel: {
                visible: true
            },
            pager: {
                visible: true,
                allowedPageSizes: [10, 20, 50, "all"],
                showPageSizeSelector: true,
                showInfo: true,
                showNavigationButtons: true
            },
            filterPanel: {
                visible: true
            }
        }).dxDataGrid("instance");


        //#endregion

        //#region Events

        loadDataToGrid();

        //#endregion
        
    });
}(window.jQuery, window, document));