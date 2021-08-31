
CREATE TABLE [dbo].[FID_Treasury_MMI](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 
    [FormId] INT NOT NULL, 

    [CashflowType] NVARCHAR(50), -- inflow/outflow

    [Dealer] NVARCHAR(50), 
	[Issuer] NVARCHAR(250), 
    [ProductType] NVARCHAR(50), 
    [CounterParty] NVARCHAR(50), 
    [ValueDate] DATE, 
    [MaturityDate] DATE, 
    [HoldingDayTenor] INT,
    [Nominal] DECIMAL(18,4),
    [SellPurchaseRateYield] DECIMAL(18,4),
    [Price] DECIMAL(18,4),
    [IntDividendReceivable] DECIMAL(18,4),
    [Proceeds] DECIMAL(18,4),
    [PurchaseProceeds] DECIMAL(18,4),
    [CertNoStockCode] NVARCHAR(150),

    [ModifiedBy] NVARCHAR(150), 
    [ModifiedDate] DATETIME
)
