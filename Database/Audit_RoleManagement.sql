

CREATE TABLE [dbo].[Audit_RoleManagement](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Activity] [nvarchar](150) NOT NULL,
	[Remarks] [nvarchar](150) NULL,
	[Role] [nvarchar](150) NOT NULL,
	[PerformedBy] [nvarchar](150) NOT NULL,
	[RecordedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO



