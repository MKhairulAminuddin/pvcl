CREATE TABLE [dbo].[ISSD_TradeSettlement](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY,
	[FormId] INT NOT NULL,

	[SettlementDate] DATETIME, 
	[BalanceType] NVARCHAR(250),
	[Amount] DECIMAL(18, 2),

    [CreatedBy] NVARCHAR(250), 
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(), 
    [UpdatedBy] NVARCHAR(250), 
    [UpdatedDate] DATETIME
)