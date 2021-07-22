CREATE TABLE [dbo].[Config_Application](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY, 

	[Key] NVARCHAR(250) NOT NULL,
	[Value] NVARCHAR(MAX) NOT NULL,

    [CreatedBy] NVARCHAR(250) NULL, 
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(), 
    [UpdatedBy] NVARCHAR(250) NULL, 
    [UpdatedDate] DATETIME NULL
)

INSERT INTO [dbo].[Config_Application]([Key],[Value]) VALUES ('Amsd.InflowFunds.CutOffTime' ,'10:00');
INSERT INTO [dbo].[Config_Application]([Key],[Value]) VALUES ('Amsd.InflowFunds.Notification' ,'true');
INSERT INTO [dbo].[Config_Application]([Key],[Value]) VALUES ('Amsd.InflowFunds.AmsdEditNotification' ,'true');
