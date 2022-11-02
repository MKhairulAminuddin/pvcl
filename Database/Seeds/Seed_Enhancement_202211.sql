-- Add column for manual calculation of P + I only for Treasury Deposit table
ALTER TABLE FID_Treasury_Deposit
ADD ManualCalc_P_Plus_I bit NOT NULL DEFAULT '0'


