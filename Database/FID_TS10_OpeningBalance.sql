CREATE TABLE [dbo].[FID_TS10_OpeningBalance](
	[Id] INT NOT NULL Identity(1,1) PRIMARY KEY,
	[FormId] INT NOT NULL,

	[Account] NVARCHAR(250), -- RENTAS/MMA
	[Amount] DECIMAL(18, 2),

    [CreatedBy] NVARCHAR(250), 
    [CreatedDate] DATETIME,

	[FcaAccount] NVARCHAR(250),
	[FcaAmount] DECIMAL(18, 2),
	[AssignedBy] NVARCHAR(150), 
    [AssignedDate] DATETIME
)