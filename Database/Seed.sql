update ISSD_TradeSettlement set AmountPlus = 0
where AmountPlus is null
update ISSD_TradeSettlement set AmountMinus = 0
where AmountMinus is null
update ISSD_TradeSettlement set Sales = 0
where Sales is null
update ISSD_TradeSettlement set Maturity = 0
where Maturity is null
update ISSD_TradeSettlement set Purchase = 0
where Purchase is null
update ISSD_TradeSettlement set SecondLeg = 0
where SecondLeg is null
update ISSD_TradeSettlement set FirstLeg = 0
where FirstLeg is null


ALTER TABLE ISSD_TradeSettlement ALTER COLUMN AmountPlus [float] NOT NULL
ALTER TABLE ISSD_TradeSettlement ALTER COLUMN AmountMinus [float] NOT NULL
ALTER TABLE ISSD_TradeSettlement ALTER COLUMN Sales [float] NOT NULL
ALTER TABLE ISSD_TradeSettlement ALTER COLUMN Maturity [float] NOT NULL
ALTER TABLE ISSD_TradeSettlement ALTER COLUMN Purchase [float] NOT NULL
ALTER TABLE ISSD_TradeSettlement ALTER COLUMN SecondLeg [float] NOT NULL
ALTER TABLE ISSD_TradeSettlement ALTER COLUMN FirstLeg [float] NOT NULL


update FID_TS10_TradeItem set AmountPlus = 0
where AmountPlus is null
update FID_TS10_TradeItem set AmountMinus = 0
where AmountMinus is null
update FID_TS10_TradeItem set Sales = 0
where Sales is null
update FID_TS10_TradeItem set Maturity = 0
where Maturity is null
update FID_TS10_TradeItem set Purchase = 0
where Purchase is null
update FID_TS10_TradeItem set SecondLeg = 0
where SecondLeg is null
update FID_TS10_TradeItem set FirstLeg = 0
where FirstLeg is null

ALTER TABLE FID_TS10_TradeItem ALTER COLUMN AmountPlus [float] NOT NULL
ALTER TABLE FID_TS10_TradeItem ALTER COLUMN AmountMinus [float] NOT NULL
ALTER TABLE FID_TS10_TradeItem ALTER COLUMN Sales [float] NOT NULL
ALTER TABLE FID_TS10_TradeItem ALTER COLUMN Maturity [float] NOT NULL
ALTER TABLE FID_TS10_TradeItem ALTER COLUMN Purchase [float] NOT NULL
ALTER TABLE FID_TS10_TradeItem ALTER COLUMN SecondLeg [float] NOT NULL
ALTER TABLE FID_TS10_TradeItem ALTER COLUMN FirstLeg [float] NOT NULL


update AMSD_IF_Item set Amount = 0
where Amount is null

ALTER TABLE AMSD_IF_Item ALTER COLUMN Amount [float] NOT NULL

update FID_Treasury_MMI set Nominal = 0
where Nominal is null
update FID_Treasury_MMI set Price = 0
where Price is null
update FID_Treasury_MMI set IntDividendReceivable = 0
where IntDividendReceivable is null
update FID_Treasury_MMI set Proceeds = 0
where Proceeds is null
update FID_Treasury_MMI set PurchaseProceeds = 0
where PurchaseProceeds is null

ALTER TABLE FID_Treasury_MMI ALTER COLUMN Nominal [float] NOT NULL
ALTER TABLE FID_Treasury_MMI ALTER COLUMN Price [float] NOT NULL
ALTER TABLE FID_Treasury_MMI ALTER COLUMN IntDividendReceivable [float] NOT NULL
ALTER TABLE FID_Treasury_MMI ALTER COLUMN Proceeds [float] NOT NULL
ALTER TABLE FID_Treasury_MMI ALTER COLUMN PurchaseProceeds [float] NOT NULL

update FID_Treasury_Deposit set Principal = 0
where Principal is null
update FID_Treasury_Deposit set IntProfitReceivable = 0
where IntProfitReceivable is null
update FID_Treasury_Deposit set PrincipalIntProfitReceivable = 0
where PrincipalIntProfitReceivable is null
update FID_Treasury_Deposit set RatePercent = 0
where RatePercent is null

ALTER TABLE FID_Treasury_Deposit ALTER COLUMN Principal [float] NOT NULL
ALTER TABLE FID_Treasury_Deposit ALTER COLUMN IntProfitReceivable [float] NOT NULL
ALTER TABLE FID_Treasury_Deposit ALTER COLUMN PrincipalIntProfitReceivable [float] NOT NULL
ALTER TABLE FID_Treasury_Deposit ALTER COLUMN RatePercent [float] NOT NULL