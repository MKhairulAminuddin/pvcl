(function ($, window, document) {
    $(function () {
        //#region Variable Definition

        var $dateSelectionBtn,
            $printBtn,
            $viewTypeDropdown,
            $formRemarksBtn,
            $formRemarksGrid;

        var $viewFormRemarksModal = $("#viewFormRemarksModal");

        var referenceUrl = {
            printRequest: window.location.origin + "/Report/DealCutOffFcy/Print",
            printResponse: window.location.origin + "/Report/DealCutOffFcy/Printed/",

            loadFormLists: window.location.origin + "/api/common/FormList/",
            loadFormRemarks: window.location.origin + "/api/common/FormRemarks/"
        };

        //#endregion

        //#region Data Source & Functions

        var populateData = function (selectedDate, viewType) {
            var epochDate = moment(selectedDate).unix();
            var params = "TradeDate=" + encodeURIComponent(epochDate) + "&ViewType=" + viewType;

            var iframeElement0 = document.getElementById("sheet1");
            iframeElement0.src = "./DealCutOffFcyPreview?" + params + "&SheetIndex=0";

            var iframeElement2 = document.getElementById("sheet2");
            iframeElement2.src = "./DealCutOffFcyPreview?" + params + "&SheetIndex=1";

            var iframeElement3 = document.getElementById("sheet3");
            iframeElement3.src = "./DealCutOffFcyPreview?" + params + "&SheetIndex=2";

            var iframeElement4 = document.getElementById("sheet4");
            iframeElement4.src = "./DealCutOffFcyPreview?" + params + "&SheetIndex=3";

            var iframeElement5 = document.getElementById("sheet5");
            iframeElement5.src = "./DealCutOffFcyPreview?" + params + "&SheetIndex=4";

            var iframeElement6 = document.getElementById("sheet6");
            iframeElement6.src = "./DealCutOffFcyPreview?" + params + "&SheetIndex=5";

            var iframeElement7 = document.getElementById("sheet7");
            iframeElement7.src = "./DealCutOffFcyPreview?" + params + "&SheetIndex=6";
        }

        var formListData = function () {
            var selectedDate = moment($dateSelectionBtn.option("value")).unix();
            var selectedFormStatus = $viewTypeDropdown.option("value");

            return {
                store: DevExpress.data.AspNet.createStore({
                    loadUrl: referenceUrl.loadFormLists + "FCY/" + selectedFormStatus + "/" + selectedDate
                })
            };
        }

        var formRemarksData = function formRemarksData(formType, formId) {
            return referenceUrl.loadFormRemarks + formType + "/" + formId;
        }

        function masterDetailTemplate(_, masterDetailOptions) {
            return $("<div>").dxTabPanel({
                items: [
                    {
                        title: "Forms Remark",
                        template: createRemarksGrid(masterDetailOptions.data)
                    }
                ]
            });
        }

        function createRemarksGrid(masterDetailData) {
            return function () {
                return $("<div>").dxDataGrid({
                    dataSource: formRemarksData(masterDetailData.formType, masterDetailData.formId),
                    showBorders: true,
                    wordWrapEnabled: true,
                    columns: [
                        {
                            caption: "Name",
                            dataField: "actionBy"
                        },
                        {
                            caption: "Date",
                            dataField: "actionDate",
                            dataType: "date",
                            format: "dd/MM/yyyy hh:mm a",
                        },
                        {
                            caption: "Remark",
                            dataField: "remarks"
                        }
                    ]
                });
            };
        }

        //#endregion

        //#region Other Widgets
        $dateSelectionBtn = $("#dateSelectionBtn").dxDateBox({
            type: "date",
            displayFormat: "dd/MM/yyyy",
            value: new Date(),
            onValueChanged: function (data) {
                populateData(data.value, $viewTypeDropdown.option("value"));
            }
        }).dxDateBox("instance");

        $viewTypeDropdown = $("#viewTypeDropdown").dxSelectBox({
            items: ["Approved", "LIVE"],
            value: "Approved",
            onValueChanged: function (data) {
                populateData($dateSelectionBtn.option("value"), data.value);
            }
        }).dxSelectBox("instance");

        $printBtn = $("#printBtn").dxDropDownButton({
            text: "Print",
            icon: "print",
            type: "normal",
            stylingMode: "contained",
            dropDownOptions: {
                width: 230
            },
            displayExpr: "name",
            keyExpr: "id",
            items: [
                { id: 1, name: "Excel Workbook (*.xlsx)", icon: "fa fa-file-excel-o" },
                { id: 2, name: "PDF", icon: "fa fa-file-pdf-o" }
            ],
            onItemClick: function (e) {
                app.toast("Generating...");

                var data = {
                    tradeDate: moment($dateSelectionBtn.option("value")).unix(),
                    isExportAsExcel: (e.itemData.id == 1),
                    viewType: $viewTypeDropdown.option("value")
                };

                $.ajax({
                    type: "POST",
                    url: referenceUrl.printRequest,
                    data: data,
                    dataType: "text",
                    success: function (data) {
                        var url = referenceUrl.printResponse + data;
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
        }).dxDropDownButton("instance");

        $formRemarksBtn = $("#formRemarksBtn").dxButton({
            text: "Remarks",
            icon: "fa fa-commenting-o",
            type: "normal",
            stylingMode: "contained",
            onClick: function (e) {
                $formRemarksGrid.option("dataSource", formListData());
                $viewFormRemarksModal.modal("show");
                e.event.preventDefault();
            }
        }).dxButton("instance");

        var tabPanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { titleId: "summaryTab", title: "Summary", template: "summaryTab" },
                { titleId: "details1", title: "CIMB FCA", template: "details1" },
                { titleId: "details2", title: "CITI MFCA", template: "details2" },
                { titleId: "details3", title: "Hong Leong Bank MFCA", template: "details3" },
                { titleId: "details4", title: "JP Morgan MFCA", template: "details4" },
                { titleId: "details5", title: "Maybank MFCA", template: "details5" },
                { titleId: "formAudit", title: "Form Audit", template: "formAudit" }
            ],
            deferRendering: false,
            showNavButtons: true
        });

        //#endregion

        // #region DataGrid

        $formRemarksGrid = $("#formRemarksGrid").dxDataGrid({
            columns: [
                {
                    dataField: "formId",
                    caption: "Form ID",
                    width: "50px"
                },
                {
                    dataField: "formType",
                    caption: "Form Type"
                },
                {
                    dataField: "formCurrency",
                    caption: "currency"
                },
                {
                    dataField: "formStatus",
                    caption: "Form Status"
                },
                {
                    dataField: "formDate",
                    caption: "Form/Settlement/Value Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy"
                },
                {
                    dataField: "preparedBy",
                    caption: "Prepared By"
                },
                {
                    dataField: "approvedBy",
                    caption: "Approved By"
                },
                {
                    dataField: "approvalDate",
                    caption: "Approval Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy hh:mm a"
                }
            ],
            showRowLines: true,
            rowAlternationEnabled: false,
            showBorders: true,
            sorting: {
                mode: "multiple",
                showSortIndexes: true
            },
            wordWrapEnabled: true,
            masterDetail: {
                enabled: true,
                template: masterDetailTemplate
            }
        }).dxDataGrid("instance");

        // #endregion DataGrid

        //#region Events



        //#endregion

        //#region Immediate Invocation function


        populateData(new Date(), $viewTypeDropdown.option("value"));

        //#endregion
    });
}(window.jQuery, window, document));