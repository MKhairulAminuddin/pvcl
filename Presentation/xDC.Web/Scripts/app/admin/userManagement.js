$(function () {
    var roleStores = function () {
        return {
            store: DevExpress.data.AspNet.createStore({
                loadUrl: window.location.origin + "/api/common/GetRoles"
            }),
            paginate: true,
            pageSize: 20
        };
    }
    
    var aduserStores = function () {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "username",
                loadUrl: window.location.origin + "/api/common/GetActiveDirectoryUsers"
            }),
            paginate: true,
            pageSize: 20
        };
    }

    var $grid1;

    $grid1 = $("#grid1").dxDataGrid({
        dataSource: DevExpress.data.AspNet.createStore({
            key: "userName",
            loadUrl: window.location.origin + "/api/admin/GetUsers",
            insertUrl: window.location.origin + "/api/admin/insertUser",
            updateUrl: window.location.origin + "/api/admin/updateUser",
            deleteUrl: window.location.origin + "/api/admin/deleteUser"
        }),
        remoteOperations: true,
        selection: { mode: "single" },
        editing: {
            mode: "form",
            allowUpdating: true,
            allowDeleting: true,
            allowAdding: true
        },
        columns: [
            {
                caption: "User",
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
                caption: "Title",
                dataField: "title",
                allowEditing: false,
                visible: false
            },
            {
                caption: "Department",
                dataField: "department",
                allowEditing: false
            },
            {
                caption: "Email",
                dataField: "email",
                allowEditing: false
            },
            {
                caption: "Tel No",
                dataField: "telephoneNumber",
                allowEditing: false,
                visible: false
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
                },
                visible: false
            },
            {
                caption: "Created Date",
                dataField: "createdDate",
                dataType: "date",
                format: "dd/MM/yyyy HH:mm:ss",
                allowEditing: false
            }
        ],
        headerFilter: {
            visible: true,
            allowSearch: true
        },
        searchPanel: {
            visible: true
        },
        filterPanel: { visible: true },
        showRowLines: true,
        showBorders: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        sorting: {
            mode: "multiple",
            showSortIndexes: true
        },
        columnChooser: {
            enabled: true,
            mode: "select"
        },
        stateStoring: {
            enabled: true,
            type: "localStorage",
            storageKey: "xDC_Admin_User"
        }
    }).dxDataGrid('instance');
});