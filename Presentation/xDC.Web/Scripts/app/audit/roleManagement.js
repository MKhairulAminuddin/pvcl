
$(function () {
    var $grid1,
        $dxFromDateBox = $("#fromDateBox"),
        $dxToDateBox = $("#toDateBox"),
        $dxRoleSelectBox = $("#roleSelectBox"),
        $dxSearchBtn = $("#searchBtn"),
        $dxPrintBtn = $("#printBtn");

    var referenceUrl = {
        loadRoleManagementAudit: window.location.origin + "/api/audit/roleManagement/",
        loadRolesList: "/api/common/GetRoles",
    };

    //#region Data Source

    var loadRolesList = function () {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "name",
                loadUrl: referenceUrl.loadRolesList
            }),
        };
    }

    var loadFilteredGridData = function (fromDate, toDate, roleName) {
        var request = {
            fromDateUnix: moment(fromDate).unix(),
            toDateUnix: moment(toDate).unix(),
            role: roleName
        }

        return $.ajax({
            url: referenceUrl.loadRoleManagementAudit,
            type: "post",
            data: request
        });
    }

    var loadDataToGrid = function () {
        $.when(
            loadFilteredGridData(
                $dxFromDateBox.dxDateBox("instance").option("value"),
                $dxToDateBox.dxDateBox("instance").option("value"),
                $dxRoleSelectBox.dxSelectBox("instance").option("value")
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



    $dxRoleSelectBox.dxSelectBox({
        dataSource: loadRolesList(),
        displayExpr: "name",
        valueExpr: "name"
    });

    $grid1 = $("#grid1").dxDataGrid({
        dataSource: referenceUrl.loadRoleManagementAudit,
        selection: { mode: "single" },
        editing: {
            enabled: false
        },
        columns: [
            {
                caption: "Activity",
                dataField: "activity"
            },
            {
                caption: "Remarks",
                dataField: "remarks"
            },
            {
                caption: "Role",
                dataField: "role"
            },
            {
                caption: "Performed By",
                dataField: "performedBy"
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
                    "Role Management Audit Report"
                ],
                fileName: "Role Management Audit Report"
            })
        }
    });


});