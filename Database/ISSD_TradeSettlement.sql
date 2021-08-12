CREATE TABLE [dbo].[ISSD_TradeSettlement](
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

	[Remarks] TEXT,

    [ModifiedBy] NVARCHAR(150) NULL, 
    [ModifiedDate] DATETIME NULL DEFAULT GETDATE()
)

CREATE TABLE [dbo].[ISSD_TradeSettlementHistory](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY,
	[FormId] INT NOT NULL,
	[VersionNo] DECIMAL(5,2) NOT NULL, 

	[InstrumentType] NVARCHAR(MAX),
	[InstrumentCode] NVARCHAR(MAX),
	[StockCode] NVARCHAR(MAX),

	[Maturity] DECIMAL(18, 2),
	[Sales] DECIMAL(18, 2),
	[Purchase] DECIMAL(18, 2),
	[FirstLeg] DECIMAL(18, 2),
	[SecondLeg] DECIMAL(18, 2),
	[AmountPlus] DECIMAL(18, 2),
	[AmountMinus] DECIMAL(18, 2),

	[Remarks] TEXT,

    [ModifiedBy] NVARCHAR(150) NULL, 
    [ModifiedDate] DATETIME NULL DEFAULT GETDATE()
)