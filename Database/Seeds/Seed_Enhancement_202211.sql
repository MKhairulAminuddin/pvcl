--#region Create New Permission Tables

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


INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (38, N'FID Dashboard', 2, 17)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (39, N'ISSD Dashboard', 2, 23)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (40, N'AMSD Dashboard', 2, 29)
GO

INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (41, N'Trade Settlement Form - Admin Edit', 2, 23)
GO
INSERT [dbo].[AspNetPermission] ([Id], [PermissionName], [PermissionLevel], [Parent]) VALUES (42, N'Inflow Fund Form - Admin Edit', 2, 29)
GO


SET IDENTITY_INSERT [dbo].[AspNetPermission] OFF
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


INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 1)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 2)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 3)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 4)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 5)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 6)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 7)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 8)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 9)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 10)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 11)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 12)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 13)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 14)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 15)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 16)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 17)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 18)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 19)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 20)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 21)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 22)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 23)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 24)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 25)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 26)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 27)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 28)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 29)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 30)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 31)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 32)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 33)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 34)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 35)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 36)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1, 37)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 8)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 9)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 13)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 14)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 15)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 16)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 17)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 18)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 19)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 20)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 21)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 22)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 23)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 24)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 26)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 29)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 30)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 32)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 33)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 34)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 35)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 36)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (2, 37)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (3, 29)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (3, 30)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (3, 31)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (3, 32)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (4, 23)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (4, 24)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (4, 25)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (4, 26)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (4, 27)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (4, 28)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (4, 33)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (4, 34)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (4, 35)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (4, 36)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (4, 37)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (5, 17)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (5, 18)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (5, 19)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (5, 20)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (5, 21)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (5, 22)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (5, 33)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (5, 34)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (5, 35)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (5, 36)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (5, 37)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (10, 33)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (10, 34)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 4)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 5)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 6)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 7)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 8)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 9)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 10)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 11)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 12)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 13)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 14)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 15)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 16)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 18)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 21)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 24)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 27)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 30)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 34)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 35)
INSERT [dbo].[AspNetRolePermissions] ([RoleId], [PermissionId]) VALUES (1011, 36)




-- Add column for manual calculation of P + I only for Treasury Deposit table
ALTER TABLE FID_Treasury_Deposit
ADD ManualCalc_P_Plus_I bit NOT NULL DEFAULT '0'


-- For email notification setting

INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.CnEmail', N'eft.contribution@kwap.gov.my', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.CnEmail.Cc', N'issd@kwap.gov.my,fid.treasury@kwap.gov.my', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.PeEmail',  N'altid.pe@kwap.gov.my', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.PeEmail.Cc',  N'settlement_ops@kwap.gov.my', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.PropertyEmail', N'property@kwap.gov.my', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.PropertyEmail.Cc', N'investmentsupport@kwap.gov.my', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.LoanEmail', N'raihan@kwap.gov.my,khairul@kwap.gov.my,afizah@kwap.gov.my', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.LoanEmail.Cc', N'settlement_ops@kwap.gov.my', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.FcaTagging', N'settlement_ops@kwap.gov.my', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.T.TreasuryApproval', N'settlement_ops@kwap.gov.my', GETDATE())

INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.TS.CnEmail', 'true', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.TS.CnEmail.Cc', 'true', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.TS.PeEmail', 'true', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.TS.PeEmail.Cc', 'true', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.TS.PropertyEmail', 'false', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.TS.PropertyEmail.Cc', 'false', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.TS.LoanEmail', 'true', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.TS.LoanEmail.Cc', 'true', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.FcaTagging', 'true', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.T.TreasuryApproval', 'true', GETDATE())

INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.FID.IF.Approved', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.FID.T.Submitted.Cc', N'fid.treasury@kwap.gov.my', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.FID.T.Approved.Cc', N'fid.treasury@kwap.gov.my', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.FID.TS.Approved.Cc', N'fid.treasury@kwap.gov.my', GETDATE())

INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.FID.IF.Approved', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.FID.T.Submitted.Cc', 'true', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.FID.T.Approved.Cc', 'true', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.FID.TS.Approved.Cc', 'true', GETDATE())


-- New Audit Tables

CREATE TABLE [dbo].[Audit_RoleManagement](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Activity] [nvarchar](150) NOT NULL,
	[Remarks] [nvarchar](150) NULL,
	[Role] [nvarchar](150) NOT NULL,
	[PerformedBy] [nvarchar](150) NOT NULL,
	[RecordedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


CREATE TABLE [dbo].[Audit_UserManagement](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Activity] [nvarchar](150) NOT NULL,
	[Remarks] [nvarchar](150) NULL,
	[UserAccount] [nvarchar](150) NOT NULL,
	[PerformedBy] [nvarchar](150) NOT NULL,
	[RecordedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

exec sp_rename 'Log_UserAccess', 'Audit_UserAccess'
GO



-- fix blank Dealer data in FID Treasury form
update FID_Treasury_Deposit set Dealer = ModifiedBy
where Dealer is null

update FID_Treasury_MMI set Dealer = ModifiedBy
where Dealer is null


-- fix blank trade date due to system bugs

update FID_Treasury_MMI set TradeDate = ModifiedDate where TradeDate is null