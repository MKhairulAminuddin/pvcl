
USE [KashflowDb]
GO
/****** Object:  Table [dbo].[AspNetPermission]    Script Date: 3/10/2022 9:13:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetPermission](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PermissionName] [nvarchar](50) NOT NULL,
	[PermissionLevel] [int] NOT NULL,
	[Parent] [int] NOT NULL,
 CONSTRAINT [PK_AspNetPermission] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[AspNetPermission] ON 
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (1, N'Administration', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (2, N'Administration - User Management', 2, 1)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (3, N'Administration - Roles Management', 2, 1)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (4, N'Administration - Application Config', 2, 1)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (5, N'Administration - Task Scheduler', 2, 1)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (6, N'Administration - Utility', 2, 1)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (7, N'Administration - System Information', 2, 1)
GO



INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (8, N'Audit Trail', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (9, N'Audit Trail - Form Audit', 2, 8)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (10, N'Audit Trail - User Access Audit', 2, 8)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (11, N'Audit Trail - User Management Audit', 2, 8)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (12, N'Audit Trail - Role Management Audit', 2, 8)
GO



INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (13, N'Settings', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (14, N'Settings - Dropdown Data', 2, 13)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (15, N'Settings - Email Notification', 2, 13)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (16, N'Settings - Approver Assignment', 2, 13)
GO


INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (17, N'FID', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (18, N'Treasury Form - View', 2, 17)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (19, N'Treasury Form - Edit', 2, 17)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (20, N'Treasury Form - Download', 2, 17)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (21, N'FCA Tagging Form - View', 2, 17)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (22, N'FCA Tagging Form - Edit', 2, 17)
GO


INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (23, N'ISSD', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (24, N'Trade Settlement Form - View', 2, 23)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (25, N'Trade Settlement Form - Edit', 2, 23)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (26, N'Trade Settlement Form - Download', 2, 23)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (27, N'FCA Tagging Form - View', 2, 23)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (28, N'FCA Tagging Form - Edit', 2, 23)
GO



INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (29, N'AMSD', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (30, N'Inflow Fund Form - View', 2, 29)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (31, N'Inflow Fund Form - Edit', 2, 29)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (32, N'Inflow Fund Form - Download', 2, 29)
GO


INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (33, N'Report', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (34, N'Report - Deal Cut Off MYR', 2, 33)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (35, N'Report - Deal Cut Off FCY', 2, 33)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (36, N'Report - Deal Cut Off 10 AM', 2, 33)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (37, N'Report - Deal Cut Off 10 AM - Edit Closing Balance', 2, 33)
GO


SET IDENTITY_INSERT [dbo].[AspNetPermission] OFF
GO
