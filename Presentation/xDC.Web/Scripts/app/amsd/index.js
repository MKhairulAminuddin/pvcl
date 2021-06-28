(function ($, window, document) {

    $(function () {
        var amsdGridData = [
            {
                formId: "123",
                formType: "AMSD",
                submittedDatetime: "03/06/2021 10:46 AM",
                submittedBy: "Muhammad Solehudden Mohd Mansor Kaman (KWAP)",
                formStatus: "Pending Approval"
            },
            {
                formId: "123",
                formType: "AMSD",
                submittedDatetime: "03/06/2021 10:46 AM",
                submittedBy: "Abdul Amer (KWAP)",
                formStatus: "Pending Review"
            },
        ];

        var $amsdGrid, $btnNewForm;
        

        $amsdGrid = $("#amsdGrid").dxDataGrid({
            dataSource: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: "../api/amsd/GetAmsdForms"
            }),
            columns: [
                {
                    dataField: "formId",
                    caption: "Form ID"
                },
                {
                    dataField: "formType",
                    caption: "Form Type"
                },
                {
                    dataField: "submittedDatetime",
                    caption: "Submitted Datetime",
                    dataType: "datetime",

                },
                {
                    dataField: "submittedBy",
                    caption: "Submitted By"
                },
                {
                    dataField: "formStatus",
                    caption: "Status"
                },
                {
                    caption: "",
                    width: "100px"
                }
            ],
            showBorders: true,
            height: 300,
            editing: {
                mode: "row",
                allowUpdating: false,
                allowDeleting: false,
                allowAdding: false
            },
            summary: {
                totalItems: [{
                    column: "formStatus",
                    summaryType: "count"
                }]
            }
        }).dxDataGrid("instance");



    });
}(window.jQuery, window, document));