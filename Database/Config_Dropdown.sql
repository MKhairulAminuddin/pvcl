CREATE TABLE [dbo].[Config_Dropdown](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 

	[Key] NVARCHAR(250) NOT NULL,
	[Value] NVARCHAR(MAX) NOT NULL,

    [CreatedBy] NVARCHAR(250) NULL, 
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(), 
    [UpdatedBy] NVARCHAR(250) NULL, 
    [UpdatedDate] DATETIME NULL
)

INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.Bank', 'RHB Bank -01', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.Bank', 'RHB Bank -02', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.Bank', 'RHB Bank -03', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.Bank', 'BNM-01', 'System');

INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.FundType', 'Funds Receive', 'System');

INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('ISSD.TradeSettlement.Currency', 'MYR', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('ISSD.TradeSettlement.Currency', 'USD', 'System');

INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.Notes', 'w/d P+I', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.Notes', 'r/o P+I', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.Notes', 'New', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.Notes', 'Early Withdrawal', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.Notes', 'Accrued Interest', 'System');

INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.AssetType', 'MMD', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.AssetType', 'FD', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.AssetType', 'CMD', 'System');

INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.ProductType', 'NID', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.ProductType', 'NIDC', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.ProductType', 'NIDL', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.ProductType', 'CP', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.ProductType', 'ICP', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.ProductType', 'BA', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.ProductType', 'AB-i', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.ProductType', 'BNMN', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.ProductType', 'BNMN-i', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.ProductType', 'MTB', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('FID.Treasury.ProductType', 'MTIB', 'System');