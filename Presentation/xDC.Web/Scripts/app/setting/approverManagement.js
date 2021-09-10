$(function () {
    var aduserStores = function() {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "username",
                loadUrl: window.location.origin + "/api/common/GetActiveDirectoryUsersRegisteredIntoSystem"
            }),
            paginate: true,
            pageSize: 20
        }
    };

    var formTypes = [
        {
            "id": "Inflow Fund",
            "name": "AMSD Inflow Fund"
        }, {
            "id": "Trade Settlement",
            "name": "ISSD Trade Settlement"
        }, {
            "id": "Treasury",
            "name": "FID Treasury"
        }
    ];


    var $grid1;

    $grid1 = $("#grid1").dxDataGrid({
        dataSource: DevExpress.data.AspNet.createStore({
            key: "id",
            loadUrl: window.location.origin + "/api/setting/GetApprover",
            insertUrl: window.location.origin + "/api/setting/insertApprover",
            updateUrl: window.location.origin + "/api/setting/updateApprover",
            deleteUrl: window.location.origin + "/api/setting/deleteApprover"
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