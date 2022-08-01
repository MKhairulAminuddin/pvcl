
CREATE TABLE [dbo].[Audit_10AMDCO_ClosingBalance](
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,

	[ReportDate] [date],

	[Currency] [nvarchar](10),
	[Account] [nvarchar](150),

	[ValueBefore] [float] not null,
	[ValueAfter] [float] not null,
	[Operation]  [nvarchar](10),
	[ModifiedBy] [nvarchar](50),
	[ModifiedOn] [datetime]
)




