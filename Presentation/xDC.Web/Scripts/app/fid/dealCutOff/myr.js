(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        
        var $dateSelectionBtn,
            $printBtn;

        var referenceUrl = {
            printRequest: window.location.origin + "/fid/DealCutOffMyr/Print",
            printResponse: window.location.origin + "/fid/DealCutOffMyr/Printed/"
        };

        //#endregion

        //#region Data Source & Functions

        var populateData = function (selectedDate) {
            var epochDate = moment(selectedDate).unix();
            var params = "TradeDate=" + encodeURIComponent(epochDate);

            var iframeElement0 = document.getElementById("sheet1");
            iframeElement0.src = "./DealCutOffMyrPreview?" + params + "&SheetIndex=0";

            var iframeElement2 = document.getElementById("sheet2");
            iframeElement2.src = "./DealCutOffMyrPreview?" + params + "&SheetIndex=1";

            var iframeElement3 = document.getElementById("sheet3");
            iframeElement3.src = "./DealCutOffMyrPreview?" + params + "&SheetIndex=2";
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
        
        
        // #endregion DataGrid

        //#region Events

        

        //#endregion

        //#region Immediate Invocation function

        
        populateData(new Date());

        //#endregion
    });
}(window.jQuery, window, document));