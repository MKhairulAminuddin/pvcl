$(function () {
    var roleStores = DevExpress.data.AspNet.createStore({
        loadUrl: "../api/common/GetRoles"
    });

    var aduserStores = DevExpress.data.AspNet.createStore({
        key: "username",
        loadUrl: "../api/common/adusers"
    });

    var $grid1;

    $grid1 = $("#grid1").dxDataGrid({
        dataSource: DevExpress.data.AspNet.createStore({
            key: "userName",
            loadUrl: "../api/admin/GetUsers",
            /*insertUrl: "../api/admin/insertUser",
            updateUrl: "../api/admin/updateUser",
            deleteUrl: "../api/admin/deleteUser"*/
        }),
        remoteOperations: true,
        searchPanel: {
            visible: true
        },
        selection: { mode: "single" },
        /*editing: {
            mode: "form",
            allowUpdating: true,
            allowDeleting: true,
            allowAdding: true
        },*/
        columns: [
            {
                caption: "Username",
                dataField: "userName",
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
                caption: "Role",
                dataField: "roleName",
                lookup: {
                    dataSource: roleStores,
                    valueExpr: "name",
                    displayExpr: "name"
                },
                validationRules: [{ type: "required" }]
            },
            {
                caption: "Status",
                dataField: "locked",
                dataType: "boolean",
                cellTemplate: function (element, info) {
                    if (info.value === true) {
                        element.append("<span class='label label-success'> Active </span>");
                    } else {
                        element.append("<span class='label label-danger'><i class='fa fa-lock' aria-hidden='true'></i> Disabled </span>");
                    }
                }
            },
            {
                caption: "Email",
                dataField: "email",
                allowEditing: false
            },
            {
                caption: "Tel No",
                dataField: "telNo",
                allowEditing: false
            },
            {
                caption: "Last Activity",
                dataField: "lastLogin",
                dataType: "date",
                format: "dd/MM/yyyy HH:mm:ss",
                allowEditing: false,
                cellTemplate: function (element, info) {
                    if (info.text === '01/01/1901 00:00:00') {
                        element.append("-");
                    } else {
                        element.append(info.text);
                    }
                }
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