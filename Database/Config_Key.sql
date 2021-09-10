CREATE TABLE [dbo].[Config_Key](
	[Key] NVARCHAR(250) NOT NULL PRIMARY KEY,
	[KeyType] NVARCHAR(250) NOT NULL,
)


INSERT INTO [dbo].[Config_Key]([Key],[KeyType]) VALUES ('Amsd.InflowFunds.Bank' ,'Dropdown');
INSERT INTO [dbo].[Config_Key]([Key],[KeyType]) VALUES ('Amsd.InflowFunds.FundType' ,'Dropdown');
INSERT INTO [dbo].[Config_Key]([Key],[KeyType]) VALUES ('Amsd.InflowFunds.AmsdEditNotification' ,'Application');
INSERT INTO [dbo].[Config_Key]([Key],[KeyType]) VALUES ('Amsd.InflowFunds.CutOffTime' ,'Application');
INSERT INTO [dbo].[Config_Key]([Key],[KeyType]) VALUES ('Amsd.InflowFunds.Notification' ,'Application');


INSERT INTO [dbo].[Config_Key]([Key],[KeyType]) VALUES ('ISSD.TradeSettlement.Currency' ,'Dropdown');

INSERT INTO [dbo].[Config_Key]([Key],[KeyType]) VALUES ('FID.Treasury.Notes' ,'Dropdown');
INSERT INTO [dbo].[Config_Key]([Key],[KeyType]) VALUES ('FID.Treasury.AssetType' ,'Dropdown');
INSERT INTO [dbo].[Config_Key]([Key],[KeyType]) VALUES ('FID.Treasury.ProductType' ,'Dropdown');