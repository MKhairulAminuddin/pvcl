CREATE TABLE [dbo].[Form_Header](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 

    [FormType] NVARCHAR(150) NULL, 
	[FormStatus] NVARCHAR(150) NULL, 
    [Currency] NVARCHAR(150) NULL, 

    [PreparedBy] NVARCHAR(150) NULL, 
    [PreparedDate] DATETIME NULL DEFAULT GETDATE(), 

    [ApprovedBy] NVARCHAR(150) NULL, 
    [ApprovedDate] DATETIME NULL,

	[AdminEditted] BIT NOT NULL DEFAULT 0, 
	[AdminEdittedBy] NVARCHAR(150) NULL, 
    [AdminEdittedDate] DATETIME NULL DEFAULT GETDATE(), 
)

CREATE TABLE [dbo].[Amsd.InflowFunds](
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