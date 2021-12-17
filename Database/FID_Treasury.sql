CREATE TABLE [dbo].[FID_Treasury](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 

    [FormType] NVARCHAR(150), 
	[FormStatus] NVARCHAR(150), 
    [Currency] NVARCHAR(150), 
    [ValueDate] DATETIME,

	[PreparedBy] NVARCHAR(150), 
    [PreparedDate] DATETIME, 

    [ApprovedBy] NVARCHAR(150), 
    [ApprovedDate] DATETIME
)

sp_rename 'FID_Treasury.TradeDate', 'ValueDate', 'COLUMN'