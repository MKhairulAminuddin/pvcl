$(function () {
    var $dxPermissionTreeView = $('#permissionTreeView');
    var $dxAddNewRoleBtn = $('#addNewRole');
    var $dxNewRoleNameTextBox = $('#newRoleNameTextBox');
    var $dxNewPermissionTreeView = $('#newPermissionTreeView');
    var $addNewRoleModal = $('#addNewRoleModal');
    var $saveNewRoleBtn = $('#saveNewRoleBtn');



    var permissionData = function (roleId) {
        return {
            store: DevExpress.data.AspNet.createStore({
                loadUrl: window.location.origin + "/api/admin/GetRolePermissions/" + roleId
            })
        };
    }

    var permissionTreeData = function () {
        return {
            store: DevExpress.data.AspNet.createStore({
                loadUrl: window.location.origin + "/api/admin/GetPermissions"
            })
        };
    }

    var $grid1 = $("#grid1").dxDataGrid({
        dataSource: DevExpress.data.AspNet.createStore({
            key: "roleId",
            loadUrl: window.location.origin + "/api/admin/GetRoles",
            updateUrl: window.location.origin + "/api/admin/UpdateRole",
            deleteUrl: window.location.origin + "/api/admin/DeleteRole"
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
                type: 'buttons',
                buttons: [
                    'edit', 'delete',
                    {
                        text: 'Permission',
                        onClick(e) {
                            $("#modalTitle").text(e.row.data.roleName);
                            $dxPermissionTreeView.dxTreeView({
                                dataSource: permissionData(e.row.data.roleId),
                                dataStructure: 'plain',
                                parentIdExpr: 'parentId',
                                keyExpr: 'permissionId',
                                displayExpr: 'permissionName',
                                showCheckBoxesMode: 'normal',
                                itemTemplate(item) {
                                    return `<div>${item.permissionName}</div>`;
                                }
                            }).dxTreeView('instance');

                            $("#rolePermissionEditModal").modal('show');
                        },
                    }
                ],
            },
            {
                caption: "Role ID",
                dataField: "roleId"
            },
            {
                caption: "Role Name",
                dataField: "roleName"
            }
        ],
        headerFilter: {
            visible: true,
            allowSearch: true
        },
        searchPanel: {
            visible: true
        },
        showRowLines: true,
        showBorders: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        sorting: {
            mode: "multiple",
            showSortIndexes: true
        }
    }).dxDataGrid('instance');

    // #region Add New Role

    $dxNewPermissionTreeView.dxTreeView({
        dataSource: permissionTreeData(),
        dataStructure: 'plain',
        parentIdExpr: 'parentId',
        keyExpr: 'permissionId',
        displayExpr: 'permissionName',
        showCheckBoxesMode: 'normal',
        itemTemplate(item) {
            return `<div>${item.permissionName}</div>`;
        }
    }).dxTreeView('instance');

    $dxAddNewRoleBtn.dxButton({
        onClick: function (e) {
            $addNewRoleModal.modal('show');
        }
    });

    $saveNewRoleBtn.dxButton({
        onClick: function (e) {

            var requestData = {
                data: {
                    RoleName: $dxNewRoleNameTextBox.dxTextBox("instance").option("value"),
                    Permissions: []
                }
            };

            $dxNewPermissionTreeView.dxTreeView("getSelectedNodes").forEach(function (x) {
                requestData.data.Permissions.push({
                    'parentId': x.itemData.parentId,
                    'permissionId': x.itemData.permissionId,
                    'permissionName': x.itemData.permissionName
                })
            });

            console.log(requestData)

            $.ajax({
                data: requestData,
                url: window.location.origin + "/api/admin/AddNewRole",
                method: "post",
                success: function (response) {
                    app.toast("Saved", "info", 3000);
                    $addNewRoleModal.modal('hide');
                },
                error: function (msg) {
                    app.toast(msg, "error", 3000);
                    $addNewRoleModal.modal('hide');
                }
            });

            e.event.preventDefault();
        }
    })

    // #endregion

    // #region Edit Role Permission

    var $saveBtn = $("#saveBtn").on({
        "click": function (e) {

            var roleId = 0;
            var requestData = {
                "data": []
            };

            $dxPermissionTreeView.dxTreeView("getSelectedNodes").forEach(function (x) {
                requestData.data.push({
                    'expanded': x.itemData.expanded,
                    'parentId': x.itemData.parentId,
                    'permissionId': x.itemData.permissionId,
                    'permissionName': x.itemData.permissionName,
                    'selected': x.itemData.selected,
                    'roleId': x.itemData.roleId,
                })
                roleId = x.itemData.roleId;
            });

            if (roleId == 0) {
                var x = $dxPermissionTreeView.dxTreeView("getNodes")[0].itemData.roleId;
                roleId = x;
            }

            $.ajax({
                data: requestData,
                url: window.location.origin + "/api/admin/UpdateRolePermission/" + roleId,
                method: "post",
                success: function (response) {
                    app.toast("Saved", "info", 3000);
                    $("#rolePermissionEditModal").modal('hide');
                },
                error: function (msg) {
                    app.toast(msg, "error", 3000);
                    $("#rolePermissionEditModal").modal('hide');
                }
            });

            e.preventDefault();
        }
    });

    // #endregion
    
});