-- Add column for manual calculation of P + I only for Treasury Deposit table
ALTER TABLE FID_Treasury_Deposit
ADD ManualCalc_P_Plus_I bit NOT NULL DEFAULT '0'


-- For email notification setting

INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.CnEmail', N'fahrulradzi.s@kwap.gov.my;shaiful.amri@kwap.gov.my', GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.CnEmail.Cc', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.PeEmail',  NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.PeEmail.Cc',  NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.PropertyEmail', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.PropertyEmail.Cc', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.LoanEmail', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.LoanEmail.Cc', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.FcaTagging', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.TreasuryApproval', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.CnEmail.Enable', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.CnEmail.Cc.Enable', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.PeEmail.Enable', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.PeEmail.Cc.Enable', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.PropertyEmail.Enable', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.PropertyEmail.Cc.Enable', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.LoanEmail.Enable', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.LoanEmail.Cc.Enable', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.FcaTagging.Enable', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Notify.TS.TreasuryApproval.Enable', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'FID.T.TreasurySubmission.Cc', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'FID.T.TreasuryApproval.Cc', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'FID.T.TreasurySubmission.Cc.Enable', NULL, GETDATE())
GO
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'FID.T.TreasuryApproval.Cc.Enable', NULL, GETDATE())
GO