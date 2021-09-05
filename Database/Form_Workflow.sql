CREATE TABLE [dbo].[Form_Workflow](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 
	[FormId] INT NOT NULL,
	[FormType] NVARCHAR(150), 

    [RequestBy] NVARCHAR(50), 
	[RequestTo] NVARCHAR(50),

    [RecordedDate] DATETIME, 

	[WorkflowStatus] NVARCHAR(150),
	[WorkflowNotes] TEXT
)