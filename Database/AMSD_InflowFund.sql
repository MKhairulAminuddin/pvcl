CREATE TABLE [dbo].[AMSD_InflowFund](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 
    [FormId] INT NOT NULL, 

	[FundType] NVARCHAR(250) NULL,
	[Bank] NVARCHAR(150) NULL,
	[Amount] DECIMAL(18, 2) NULL,

	[LatestSubmission] DATETIME NULL,

    [CreatedBy] NVARCHAR(50) NULL, 
    [CreatedDate] DATETIME NULL DEFAULT GETDATE(), 
    [UpdatedBy] NVARCHAR(50) NULL, 
    [UpdatedDate] DATETIME NULL
)


CREATE TABLE [dbo].[AmsdInflowFundsHistory](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 
    [FormHeaderId] INT NOT NULL, 

	[LatestSubmission] DATETIME NULL,

    [CreatedBy] NVARCHAR(50) NULL, 
    [CreatedDate] DATETIME NULL DEFAULT GETDATE(), 
    [UpdatedBy] NVARCHAR(50) NULL, 
    [UpdatedDate] DATETIME NULL
)