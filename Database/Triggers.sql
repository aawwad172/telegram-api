CREATE OR ALTER TRIGGER dbo.trg_ReadyTable_ArchiveDuplicates
  ON dbo.ReadyTable
  AFTER INSERT
AS
BEGIN
  SET NOCOUNT ON;

  ----------------------------------------------------------------
  -- 1) NEW hashes: insert into RecentMessages only if not already there
  ----------------------------------------------------------------
  INSERT INTO dbo.RecentMessages (MessageHash, ReceivedDateTime, ReadyId)
  SELECT
    i.MessageHash,
    i.ReceivedDateTime,
    i.ID
  FROM inserted AS i
  LEFT JOIN dbo.RecentMessages AS rm
    ON i.MessageHash = rm.MessageHash
  WHERE rm.MessageHash IS NULL;     -- only those not seen before

  ----------------------------------------------------------------
  -- 2) DUPLICATE inserts: exactly those new rows whose hash
  --    was already in RecentMessages *before* they fired this trigger
  ----------------------------------------------------------------
  DECLARE @Dupes TABLE
  (
    ID                  INT,
    CustId          INT,
    ChatId              NVARCHAR(50),
    BotKey              NVARCHAR(100),
    PhoneNumber         NVARCHAR(20),
    MessageText         NVARCHAR(MAX),
    MsgType             NVARCHAR(10),
    ReceivedDateTime    DATETIME2,
    ScheduledSendDateTime DATETIME2,
    MessageHash         BINARY(32),
    Priority            SMALLINT,
    MobileCountry      NVARCHAR(10),
    CampaignId          NVARCHAR(50),
    CampDescription     NVARCHAR(512),
    IsSystemApproved    BIT,
    Paused              BIT
  );

  INSERT INTO @Dupes
  SELECT
    i.ID,
    i.CustId,
    i.ChatId,
    i.BotKey,
    i.PhoneNumber,
    i.MessageText,
    i.MsgType,
    i.ReceivedDateTime,
    i.ScheduledSendDateTime,
    i.MessageHash,
    i.Priority,
    A2A_iMessaging.dbo.GetCountryCode(i.PhoneNumber),
    i.CampaignId,
    i.CampDescription,
    i.IsSystemApproved,
    i.Paused
  FROM inserted AS i
  INNER JOIN dbo.RecentMessages AS rm
    ON i.MessageHash = rm.MessageHash
  WHERE EXISTS (
    -- ensure that the matching rm row is *not* the one we just inserted above,
    -- i.e. it existed prior to this batch. We compare times:
    SELECT 1
      FROM dbo.RecentMessages AS old
     WHERE old.MessageHash = i.MessageHash
       AND old.ReceivedDateTime   < i.ReceivedDateTime
  );

  ----------------------------------------------------------------
  -- 3) Archive those duplicates
  ----------------------------------------------------------------
  INSERT INTO dbo.ArchiveTable
        (ID
    ,CustId
    ,ChatId
    ,BotKey
    ,PhoneNumber
    ,MessageText
    ,MsgType
    ,ReceivedDateTime
    ,ScheduledSendDateTime
    ,GatewayDateTime
    ,MessageHash
    ,Priority
    ,StatusId
    ,MobileCountry
    ,CampaignId
    ,CampDescription
    ,IsSystemApproved
    ,Paused)  
   SELECT
    d.ID,
    d.CustId,
    d.ChatId,
    d.BotKey,
    d.PhoneNumber,
    d.MessageText,
    d.MsgType,
    d.ReceivedDateTime,
    d.ScheduledSendDateTime,
    GETDATE()                  AS GatewayDateTime,
    d.MessageHash,
    d.Priority,
    -3, -- Duplicate Status
    d.MobileCountry,
    d.CampaignId,
    d.CampDescription,
    d.IsSystemApproved,
    d.Paused
  FROM @Dupes AS d;

  ----------------------------------------------------------------
  -- 4) Remove only those same duplicate rows from ReadyTable
  ----------------------------------------------------------------
  DELETE r
  FROM dbo.ReadyTable AS r
  INNER JOIN @Dupes       AS d
    ON r.ID = d.ID;
END;
GO


/* ============================================
   2) (Recommended) Keep StatusDescription in sync
   - If archiving SP always stamps StatusDescription, you can skip this.
   - Keeping it ensures correctness if someone inserts directly.
   ============================================ */
CREATE OR ALTER TRIGGER dbo.trg_ArchiveTable_StatusDescription
ON dbo.ArchiveTable
AFTER INSERT, UPDATE
AS
BEGIN
  SET NOCOUNT ON;

  UPDATE a
     SET a.StatusDescription = ms.StatusDescription
  FROM dbo.ArchiveTable a
  JOIN inserted i              ON a.ID = i.ID
  JOIN dbo.MessageStatus ms    ON ms.StatusID = a.StatusId
  WHERE a.StatusDescription IS NULL
     OR a.StatusDescription <> ms.StatusDescription;
END
GO
