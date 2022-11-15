
CREATE TABLE [dbo].[AspNetRolePermissions](
	[RoleId] [int] NOT NULL,
	[PermissionId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.AspNetRolePermissions] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC,
	[PermissionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[AspNetRolePermissions]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetRolePermissions_dbo.AspNetPermission_PermissionId] FOREIGN KEY([PermissionId])
REFERENCES [dbo].[AspNetPermission] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[AspNetRolePermissions] CHECK CONSTRAINT [FK_dbo.AspNetRolePermissions_dbo.AspNetPermission_PermissionId]
GO

ALTER TABLE [dbo].[AspNetRolePermissions]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetRolePermissions_dbo.AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[AspNetRolePermissions] CHECK CONSTRAINT [FK_dbo.AspNetRolePermissions_dbo.AspNetRoles_RoleId]
GO


INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 1)
GO
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 2)
GO
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 3)
GO
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 4)
GO
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 5)
GO
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 6)
GO
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 7)
GO
