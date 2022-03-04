﻿(function ($, window, document) {
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

            loadFormRemarks: window.location.origin + "/api/common/FormRemarksMyr/"
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
        }

        var formRemarksData = function () {
            var selectedDate = moment($dateSelectionBtn.option("value")).unix();

            return {
                store: DevExpress.data.AspNet.createStore({
                    loadUrl: referenceUrl.loadFormRemarks + selectedDate
                })
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
                $formRemarksGrid.option("dataSource", formRemarksData());
                $viewFormRemarksModal.modal("show");
                e.event.preventDefault();
            }
        }).dxButton("instance");

        var tabPanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { titleId: "titleBadge2", title: "Cashflow", template: "bondTab" },
                { titleId: "titleBadge3", title: "Money Market", template: "cpTab" },
                { titleId: "titleBadge4", title: "Others", template: "notesPaperTab" }
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
                    dataField: "formType",
                    caption: "Form Type"
                },
                {
                    dataField: "formDate",
                    caption: "Form/Settlement/Value Date",
                    dataType: "datetime",
                    format: "dd/MM/yyyy"
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
                },
                {
                    dataField: "remarks",
                    caption: "Remarks"
                }
            ],
            showRowLines: true,
            rowAlternationEnabled: false,
            showBorders: true,
            sorting: {
                mode: "multiple",
                showSortIndexes: true
            },
            wordWrapEnabled: true
        }).dxDataGrid("instance");
        
        
        // #endregion DataGrid

        //#region Events

        

        //#endregion

        //#region Immediate Invocation function

        
        populateData(new Date(), "Approved");

        //#endregion
    });
}(window.jQuery, window, document));