CREATE TABLE [dbo].[AMSD_IF](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 

    [FormType] NVARCHAR(150) NULL, 
	[FormStatus] NVARCHAR(150) NULL, 
    [Currency] NVARCHAR(150) NULL, 

    [FormDate] DATETIME NULL,

    [PreparedBy] NVARCHAR(150) NULL, 
    [PreparedDate] DATETIME NULL DEFAULT GETDATE(), 

    [ApprovedBy] NVARCHAR(150) NULL, 
    [ApprovedDate] DATETIME NULL,

	[AdminEditted] BIT NOT NULL DEFAULT 0, 
	[AdminEdittedBy] NVARCHAR(150) NULL, 
    [AdminEdittedDate] DATETIME NULL DEFAULT GETDATE(), 
)