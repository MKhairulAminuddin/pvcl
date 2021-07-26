$(function () {
    var aduserStores = DevExpress.data.AspNet.createStore({
        key: "username",
        loadUrl: "../api/common/GetActiveDirectoryUsersRegisteredIntoSystem"
    });

    var formTypes = [
        {
            "id": "Inflow Funds",
            "name": "AMSD Inflow Funds"
        }, {
            "id": "Trade Settlement",
            "name": "IISD Trade Settlement"
        }
    ];


    var $grid1;

    $grid1 = $("#grid1").dxDataGrid({
        dataSource: DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: "../api/setting/GetApprover",
            insertUrl: "../api/setting/insertApprover",
            updateUrl: "../api/setting/updateApprover",
            deleteUrl: "../api/setting/deleteApprover"
        }),
        remoteOperations: true,
        searchPanel: {
            visible: true
        },
        selection: { mode: "single" },
        editing: {
            mode: "form",
            allowUpdating: true,
            allowDeleting: true,
            allowAdding: true
        },
        columns: [
            {
                caption: "Approver",
                dataField: "username",
                lookup: {
                    dataSource: aduserStores,
                    valueExpr: "username",
                    displayExpr: "displayName"
                },
                validationRules: [
                    { type: "required" }
                ]
            },
            {
                caption: "Form Type",
                dataField: "formType",
                lookup: {
                    dataSource: formTypes,
                    valueExpr: "id",
                    displayExpr: "name"
                },
                validationRules: [{ type: "required" }]
            },
            {
                caption: "Created Date",
                dataField: "createdDate",
                dataType: "date",
                format: "dd/MM/yyyy HH:mm:ss",
                allowEditing: false
            }
        ]
    }).dxDataGrid('instance');

    $grid1.option(dxGridUtils.editingGridConfig);
});