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
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.Bank', 'BA', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.Bank', 'CP Matured', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.Bank', 'Cagamas Int', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.Bank', 'Loan SPnb', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.Bank', 'Bon Int', 'System');


INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.FundType', 'Funds Receive', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.FundType', 'Maturity', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.FundType', 'Proceed', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('Amsd.InflowFunds.FundType', 'Interest Received', 'System');

INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('ISSD.TradeSettlement.Currency', 'MYR', 'System');
INSERT INTO [dbo].[Config_Dropdown] ([Key],[Value],[CreatedBy]) VALUES ('ISSD.TradeSettlement.Currency', 'USD', 'System');