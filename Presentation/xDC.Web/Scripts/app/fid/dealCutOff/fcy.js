(function ($, window, document) {
    $(function () {
        //#region Variable Definition
        
        var $dateSelectionBtn,
            $printBtn;

        var referenceUrl = {
            printRequest: window.location.origin + "/fid/DealCutOffFcy/Print",
            printResponse: window.location.origin + "/fid/DealCutOffFcy/Printed/"
        };

        //#endregion

        //#region Data Source & Functions
        
        //#endregion

        //#region Other Widgets
        $dateSelectionBtn = $("#dateSelectionBtn").dxDateBox({
            type: "date",
            displayFormat: "dd/MM/yyyy",
            value: new Date(),
            onValueChanged: function (data) {

                var epochDate = moment(data.value).unix();
                var params = "TradeDate=" + encodeURIComponent(epochDate);

                var iframeElement = document.getElementById("previewFrame0");
                iframeElement.src = "./DealCutOffMyrPreview?" + params;
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

        //#endregion

        // #region DataGrid
        
        
        // #endregion DataGrid

        //#region Events

        

        //#endregion

        //#region Immediate Invocation function
        

        //#endregion
    });
}(window.jQuery, window, document));