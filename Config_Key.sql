﻿CREATE TABLE [dbo].[Config_Key](
	[Key] NVARCHAR(250) NOT NULL PRIMARY KEY,
	[KeyType] NVARCHAR(250) NOT NULL,
)


INSERT INTO [dbo].[Config_Key]([Key],[KeyType]) VALUES ('Amsd.InflowFunds.Bank' ,'Dropdown');
INSERT INTO [dbo].[Config_Key]([Key],[KeyType]) VALUES ('Amsd.InflowFunds.FundType' ,'Dropdown');
