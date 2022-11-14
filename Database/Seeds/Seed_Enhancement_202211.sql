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


-- App log

CREATE TABLE [App_Logs] (

   [Id] int IDENTITY(1,1) NOT NULL,
   [Message] nvarchar(max) NULL,
   [MessageTemplate] nvarchar(max) NULL,
   [Level] nvarchar(128) NULL,
   [TimeStamp] datetime NOT NULL,
   [Exception] nvarchar(max) NULL,
   [Properties] nvarchar(max) NULL

   CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED ([Id] ASC)
);