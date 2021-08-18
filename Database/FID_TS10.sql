CREATE TABLE [dbo].[FID_TS10](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 

    [FormType] NVARCHAR(150) NULL, 
	[FormStatus] NVARCHAR(150) NULL, 
    [Currency] NVARCHAR(150) NULL, 

    [SettlementDate] DATETIME NULL
)
