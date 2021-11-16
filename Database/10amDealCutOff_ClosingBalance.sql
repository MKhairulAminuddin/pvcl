SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TenAmDealCutOff_ClosingBalance](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [date] NULL,
	[Currency] [nvarchar](50) NULL,
	[Account] [nvarchar](150) NULL,
	[ClosingBalance] [float] NOT NULL,
	[ModifiedBy] [nvarchar](150) NULL,
	[ModifiedDate] [datetime] NULL,
 CONSTRAINT [PK__10amDeal] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

