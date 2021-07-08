
CREATE TABLE [dbo].[Config_ApproverList](
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[ApproverUsername] [nvarchar](150) NULL,
	[FormType] [nvarchar](150) NULL,

	[CreatedBy] [nvarchar](150) NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedBy] [nvarchar](150) NULL,
	[UpdatedDate] [datetime] NULL
)