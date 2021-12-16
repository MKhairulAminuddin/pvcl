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
	OthersType NVARCHAR(10),
	CouponType NVARCHAR(10),
	BondType NVARCHAR(10)
)

ALTER TABLE [ISSD_TradeSettlement]
ADD OthersType NVARCHAR(10);

ALTER TABLE [ISSD_TradeSettlement]
ADD CouponType NVARCHAR(10);

ALTER TABLE [ISSD_TradeSettlement]
ADD BondType NVARCHAR(10);

update ISSD_TradeSettlement set CouponType = 'MGS' 
where InstrumentCode like 'MGS%' or InstrumentCode like 'GII%' or InstrumentCode like 'MGII%'
and InstrumentType = 'COUPON'

update ISSD_TradeSettlement set CouponType = 'PDS' 
where InstrumentCode not like 'MGS%' and InstrumentCode not like 'GII%' and InstrumentCode not like 'MGII%'
and InstrumentType = 'COUPON'

update ISSD_TradeSettlement set BondType = 'MGS' 
where (InstrumentCode like 'MGS%' or InstrumentCode like 'GII%' or InstrumentCode like 'MGII%') 
and InstrumentType = 'BOND'

update ISSD_TradeSettlement set BondType = 'PDS' 
where (InstrumentCode not like 'MGS%' and InstrumentCode not like 'GII%' and InstrumentCode not like 'MGII%')
and InstrumentType = 'BOND'