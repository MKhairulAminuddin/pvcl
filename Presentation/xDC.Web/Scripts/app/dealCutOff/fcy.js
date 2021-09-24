(function ($, window, document) {
    $(function () {
        //#region Variable Definition

        var $dateSelectionBtn,
            $printBtn;

        var referenceUrl = {
            printRequest: window.location.origin + "/DealCutOff/Fcy/Print",
            printResponse: window.location.origin + "/DealCutOff/Fcy/Printed/"
        };

        //#endregion

        //#region Data Source & Functions

        var populateData = function (selectedDate) {
            var epochDate = moment(selectedDate).unix();
            var params = "TradeDate=" + encodeURIComponent(epochDate);

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
        }

        //#endregion

        //#region Other Widgets
        $dateSelectionBtn = $("#dateSelectionBtn").dxDateBox({
            type: "date",
            displayFormat: "dd/MM/yyyy",
            value: new Date(),
            onValueChanged: function (data) {
                populateData(data.value);
            }
        }).dxDateBox("instance");

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
                    TradeDate: moment($dateSelectionBtn.option("value")).unix(),
                    isExportAsExcel: (e.itemData.id == 1)
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

        var tabPanel = $("#tabpanel-container").dxTabPanel({
            dataSource: [
                { titleId: "summaryTab", title: "Summary", template: "summaryTab" },
                { titleId: "details1", title: "CIMB FCA", template: "details1" },
                { titleId: "details2", title: "CITI MFCA", template: "details2" },
                { titleId: "details3", title: "Hong Leong Bank MFCA", template: "details3" },
                { titleId: "details4", title: "JP Morgan MFCA", template: "details4" },
                { titleId: "details5", title: "Maybank MFCA", template: "details5" }
            ],
            deferRendering: false,
            showNavButtons: true
        });

        //#endregion

        // #region DataGrid


        // #endregion DataGrid

        //#region Events



        //#endregion

        //#region Immediate Invocation function


        populateData(new Date());

        //#endregion
    });
}(window.jQuery, window, document));