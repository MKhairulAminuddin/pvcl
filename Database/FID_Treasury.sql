CREATE TABLE [dbo].[FID_Treasury](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 

    [FormType] NVARCHAR(150), 
	[FormStatus] NVARCHAR(150), 
    [Currency] NVARCHAR(150), 
    [TradeDate] DATETIME,

	[PreparedBy] NVARCHAR(150), 
    [PreparedDate] DATETIME, 

    [ApprovedBy] NVARCHAR(150), 
    [ApprovedDate] DATETIME
)

CREATE TABLE [dbo].[FID_Treasury_Deposit](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 

    [CashflowType] NVARCHAR(50), -- inflow/outflow

    [Dealer] NVARCHAR(50), 
	[Bank] NVARCHAR(250), 
    [ValueDate] DATE, 
    [MaturityDate] DATE, 
    [Tenor] INT,
    [RatePercent] DECIMAL(18,2),
    [IntProfitReceivable] DECIMAL(18,2),
    [PrincipalIntProfitReceivable] DECIMAL(18,2),
    [AssetType] NVARCHAR(50),
    [RepoTag] NVARCHAR(50),
    [ContactPerson] NVARCHAR(100),
    [Notes] NVARCHAR(50),

    [ModifiedBy] NVARCHAR(150), 
    [ModifiedDate] DATETIME
)

CREATE TABLE [dbo].[FID_Treasury_Item](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 

    [CashflowType] NVARCHAR(50), -- inflow/outflow

    [Dealer] NVARCHAR(50), 
	[Issuer] NVARCHAR(250), 
    [ProductType] NVARCHAR(50), 
    [CounterParty] NVARCHAR(50), 
    [ValueDate] DATE, 
    [MaturityDate] DATE, 
    [HoldingDayTenor] INT,
    [Nominal] DECIMAL(18,2),
    [SellPurchaseRateYield] DECIMAL(18,2),
    [Price] DECIMAL(18,2),
    [IntDividendReceivable] DECIMAL(18,2),
    [Proceeds] DECIMAL(18,2),
    [CertNoStockCode] DECIMAL(18,2),

    [ModifiedBy] NVARCHAR(150), 
    [ModifiedDate] DATETIME
)
