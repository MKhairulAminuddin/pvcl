$(function () {
    var $dxPermissionDetailsTree = $('#permissionDetailsTree');
    var $dxPermissionTreeView = $('#permissionTreeView');
    var $dxAddNewRoleBtn = $('#addNewRole');
    var $dxNewRoleNameTextBox = $('#newRoleNameTextBox');
    var $dxNewPermissionTreeView = $('#newPermissionTreeView');
    var $addNewRoleModal = $('#addNewRoleModal');
    var $saveNewRoleBtn = $('#saveNewRoleBtn');
    var $roleNameInDetailPermission = $("#permissionName");

    $dxPermissionDetailsTree.dxTreeView().dxTreeView('instance');


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

    var instantiatePermissionDetailsTreeView = function (data) {
        if (data) {
            $roleNameInDetailPermission.text(data.roleName);
            $dxPermissionDetailsTree.dxTreeView({
                dataSource: permissionData(data.roleId),
                dataStructure: 'plain',
                parentIdExpr: 'parentId',
                keyExpr: 'permissionId',
                displayExpr: 'permissionName',
                showCheckBoxesMode: 'normal',
                disabled: true,
                itemTemplate(item) {
                    return `<div>${item.permissionName}</div>`;
                }
            }).dxTreeView('instance');
        } else {
            $roleNameInDetailPermission.text("");
            $dxPermissionDetailsTree.dxTreeView("dispose");
        }
        
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
            allowAdding: false
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
                dataField: "roleId",
                visible: false,
                allowEditing: false,
            },
            {
                caption: "Role Name",
                dataField: "roleName"
            },
        ],
        onCellPrepared: function (e) {
            if (e.rowType == "data") {
                if (e.data.roleName == "Administrator")
                    e.cellElement.find(".dx-link-delete").remove();
            }
        },
        onSelectionChanged(selectedItems) {
            const data = selectedItems.selectedRowsData[0];
            if (data) {
                instantiatePermissionDetailsTreeView(data);
            }
        },
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
                    RoleName: $dxNewRoleNameTextBox.dxTextBox("instance").option("value"),
                    Permissions: []
            };

            $dxNewPermissionTreeView.dxTreeView("getSelectedNodes").forEach(function (x) {
                requestData.Permissions.push({
                    'parentId': x.itemData.parentId,
                    'permissionId': x.itemData.permissionId,
                    'permissionName': x.itemData.permissionName
                })
            });

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
                },
                complete: function (jqXHR, textStatus) {
                    $grid1.refresh();
                    $dxNewPermissionTreeView.dxTreeView("unselectAll");
                    $dxNewRoleNameTextBox.dxTextBox("reset");
                    instantiatePermissionDetailsTreeView();
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

    var $refreshDetailsBtn = $("#refreshDetailsBtn").dxButton({
        onClick(e) {
            var selectedItem = $grid1.getSelectedRowsData();
            if (selectedItem.length > 0) {
                instantiatePermissionDetailsTreeView(selectedItem[0])
            } else {
                app.toast("Please select at least one row", "warning" , 2000);
            }

            e.event.preventDefault;
        }
    })


    // #endregion
    
});