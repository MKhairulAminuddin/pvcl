CREATE TABLE [dbo].[Iisd.TradeSettlement](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY,
	[FormId] INT NOT NULL,

	[InstrumentType] NVARCHAR(MAX) NOT NULL,
	[InstrumentCode] NVARCHAR(MAX),
	[StockCode] NVARCHAR(MAX),

	[Maturity] DECIMAL(18, 2),
	[Sales] DECIMAL(18, 2),
	[Purchase] DECIMAL(18, 2),
	[FirstLeg] DECIMAL(18, 2),
	[SecondLeg] DECIMAL(18, 2),
	[AmountPlus] DECIMAL(18, 2),
	[AmountMinus] DECIMAL(18, 2),

    [CreatedBy] NVARCHAR(250), 
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(), 
    [UpdatedBy] NVARCHAR(250), 
    [UpdatedDate] DATETIME
)