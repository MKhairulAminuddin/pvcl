CREATE TABLE [dbo].[App_Notification](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[UserId] [nvarchar](150) NOT NULL,
	
	[NotificationType] [nvarchar](150),
	[NotificationIconClass] [nvarchar](150),

	[IsRead] [bit] DEFAULT 0,

	[Title] [nvarchar](150),
	[ShortMessage] [text],
	[Message] [text],

	[CreatedOn] [datetime] NOT NULL DEFAULT GETDATE()
)