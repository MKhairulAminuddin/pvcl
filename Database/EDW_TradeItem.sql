CREATE TABLE [dbo].[EDW_TradeItem](
	[InstrumentType] [nvarchar](max) NULL,
	[InstrumentName] [nvarchar](max) NULL,
	[StockCode] [nvarchar](max) NULL,
	[ISIN] [nvarchar](max) NULL,
	[Type] [varchar](100) NULL,
	[Amount] [float] NULL,
	[TradeDate] [date] NULL,
	[SettlementDate] [date] NULL,
	[Currency] [varchar](3) NULL,
	[CreatedBy] [nvarchar](250) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedBy] [nvarchar](250) NULL,
	[UpdatedDate] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]