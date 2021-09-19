CREATE TABLE [dbo].[Config_FcaBankAccount](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 

	[AccountName1] NVARCHAR(250),
	[AccountName2] NVARCHAR(250),
	[AccountName3] NVARCHAR(250),
	[AccountNo] NVARCHAR(250),
	[Currency] NVARCHAR(10),

    [CreatedBy] NVARCHAR(250), 
    [CreatedDate] DATETIME, 
    [UpdatedBy] NVARCHAR(250), 
    [UpdatedDate] DATETIME
)


INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'Maybank MFCA', N'MAYBANK101', N'MBBFCA', N'714011030452', N'USD', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'Maybank MFCA', N'MAYBANK101', N'MBBFCA', N'714011030452', N'GBP', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'Maybank MFCA', N'MAYBANK101', N'MBBFCA', N'714011030452', N'AUD', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'Maybank MFCA', N'MAYBANK101', N'MBBFCA', N'714011030452', N'EUR', N'System', '2021-09-20')

INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'CITI MFCA', N'CITIBANK101', N'CITIFCA', N'116435004', N'USD', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'CITI MFCA', N'CITIBANK101', N'CITIFCA', N'116435012', N'GBP', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'CITI MFCA', N'CITIBANK101', N'CITIFCA', N'116435039', N'AUD', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'CITI MFCA', N'CITIBANK101', N'CITIFCA', N'116435047', N'EUR', N'System', '2021-09-20')

INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'Hong Leong Bank MFCA', N'HLBB101', N'HLBFCA', N'39502002088', N'USD', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'Hong Leong Bank MFCA', N'HLBB101', N'HLBFCA', N'39506000897', N'GBP', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'Hong Leong Bank MFCA', N'HLBB101', N'HLBFCA', N'39503001435', N'AUD', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'Hong Leong Bank MFCA', N'HLBB101', N'HLBFCA', N'39505000544', N'EUR', N'System', '2021-09-20')

INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'JP Morgan MFCA', N'JPMORG101', N'JPMORGFCA', N'76953929', N'USD', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'JP Morgan MFCA', N'JPMORG101', N'JPMORGFCA', N'76953931', N'GBP', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'JP Morgan MFCA', N'JPMORG101', N'JPMORGFCA', N'76953934', N'AUD', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'JP Morgan MFCA', N'JPMORG101', N'JPMORGFCA', N'76953930', N'EUR', N'System', '2021-09-20')


INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'CIMB FCA', N'CIMB101', N'CIMBFCA', N'800001076540', N'USD', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'CIMB FCA', N'CIMB101', N'CIMBFCA', N'800008399331', N'GBP', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'CIMB FCA', N'CIMB101', N'CIMBFCA', N'800010467850', N'AUD', N'System', '2021-09-20')
INSERT [dbo].[Config_FcaBankAccount] ([AccountName1], [AccountName2], [AccountName3], [AccountNo], [Currency], [CreatedBy], [CreatedDate]) 
VALUES (N'CIMB FCA', N'CIMB101', N'CIMBFCA', N'800014611730', N'EUR', N'System', '2021-09-20')



