
CREATE TABLE [dbo].[TenAmDealCutOff_ClosingBalance](
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Date] [date] NULL,
	[Currency] [nvarchar](50) NULL,
	[Account] [nvarchar](150) NULL,
	[ClosingBalance] [float] NOT NULL,
	[ModifiedBy] [nvarchar](150) NULL,
	[ModifiedDate] [datetime] NULL
)
