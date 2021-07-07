CREATE TABLE [dbo].[Config_Application](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 

	[Key] NVARCHAR(250) NOT NULL,
	[Value] NVARCHAR(MAX) NOT NULL,

    [CreatedBy] NVARCHAR(250) NULL, 
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(), 
    [UpdatedBy] NVARCHAR(250) NULL, 
    [UpdatedDate] DATETIME NULL
)