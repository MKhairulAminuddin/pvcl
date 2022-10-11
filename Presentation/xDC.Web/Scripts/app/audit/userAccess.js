
$(function () {
    var $grid1,
        $dxFromDateBox = $("#fromDateBox"),
        $dxToDateBox = $("#toDateBox"),
        $dxUserIdSelectBox = $("#userIdSelectBox"),
        $dxSearchBtn = $("#searchBtn"),
        $dxPrintBtn = $("#printBtn");

    var referenceUrl = {
        loadUserAccessAudit: window.location.origin + "/api/audit/userAccess/",

        loadUserIdList: "/api/common/Users",

    };

    //#region Data Source

    var loadUsersList = function () {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "userName",
                loadUrl: referenceUrl.loadUserIdList
            }),
            paginate: true,
            pageSize: 20
        };
    }

    var loadFilteredGridData = function (fromDate, toDate, userId) {
        var request = {
            fromDateUnix: moment(fromDate).unix() ,
            toDateUnix: moment(toDate).unix() ,
            userId: userId
        }

        return $.ajax({
            url: referenceUrl.loadUserAccessAudit,
            type: "post",
            data: request
        });
    }

    var loadDataToGrid = function () {
        $.when(
            loadFilteredGridData(
                $dxFromDateBox.dxDateBox("instance").option("value"),
                $dxToDateBox.dxDateBox("instance").option("value"),
                $dxUserIdSelectBox.dxSelectBox("instance").option("value")
            )
        )
            .done(function (grid1) {
                if (grid1.data.length === 0) {
                    dxGridUtils.clearGrid($grid1);

                } else {
                    $grid1.option("dataSource", grid1.data);
                    $grid1.repaint();
                }
            });
    }

    //#endregion

   

    $dxUserIdSelectBox.dxSelectBox({
        dataSource: loadUsersList(),
        displayExpr: "userName",
        valueExpr: "userName"
    });

    $grid1 = $("#grid1").dxDataGrid({
        dataSource: window.location.origin + "/api/audit/userAccess",
        selection: { mode: "single" },
        editing: {
            enabled: false
        },
        columns: [
            {
                caption: "Username",
                dataField: "userName"
            },
            {
                caption: "IP Address",
                dataField: "clientAddress"
            },
            {
                caption: "Browser",
                dataField: "clientBrowser"
            },
            {
                caption: "Recorded Date",
                dataField: "recordedDate",
                dataType: "date",
                format: "dd/MM/yyyy HH:mm:ss",
                sortIndex: 1,
                sortOrder: "desc"
            }
        ]
    }).dxDataGrid('instance');

    $grid1.option(dxGridUtils.viewOnlyGridConfig);

    $dxSearchBtn.dxButton({
        onClick(e) {
            loadDataToGrid();
        }
    });

    $dxPrintBtn.dxButton({
        onClick() {
            dxGridUtils.exportGrid({
                grids: [
                    $grid1,
                ],
                gridsTitle: [
                    "User Access Audit Report"
                ],
                fileName: "User Access Audit Report"
            })
        }
    });


});