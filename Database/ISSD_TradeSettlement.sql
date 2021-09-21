CREATE TABLE [dbo].[ISSD_TradeSettlement](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[FormId] [int] NOT NULL,
	[InstrumentType] [nvarchar](max) NOT NULL,
	[InstrumentCode] [nvarchar](max) NULL,
	[StockCode] [nvarchar](max) NULL,
	[Maturity] [float] NOT NULL,
	[Sales] [float] NOT NULL,
	[Purchase] [float] NOT NULL,
	[FirstLeg] [float] NOT NULL,
	[SecondLeg] [float] NOT NULL,
	[AmountPlus] [float] NOT NULL,
	[AmountMinus] [float] NOT NULL,
	[Remarks] [text] NULL,

	[ModifiedBy] [nvarchar](150) NULL,
	[ModifiedDate] [datetime] NULL DEFAULT GETDATE(),

	[InflowAmount] [float] NOT NULL,
	[OutflowAmount] [float] NOT NULL,

	[InflowTo] [nvarchar](150) NULL,
	[OutflowFrom] [nvarchar](150) NULL,
	[AssignedBy] [nvarchar](150) NULL,
	[AssignedDate] [datetime] NULL,
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