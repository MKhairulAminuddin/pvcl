
CREATE TABLE [dbo].[Audit_10AMDCO_ClosingBalance](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
	[FormDate] [date],
	[Currency] [nvarchar](10),
	[Account] [nvarchar](150),
	[ValueBefore] [int],
	[ValueAfter] [int],
	[ModifiedBy] [nvarchar](50),
	[ModifiedOn] [datetime]
)


