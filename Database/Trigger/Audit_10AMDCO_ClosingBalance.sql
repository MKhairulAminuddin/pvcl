USE [KashflowDb]
GO

/****** Object:  Trigger [dbo].[Audit_ClosingBalance]    Script Date: 1/8/2022 6:48:01 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER [dbo].[Audit_ClosingBalance] 
   ON  [dbo].[TenAmDealCutOff_ClosingBalance]
   FOR INSERT, UPDATE
AS 
BEGIN
	SET NOCOUNT ON;

	--Determine if this is an INSERT,UPDATE, or DELETE Action or a "failed delete".
	DECLARE @Action as char(1);
    SET @Action = (
		CASE 
			WHEN EXISTS(SELECT * FROM INSERTED) AND EXISTS(SELECT * FROM DELETED) THEN 'U'  -- Set Action to Updated.
			WHEN EXISTS(SELECT * FROM INSERTED) THEN 'I'  -- Set Action to Insert.
            WHEN EXISTS(SELECT * FROM DELETED) THEN 'D'  -- Set Action to Deleted.
            ELSE NULL -- Skip. It may have been a "failed delete".   
        END
	)

	IF @Action = 'I'
	BEGIN
		INSERT INTO [Audit_10AMDCO_ClosingBalance]
        (
            [ReportDate],
            [Currency],
            [Account],
            [ValueBefore],
            [ValueAfter],
			[Operation],
            [ModifiedBy],
            [ModifiedOn]
        )
		SELECT
			i.Date,
			i.Currency,
			i.Account,
			0,
			i.ClosingBalance,
			'INS',
			i.ModifiedBy,
			GETDATE()
		FROM
			inserted i
	END
	IF @Action = 'U'
	BEGIN
		INSERT INTO [Audit_10AMDCO_ClosingBalance]
        (
            [ReportDate],
            [Currency],
            [Account],
            [ValueBefore],
            [ValueAfter],
			[Operation],
            [ModifiedBy],
            [ModifiedOn]
        )
		SELECT
			i.Date,
			i.Currency,
			i.Account,
			d.ClosingBalance,
			i.ClosingBalance,
			'UPD',
			i.ModifiedBy,
			GETDATE()
		FROM
			inserted i
		JOIN deleted d on i.id = d.id
	END

END
GO

ALTER TABLE [dbo].[TenAmDealCutOff_ClosingBalance] ENABLE TRIGGER [Audit_ClosingBalance]
GO


