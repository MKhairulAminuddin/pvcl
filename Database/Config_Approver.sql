CREATE TABLE [dbo].[Config_Approver](
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Username] [nvarchar](150) NULL,

	[Email] [nvarchar](256),
	[DisplayName] [nvarchar](256),
	[Title] [nvarchar](256),
	[Department] [nvarchar](256),

	[FormType] [nvarchar](150) NULL,

	[CreatedBy] [nvarchar](150) NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedBy] [nvarchar](150) NULL,
	[UpdatedDate] [datetime] NULL
)