CREATE TABLE [dbo].[AspNetActiveDirectoryUsers](
	[Username] [nvarchar](256) NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[DisplayName] [nvarchar](256) NULL,
	[Title] [nvarchar](256) NULL,
	[Department] [nvarchar](256) NULL,
	[TelNo] [nvarchar](256) NULL,
	[Office] [nvarchar](256) NULL,
	[AdType] [nvarchar](256) NULL,
	[DistinguishedName] [nvarchar](256) NULL,
	[AdAccountCreated] [datetime] NULL,
	[AdAccountChanged] [datetime] NULL,
	[LastBadPasswordAttempt] [datetime] NULL,
	[LastLogon] [datetime] NULL,
	[LastPasswordSet] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
