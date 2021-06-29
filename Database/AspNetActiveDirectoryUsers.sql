
CREATE TABLE [dbo].[AspNetActiveDirectoryUsers](
	[Username] [nvarchar](256) PRIMARY KEY	NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[DisplayName] [nvarchar](256),
	[Title] [nvarchar](256),
	[Department] [nvarchar](256),
	[TelNo] [nvarchar](256),
	[Office] [nvarchar](256),
	[AdType] [nvarchar](256),
	[DistinguishedName] [nvarchar](256),
	
	[AdAccountCreated] [datetime],
	[AdAccountChanged] [datetime],
	[LastBadPasswordAttempt] [datetime],
	[LastBadPasswordAttempt] [datetime],
	[LastLogon] [datetime],
	[LastPasswordSet] [datetime]
)