CREATE TABLE [dbo].[Form_Workflow](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 
	[FormId] INT NOT NULL,

    [RequestBy] NVARCHAR(50) NULL, 
	[RequestTo] NVARCHAR(50) NULL,

    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL,

	[WorkflowStatus] NVARCHAR(150) NULL,
	[WorkflowNotes] TEXT NULL
)