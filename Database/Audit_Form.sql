
CREATE TABLE [dbo].[Audit_Form](
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[FormId] [int] NOT NULL,
	[FormType] [nvarchar](150) NULL,

	[ActionType] [nvarchar](150) NULL,
	[Remarks] [text] NULL,

	[ValueBefore] [nvarchar](150) NULL,
	[ValueAfter] [nvarchar](150) NULL,

	[ModifiedBy] [nvarchar](50) NULL,
	[ModifiedOn] [datetime] NULL
)

