CREATE TABLE [dbo].[FID_T10_Approval](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 
	[FormId] INT NOT NULL,
	[FormType] NVARCHAR(150) NULL, 

	[ApprovedBy] NVARCHAR(150), 
    [ApprovedDate] DATETIME
)