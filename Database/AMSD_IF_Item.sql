CREATE TABLE [dbo].[AMSD_IF_Item](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 
    [FormId] INT NOT NULL, 

	[FundType] NVARCHAR(250) NULL,
	[Bank] NVARCHAR(150) NULL,
	[Amount] DECIMAL(18, 2) NULL,

	[LatestSubmission] DATETIME NULL,

    [ModifiedBy] NVARCHAR(150), 
    [ModifiedDate] DATETIME
)