(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        
        var $dateSelectionBtn,
            $formRemarksBtn,
            $printBtn,
            $viewTypeDropdown,
            $formRemarksGrid;

        var $viewFormRemarksModal = $("#viewFormRemarksModal");

        var referenceUrl = {
            printRequest: window.location.origin + "/DealCutOff/Myr/Print",
            printResponse: window.location.origin + "/DealCutOff/Myr/Printed/",

            loadFormLists: window.location.origin + "/api/common/FormList/",
            loadFormRemarks: window.location.origin + "/api/common/FormRemarks/"
        };

        //#endregion

        //#region Data Source & Functions

        var populateData = function (selectedDate, viewType) {
            var epochDate = moment(selectedDate).unix();
            var params = "TradeDate=" + encodeURIComponent(epochDate) + "&ViewType=" + viewType;

            var iframeElement0 = document.getElementById("sheet1");
            iframeElement0.src = "./DealCutOffMyrPreview?" + params + "&SheetIndex=0";

            var iframeElement2 = document.getElementById("sheet2");
            iframeElement2.src = "./DealCutOffMyrPreview?" + params + "&SheetIndex=1";

            var iframeElement3 = document.getElementById("sheet3");
            iframeElement3.src = "./DealCutOffMyrPreview?" + params + "&SheetIndex=2";

            var iframeElement4 = document.getElementById("sheet4");
            iframeElement4.src = "./DealCutOffMyrPreview?" + params + "&SheetIndex=3";
        }

        var formListData = function () {
            var selectedDate = moment($dateSelectionBtn.option("value")).unix();
            var selectedFormStatus = $viewTypeDropdown.option("value");

            return {
                store: DevExpress.data.AspNet.createStore({
                    loadUrl: referenceUrl.loadFormLists + "MYR/" + selectedFormStatus + "/" + selectedDate
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
                { titleId: "titleBadge2", title: "Cashflow", template: "cashflowTab" },
                { titleId: "titleBadge3", title: "Money Market", template: "moneyMarketTab" },
                { titleId: "titleBadge4", title: "Others", template: "othersTab" },
                { titleId: "titleBadge5", title: "Form Audit", template: "auditListTab" }
            ],
            deferRendering: false,
            //itemTitleTemplate: $("#dxPanelTitle"),
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

        
        populateData(new Date(), "Approved");

        //#endregion
    });
}(window.jQuery, window, document));