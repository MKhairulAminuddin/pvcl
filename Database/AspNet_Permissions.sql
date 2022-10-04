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
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (5, N'Administration - Roles Management', 2, 1)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (6, N'Administration - Application Config', 2, 1)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (7, N'Administration - Task Scheduler', 2, 1)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (8, N'Administration - Utility', 2, 1)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (9, N'Audit Trail', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (10, N'Audit Trail - Form Audit', 2, 9)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (11, N'Audit Trail - User Access Audit', 2, 9)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (12, N'Settings', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (13, N'Settings - Dropdown Data', 2, 12)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (14, N'Settings - Email Notification', 2, 12)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (15, N'Settings - Approver Assignment', 2, 12)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (16, N'Treasury Form', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (17, N'Treasury Form - Creator', 2, 16)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (18, N'Treasury Form - View', 2, 16)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (19, N'Treasury Form - Download', 2, 16)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (20, N'Treasury Form - Workflow', 2, 16)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (22, N'Trade Settlement Form', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (24, N'Trade Settlement Form - Creator', 2, 22)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (25, N'Trade Settlement Form - View', 2, 22)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (26, N'Trade Settlement Form - Download', 2, 22)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (27, N'Trade Settlement Form - Workflow', 2, 22)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (29, N'Inflow Fund Form', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (30, N'Inflow Fund Form - Creator', 2, 29)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (31, N'Inflow Fund Form - View', 2, 29)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (32, N'Inflow Fund Form - Download', 2, 29)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (33, N'Inflow Fund Form - Workflow', 2, 29)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (35, N'FCA Tagging', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (36, N'FCA Tagging - View', 2, 35)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (37, N'FCA Tagging - Tag', 2, 35)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (39, N'Report', 1, 0)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (40, N'Report - Deal Cut Off MYR', 2, 39)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (41, N'Report - Deal Cut Off FCY', 2, 39)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (42, N'Report - Deal Cut Off 10 AM', 2, 39)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (43, N'Administration - System Information', 2, 1)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (44, N'Report - Deal Cut Off 10 AM - Edit Closing Balance', 2, 39)
GO
SET IDENTITY_INSERT [dbo].[AspNetPermission] OFF
GO
