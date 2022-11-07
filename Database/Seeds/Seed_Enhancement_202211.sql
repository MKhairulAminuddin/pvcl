-- Add column for manual calculation of P + I only for Treasury Deposit table
ALTER TABLE FID_Treasury_Deposit
ADD ManualCalc_P_Plus_I bit NOT NULL DEFAULT '0'


-- For email notification setting

INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.CnEmail', N'fahrulradzi.s@kwap.gov.my;shaiful.amri@kwap.gov.my', GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.CnEmail.Cc', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.PeEmail',  NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.PeEmail.Cc',  NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.PropertyEmail', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.PropertyEmail.Cc', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.LoanEmail', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.TS.LoanEmail.Cc', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.FcaTagging', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.ISSD.T.TreasuryApproval', NULL, GETDATE())

INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.CnEmail', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.CnEmail.Cc', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.PeEmail', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.PeEmail.Cc', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.PropertyEmail', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.PropertyEmail.Cc', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.LoanEmail', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.LoanEmail.Cc', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.FcaTagging', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.ISSD.TreasuryApproval', NULL, GETDATE())

INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.FID.T.TreasurySubmission.Cc', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.FID.T.TreasuryApproval.Cc', NULL, GETDATE())

INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.FID.T.TreasurySubmission.Cc', NULL, GETDATE())
INSERT [dbo].[Config_Application] ([Key], [Value], [CreatedDate]) VALUES (N'Noti.Enable.FID.T.TreasuryApproval.Cc', NULL, GETDATE())