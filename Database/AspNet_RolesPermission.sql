/****** Object:  Table [dbo].[AspNetRolePermissions]    Script Date: 3/10/2022 9:16:12 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

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

