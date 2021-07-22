CREATE TABLE [dbo].[EDW_TradeItem](
	[InstrumentType] NVARCHAR(MAX),
	[InstrumentCode] NVARCHAR(MAX),
	[StockCode] NVARCHAR(MAX),

    [CreatedBy] NVARCHAR(250), 
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(), 
    [UpdatedBy] NVARCHAR(250), 
    [UpdatedDate] DATETIME
)