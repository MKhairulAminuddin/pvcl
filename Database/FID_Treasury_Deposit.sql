CREATE TABLE [dbo].[FID_Treasury_Deposit](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 
	[FormId] INT NOT NULL, 

    [CashflowType] NVARCHAR(50), -- inflow/outflow

    [Dealer] NVARCHAR(50), 
	[Bank] NVARCHAR(250), 
    [TradeDate] DATE,
    [ValueDate] DATE, 
    [MaturityDate] DATE, 
    [Tenor] INT,
    [Principal] DECIMAL(18,4),
    [RatePercent] DECIMAL(18,4),
    [IntProfitReceivable] DECIMAL(18,4),
    [PrincipalIntProfitReceivable] DECIMAL(18,4),
    [AssetType] NVARCHAR(50),
    [RepoTag] NVARCHAR(50),
    [ContactPerson] NVARCHAR(100),
    [Notes] NVARCHAR(50),

    [ModifiedBy] NVARCHAR(150), 
    [ModifiedDate] DATETIME
);

update [FID_Treasury_Deposit] set TradeDate = FID_Treasury.PreparedDate
from [FID_Treasury_Deposit]
join FID_Treasury on FormId= FID_Treasury.Id