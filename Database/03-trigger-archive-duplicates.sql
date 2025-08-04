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
    MessageText         NVARCHAR(MAX),
    MsgType             CHAR(1),
    BotKey              NVARCHAR(100),
    ScheduledSendDateTime DATETIME,
    ReceivedDateTime    DATETIME,
    MessageHash         BINARY(32),
    Priority            SMALLINT,
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
    i.MessageText,
    i.MsgType,
    i.BotKey,
    i.ScheduledSendDateTime,
    i.ReceivedDateTime,
    i.MessageHash,
    i.Priority,
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
    ,CampaignId
    ,MessageText
    ,MsgType
    ,BotKey
    ,ScheduledSendDateTime
    ,ReceivedDateTime
    ,GatewayDateTime
    ,MessageHash
    ,Priority
    ,CampDescription
    ,IsSystemApproved
    ,Paused)  
   SELECT
    d.ID,
    d.CustId,
    d.ChatId,
    d.CampaignId,
    d.MessageText,
    d.MsgType,
    d.BotKey,
    d.ScheduledSendDateTime,
    d.ReceivedDateTime,
    GETDATE()                  AS GatewayDateTime,
    d.MessageHash,
    d.Priority,
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
