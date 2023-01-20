CREATE TABLE [dbo].[App_TsFcaTaggingQueue](
	[Id] [int] NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[FormId] [int] NOT NULL,
	[TradeId] [int] NOT NULL,

	[CreatedDate] [datetime] NULL,
)
GO

ALTER TABLE [dbo].[App_TsFcaTaggingQueue] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO