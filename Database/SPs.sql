/*******************************************
 * 2.1) usp_Enqueue
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_Enqueue
    @CustomerId  		INT,
    @ChatId      		NVARCHAR(50),
    @BotId      		INT,
    @MessageText 		NVARCHAR(MAX),
    @PhoneNumber      NVARCHAR(32),
    @MsgType     		NVARCHAR(10), 
    @CampaignId  		NVARCHAR(128),
    @CampDescription 	NVARCHAR(512),
    @Priority    		SMALLINT,
    @ScheduledSendDateTime DATETIME = NULL,  -- Auto inserted in case of one message
    @IsSystemApproved BIT
  --@Paused = 0 always zero and it will change when the portal change them
  AS
  BEGIN
    SET NOCOUNT ON; 
    SET XACT_ABORT ON;

    DECLARE @now DATETIME = GETDATE();
    DECLARE @hash BINARY(32) = HASHBYTES('SHA2_256', CONCAT(ISNULL(@ChatId,N''),N'|',@BotId,N'|',ISNULL(@MessageText,N'')));
    DECLARE @Pending SMALLINT = 1;
    DECLARE @Duplicate SMALLINT = 20;
    DECLARE @NotSubId SMALLINT = -2;
    DECLARE @isMissingChat BIT =
      CASE
        WHEN @ChatId IS NULL OR
            LEN(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(@ChatId,N''))), NCHAR(9), N''), NCHAR(10), N''), NCHAR(13), N'')) = 0
        THEN 1 ELSE 0 END;

    SET @ScheduledSendDateTime = ISNULL(@ScheduledSendDateTime, @now);


    BEGIN TRAN;
    DECLARE @isNew BIT=0;
      IF @isMissingChat = 0
      BEGIN
        INSERT dbo.Table_Telegram_RecentMessages(MessageHash, ReadyId, ReceivedDateTime)
        SELECT @hash, NULL, @now;
        IF @@ROWCOUNT = 1 SET @isNew = 1;  -- first writer wins
      END

    INSERT dbo.Table_Telegram_RecentMessages(MessageHash, ReadyId, ReceivedDateTime)
      SELECT @hash, NULL, @now;
    IF @@ROWCOUNT = 1 SET @isNew = 1;  -- first writer wins

  INSERT INTO dbo.Table_Telegram_ReadyTable
      (ChatId
      ,CustomerId
      ,BotId
      ,PhoneNumber
      ,MessageText
      ,MsgType
      ,ReceivedDateTime
      ,ScheduledSendDateTime     -- ← add this
      ,MessageHash
      ,StatusId
      ,Priority
      ,CampaignId
      ,CampDescription
      ,IsSystemApproved
      ,Paused)
    VALUES
      (@ChatId
      ,@CustomerId
      ,@BotId
      ,@PhoneNumber
      ,@MessageText
      ,@MsgType
      ,GETDATE()
      ,@ScheduledSendDateTime   -- ← use your variable here
      ,@hash
      ,CASE
          WHEN @isMissingChat = 1 THEN @NotSubId
          WHEN @isNew = 1         THEN @Pending
          ELSE                          @Duplicate
      END
      ,@Priority
      ,NULLIF(@CampaignId, '')
      ,NULLIF(@CampDescription, '')
      ,@IsSystemApproved
      ,0);

      DECLARE @id INT = SCOPE_IDENTITY();
      IF @isNew=1 UPDATE dbo.Table_Telegram_RecentMessages SET ReadyId=@id, ReceivedDateTime=@now WHERE MessageHash=@hash;

    COMMIT;
    SELECT @id AS NewId;
  END;
GO


/*******************************************
 * 2.4) usp_GetCustomerByUsername
 *******************************************/
CREATE OR ALTER PROCEDURE [dbo].[usp_GetCustomerByUsername]
    @Username NVARCHAR(100)
  AS
  BEGIN
      SET NOCOUNT ON;
      SELECT CustId as CustomerId, 
              UserName, 
              Password, 
              RequireSystemApprove,
              RequireAdminApprove, 
              IsActive, 
              IsBlocked, 
              IsTelegramActive
      FROM A2A_iMessaging.dbo.Table_UserSMSProfile
      WHERE UserName = @Username
  END;
GO


/*******************************************
 * 2.5) usp_GetTelegramUser   (NEW)
 * Returns the Table_Telegram_Recipients row for a given BotId
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_GetTelegramUser
    @BotId                 INT,
    @PhoneNumber           NVARCHAR(32)
  AS
  BEGIN
    SET NOCOUNT ON;

    SELECT * FROM dbo.Table_Telegram_Recipients
    WHERE   BotId     = @BotId
      AND   PhoneNumber = @PhoneNumber;
  END
GO


/*******************************************
 * 2.6) usp_GetChatIdsForPhones   (NEW)
 * Input:  @BotId, @PhoneNumbers TVP (expects E.164 in PhoneNumber)
 * Output: one row per requested phone with ChatId (NULL if not found)
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_GetChatIdsForPhones
    @BotId        INT,
    @PhoneNumbers dbo.PhoneList READONLY
  AS
  BEGIN
    SET NOCOUNT ON;

    SELECT
        p.PhoneNumber,
        u.ChatId
    FROM @PhoneNumbers AS p
    LEFT JOIN dbo.Table_Telegram_Recipients AS u
      ON u.BotId      = @BotId
    AND u.PhoneNumber  = p.PhoneNumber
    AND u.IsActive = 1;
  END
GO


/*******************************************
 * 2.7) usp_AddBatchFile to add the batch data into DB
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_AddBatchFile
    @CustomerId                 INT,
    @BotId                      INT,
    @MsgText                    NVARCHAR(MAX) = NULL,
    @MsgType                    NVARCHAR(10),
    @CampaignId                 NVARCHAR(128),
    @CampDescription            NVARCHAR(512) = NULL,
    @Priority                   SMALLINT,           -- table uses SMALLINT
    @IsSystemApproved           BIT,
    @IsAdminApproved            BIT,
    @ScheduledSendDateTime      DATETIME2 = NULL,  -- if NULL => GETDATE()
    @FilePath                   NVARCHAR(260),
    @FileType                   NVARCHAR(16),
    @IsProcessed                BIT = 0
  AS
  BEGIN
      SET NOCOUNT ON;

      DECLARE @Now DATETIME = GETDATE();

      INSERT INTO dbo.Table_Telegram_TelegramFiles
      (
          CustomerId,
          BotId,
          MsgText,
          MsgType,
          Priority,
          FilePath,
          FileType,
          CampaignId,
          CampDescription,
          ScheduledSendDateTime,
          CreationDate,
          IsSystemApproved,
          IsAdminApproved,
          IsProcessed
      )
      VALUES
      (
          @CustomerId,
          @BotId,
          @MsgText,
          @MsgType,
          @Priority,
          @FilePath,
          @FileType,
          @CampaignId,
          @CampDescription,
          ISNULL(@ScheduledSendDateTime, @Now),
          @Now,
          @IsSystemApproved,
          @IsAdminApproved,
          @IsProcessed
      );
  END
GO

/*******************************************
 * 2.8) usp_ReadyTable_BulkEnqueue
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_ReadyTable_BulkEnqueue
  @Batch dbo.TelegramMessage_Tvp READONLY
  AS
  BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @now DATETIME = GETDATE();

    -- Resolve status ids (Pending / Duplicate)
    DECLARE @PendingId SMALLINT = 1;

    DECLARE @DuplicateId SMALLINT = 20

    DECLARE @NotSubId SMALLINT = -2;

  -------------------------------------------------------------
    -- 1) Stage input (NO PK that would block dup rows in batch)
    -------------------------------------------------------------
    DECLARE @t TABLE
    (
      RowId INT IDENTITY(1,1) PRIMARY KEY,
      CustomerId INT,
      ChatId NVARCHAR(50) NULL,
      BotId INT,
      PhoneNumber NVARCHAR(32),
      MessageText NVARCHAR(MAX),
      MessageType NVARCHAR(10),
      ScheduledSendDateTime DATETIME2,
      Priority SMALLINT,
      CampaignId NVARCHAR(128) NULL,
      CampDescription NVARCHAR(512) NULL,
      IsSystemApproved BIT,
      IsMissingChat BIT,
      MessageHash BINARY(32)
    );

    INSERT @t
    SELECT
      b.CustomerId,
      b.ChatId,
      b.BotId,
      b.PhoneNumber,
      b.MessageText,
      b.MessageType,
      ISNULL(b.ScheduledSendDateTime, @now),
      CAST(b.Priority AS SMALLINT),
      NULLIF(LTRIM(RTRIM(b.CampaignId)), N''),
      NULLIF(LTRIM(RTRIM(b.CampDescription)), N''),
      b.IsSystemApproved,
      CASE
        WHEN b.ChatId IS NULL
          OR LEN(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(b.ChatId)), NCHAR(9), N''), NCHAR(10), N''), NCHAR(13), N'')) = 0
        THEN 1 ELSE 0 END,
      HASHBYTES('SHA2_256', CONCAT(ISNULL(b.ChatId,N''), N'|', b.BotId, N'|', ISNULL(b.MessageText,N'')))
    FROM @Batch AS b;

    BEGIN TRAN;

    -------------------------------------------------------------
    -- 2) NotSubscribed (skip Table_Telegram_RecentMessages)
    -------------------------------------------------------------
    INSERT dbo.Table_Telegram_ReadyTable
      (ChatId, CustomerId, BotId, PhoneNumber, MessageText, MsgType,
      ReceivedDateTime, ScheduledSendDateTime, MessageHash, Priority,
      CampaignId, CampDescription, IsSystemApproved, Paused, StatusId)
    SELECT
      t.ChatId, t.CustomerId, t.BotId, t.PhoneNumber, t.MessageText, t.MessageType,
      @now, t.ScheduledSendDateTime, t.MessageHash, t.Priority,
      t.CampaignId, t.CampDescription, t.IsSystemApproved, 0, @NotSubId
    FROM @t AS t
    WHERE t.IsMissingChat = 1;

    -------------------------------------------------------------
    -- 3) Register hashes for rows with ChatId present (1st-writer wins)
    -------------------------------------------------------------
    DECLARE @newHashes TABLE (MessageHash BINARY(32) PRIMARY KEY);
    INSERT dbo.Table_Telegram_RecentMessages (MessageHash, ReadyId, ReceivedDateTime)
    OUTPUT inserted.MessageHash INTO @newHashes(MessageHash)
    SELECT t.MessageHash, NULL, @now
    FROM @t AS t
    WHERE t.IsMissingChat = 0;

    -------------------------------------------------------------
    -- 4) Pick exactly ONE “winner” row per *new* hash (materialize)
    -------------------------------------------------------------
    DECLARE @winners TABLE (RowId INT PRIMARY KEY);

    INSERT @winners (RowId)
    SELECT RowId
    FROM (
        SELECT
          t.RowId,
          t.MessageHash,
          ROW_NUMBER() OVER (PARTITION BY t.MessageHash ORDER BY t.RowId) AS rn
        FROM @t AS t
        JOIN @newHashes nh ON nh.MessageHash = t.MessageHash   -- only NEW hashes
        WHERE t.IsMissingChat = 0
    ) x
    WHERE x.rn = 1;

    -------------------------------------------------------------
    -- 5) Winners -> Ready (Pending), capture IDs to backfill Table_Telegram_RecentMessages
    -------------------------------------------------------------
    DECLARE @insNew TABLE (MessageHash BINARY(32), ReadyId INT, ReceivedDateTime DATETIME2);

    INSERT dbo.Table_Telegram_ReadyTable
      (ChatId, CustomerId, BotId, PhoneNumber, MessageText, MsgType,
      ReceivedDateTime, ScheduledSendDateTime, MessageHash, Priority,
      CampaignId, CampDescription, IsSystemApproved, Paused, StatusId)
    OUTPUT inserted.MessageHash, inserted.ID, inserted.ReceivedDateTime
      INTO @insNew (MessageHash, ReadyId, ReceivedDateTime)
    SELECT
      t.ChatId, t.CustomerId, t.BotId, t.PhoneNumber, t.MessageText, t.MessageType,
      @now, t.ScheduledSendDateTime, t.MessageHash, t.Priority,
      t.CampaignId, t.CampDescription, t.IsSystemApproved, 0, @PendingId
    FROM @t t
    JOIN @winners w ON w.RowId = t.RowId;

    UPDATE rm
      SET rm.ReadyId = i.ReadyId,
          rm.ReceivedDateTime = i.ReceivedDateTime
    FROM dbo.Table_Telegram_RecentMessages rm
    JOIN @insNew i ON i.MessageHash = rm.MessageHash;

    -------------------------------------------------------------
    -- 6) All other Chat-present rows -> Ready (Duplicate)
    -------------------------------------------------------------
    INSERT dbo.Table_Telegram_ReadyTable
      (ChatId, CustomerId, BotId, PhoneNumber, MessageText, MsgType,
      ReceivedDateTime, ScheduledSendDateTime, MessageHash, Priority,
      CampaignId, CampDescription, IsSystemApproved, Paused, StatusId)
    SELECT
      t.ChatId, t.CustomerId, t.BotId, t.PhoneNumber, t.MessageText, t.MessageType,
      @now, t.ScheduledSendDateTime, t.MessageHash, t.Priority,
      t.CampaignId, t.CampDescription, t.IsSystemApproved, 0, @DuplicateId
    FROM @t t
    WHERE t.IsMissingChat = 0
      AND NOT EXISTS (SELECT 1 FROM @winners w WHERE w.RowId = t.RowId);

    COMMIT;
  END
GO

/*******************************************
 * 2.9) usp_GetBulkMessageByCampaignId
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_GetBulkMessageByCampaignId
    @CampaignId NVARCHAR(128)
  AS
  BEGIN
      SET NOCOUNT ON;

      SELECT *
      FROM dbo.Table_Telegram_TelegramFiles
      WHERE CampaignId = @CampaignId AND IsProcessed = 0 AND IsSystemApproved = 1;
  END;
GO

/*******************************************
 * 2.10) usp_ArchiveTelegramFileByCampaignId
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_ArchiveTelegramFileByCampaignId
    @CampaignId NVARCHAR(128)
  AS
  BEGIN
      SET NOCOUNT ON;

      BEGIN TRY
          BEGIN TRANSACTION;

          -- Insert into Table_Telegram_TelegramSentFiles the matching campaign
          INSERT INTO dbo.Table_Telegram_TelegramSentFiles (
              CustomerId, BotId, MsgText, MsgType, Priority,
              FilePath, FileType, CampaignId, CampDescription,
              ScheduledSendDateTime, CreationDate, IsProcessed
          )
          SELECT
              CustomerId, BotId, MsgText, MsgType, Priority,
              FilePath, FileType, CampaignId, CampDescription,
              ScheduledSendDateTime, CreationDate, 1
          FROM dbo.Table_Telegram_TelegramFiles
          WHERE CampaignId = @CampaignId;

          -- Delete from Table_Telegram_TelegramFiles after successful insert
          DELETE FROM dbo.Table_Telegram_TelegramFiles
          WHERE CampaignId = @CampaignId;

          COMMIT TRANSACTION;
      END TRY
      BEGIN CATCH
          IF @@TRANCOUNT > 0
              ROLLBACK TRANSACTION;

          THROW;
      END CATCH
  END;
GO

/*******************************************
 * 2.11) usp_GetBotById
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_GetBotById
    @BotId           INT,
    @CustomerId			 INT
  AS
  BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 * FROM dbo.Table_Telegram_Bots
    WHERE Id = @BotId
    AND CustomerId = @CustomerId
  END
GO

/*******************************************
 * 2.12) usp_Bot_UpdateActivity
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_Bot_UpdateActivity
    @BotId    INT,
    @IsActive BIT
  AS
  BEGIN
      SET NOCOUNT ON;

      UPDATE dbo.Table_Telegram_Bots
      SET IsActive = @IsActive
      WHERE Id = @BotId;
  END
GO

/*******************************************
 * 2.13) usp_Bot_CreateBot
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_Bot_CreateBot
    @BotName          NVARCHAR(50),
    @CustomerId       INT,
    @IsActive         BIT,
    @PublicId         NVARCHAR(128),
    @EncryptedBotKey  NVARCHAR(256),
    @WebhookSecret    NVARCHAR(128),
    @WebhookUrl       NVARCHAR(512)
  AS
  BEGIN
      SET NOCOUNT ON;

      INSERT INTO dbo.Table_Telegram_Bots (
          Name,
          CustomerId,
          IsActive,
          PublicId,
          EncryptedBotKey,
          WebhookSecret,
          WebhookUrl,
          CreationDateTime
      )
      VALUES (
          @BotName,
          @CustomerId,
          @IsActive,
          @PublicId,
          @EncryptedBotKey,
          @WebhookSecret,
          @WebhookUrl,
          GETDATE()
      );

    SELECT *
    FROM dbo.Table_Telegram_Bots
    WHERE Id = SCOPE_IDENTITY();
  END
GO
/*******************************************
 * 2.14) usp_GetBotByPublicId
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_GetBotByPublicId
    @PublicId NVARCHAR(128)
  AS
  BEGIN
      SET NOCOUNT ON;

      SELECT TOP 1
          Id,
          CustomerId,
          EncryptedBotKey,
          PublicId,
          WebhookSecret,
          WebhookUrl,
          IsActive,
          CreationDateTime
      FROM dbo.Table_Telegram_Bots
      WHERE PublicId = @PublicId;
  END
GO

/*******************************************
 * 2.15) usp_Recipient_Upsert
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_Recipient_Upsert
    @BotId          INT,
    @ChatId         NVARCHAR(50),
    @PhoneNumber    NVARCHAR(32) = NULL,
    @TelegramUserId BIGINT,
    @Username       NVARCHAR(64) = NULL,
    @FirstName      NVARCHAR(64) = NULL,
    @IsActive       BIT          = 1
  AS
  BEGIN
      SET NOCOUNT ON;
      DECLARE @now DATETIME = GETDATE();

      UPDATE dbo.Table_Telegram_Recipients
      SET TelegramUserId          = @TelegramUserId,
          PhoneNumber             = COALESCE(@PhoneNumber, PhoneNumber),
          Username                = COALESCE(@Username, Username),
          FirstName               = COALESCE(@FirstName, FirstName),
          IsActive                = @IsActive,
          LastUpdatedDateTime     = @now,
          LastSeenDateTime        = @now
      WHERE BotId = @BotId
      AND ChatId = @ChatId;

      IF @@ROWCOUNT = 0
      BEGIN
          INSERT INTO dbo.Table_Telegram_Recipients
              (BotId, ChatId, TelegramUserId, PhoneNumber, Username, FirstName, IsActive, CreationDateTime, LastSeenDateTime, LastUpdatedDateTime)
          VALUES
              (@BotId, @ChatId, @TelegramUserId, @PhoneNumber, @Username, @FirstName, @IsActive, @now, @now, @now);
      END
  END
GO

/*******************************************
 * 2.15) usp_ArchiveReady_NegativeStatuses_Daily: This is being used for the background job
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_ArchiveReady_NegativeStatuses_Daily
  @BatchSize INT = 50000
  AS
  BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    WHILE 1 = 1
    BEGIN
      DECLARE @now DATETIME = GETDATE();
      DECLARE @moved INT = 0;

      DECLARE @del TABLE(
        ID                   INT           PRIMARY KEY,
        CustomerId           INT,
        ChatId               NVARCHAR(50)  NULL,
        BotId                INT,
        PhoneNumber          NVARCHAR(32),
        MessageText          NVARCHAR(MAX),
        MsgType              NVARCHAR(10),
        ReceivedDateTime     DATETIME2,
        ScheduledSendDateTime DATETIME2,
        GatewayDateTime      DATETIME2,
        MessageHash          VARBINARY(32),
        Priority             SMALLINT,
        StatusId             SMALLINT,
        CampaignId           NVARCHAR(128),
        CampDescription      NVARCHAR(512)
      );

      BEGIN TRAN;

      ;WITH pick AS (
        SELECT TOP (@BatchSize) *
        FROM dbo.Table_Telegram_ReadyTable WITH (READPAST, ROWLOCK)
        WHERE StatusId IN (-1, -2, 20)
        ORDER BY ID
      )
      DELETE FROM pick
      OUTPUT
        deleted.ID, deleted.CustomerId, deleted.ChatId, deleted.BotId,
        deleted.PhoneNumber, deleted.MessageText, deleted.MsgType,
        deleted.ReceivedDateTime, deleted.ScheduledSendDateTime,
        @now,
        deleted.MessageHash, deleted.Priority, deleted.StatusId,
        deleted.CampaignId, deleted.CampDescription
      INTO @del(
        ID, CustomerId, ChatId, BotId, PhoneNumber, MessageText, MsgType,
        ReceivedDateTime, ScheduledSendDateTime, GatewayDateTime,
        MessageHash, Priority, StatusId,
        CampaignId, CampDescription
      );
      SET @moved = @@ROWCOUNT;

      IF @moved > 0
      BEGIN
        INSERT dbo.Table_Telegram_ArchiveTable
        (ID, CustomerId, ChatId, BotId, PhoneNumber, MessageText, MsgType,
         ReceivedDateTime, ScheduledSendDateTime, GatewayDateTime,
         MessageHash, Priority, StatusId, StatusDescription, MobileCountry,
         CampaignId, CampDescription)
      SELECT
        d.ID, d.CustomerId, d.ChatId, d.BotId, d.PhoneNumber, d.MessageText, d.MsgType,
        d.ReceivedDateTime, d.ScheduledSendDateTime, d.GatewayDateTime,
        d.MessageHash, d.Priority, d.StatusId,
          ISNULL(ms.StatusDescription, N'Unknown') AS StatusDescription,
          CASE WHEN OBJECT_ID(N'[A2A_iMessaging].[dbo].[GetCountryCode]') IS NOT NULL
              THEN [A2A_iMessaging].[dbo].[GetCountryCode](d.PhoneNumber)
              ELSE N'UNK' END,
          d.CampaignId, d.CampDescription
        FROM @del AS d
        LEFT JOIN dbo.Table_Telegram_MessageStatus AS ms
          ON ms.StatusId = d.StatusId;

        DELETE FROM @del; -- clear buffer for next loop
      END

      COMMIT;
      IF @moved < @BatchSize BREAK; -- last batch was smaller → done
    END
  END
GO

/*******************************************
 * 3) Gateway SPs
 *******************************************/
/*******************************************
 * 3.1) usp_GetPendingReadyMessages
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_GetPendingReadyMessages
  AS
  BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    ;WITH ranked AS
    (
      SELECT
        r.ID, r.BotId, r.ChatId, r.MessageText,
        r.Priority, r.ReceivedDateTime,
        ROW_NUMBER() OVER
        (
          PARTITION BY r.ChatId
          ORDER BY r.Priority DESC, r.ReceivedDateTime ASC
        ) AS rn
      FROM dbo.Table_Telegram_ReadyTable AS r WITH (READPAST, ROWLOCK)
      WHERE r.ScheduledSendDateTime <= GETDATE()
        AND r.StatusId = 1            -- Pending
    ),
    picks AS
    (
      SELECT TOP (30) ID
      FROM ranked
      WHERE rn = 1
      ORDER BY Priority DESC, ReceivedDateTime ASC
    )
    UPDATE r
      SET r.StatusId = 4               -- InFlight (or your value)
    OUTPUT inserted.ID,
          inserted.MessageText,
          inserted.ChatId,
          b.EncryptedBotKey
    FROM dbo.Table_Telegram_ReadyTable AS r WITH (ROWLOCK, READPAST)
    JOIN picks       AS p ON p.Id = r.Id
    JOIN dbo.Table_Telegram_Bots    AS b ON b.Id = r.BotId
    WHERE r.StatusId = 1; -- Pending             -- ensures we don’t output rows already claimed
  END
GO

/*******************************************
 * 3.2) usp_ArchiveAndDeleteQueuedMessage
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_ArchiveAndDeleteReadyMessage
  @Id INT  -- this is the Table_Telegram_ReadyTable.ID
  AS
  BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;  -- auto‐rollback on error

    BEGIN TRAN;
    BEGIN TRY

    DECLARE @now DATETIME = GETDATE();

    INSERT INTO dbo.Table_Telegram_ArchiveTable
      (ID,
       CustomerId,
       ChatId,
       BotId,
       PhoneNumber,
       MessageText,
       MsgType,
       ReceivedDateTime,
       ScheduledSendDateTime,
       GatewayDateTime,
       MessageHash,
       Priority,
       StatusId,
       StatusDescription,
       MobileCountry,
       CampaignId,
       CampDescription)
    SELECT
      rt.ID,
      rt.CustomerId,
      rt.ChatId,
      rt.BotId,
      rt.PhoneNumber,
      rt.MessageText,
      rt.MsgType,
      rt.ReceivedDateTime,
      rt.ScheduledSendDateTime,
      @now,
      rt.MessageHash,
      rt.Priority,
      0, -- Delivered
      ISNULL(ms.StatusDescription, N'Unknown') AS StatusDescription,
      CASE
        WHEN OBJECT_ID(N'[A2A_iMessaging].[dbo].[GetCountryCode]') IS NOT NULL
          THEN [A2A_iMessaging].[dbo].[GetCountryCode](PhoneNumber)
        ELSE N'UNK'
      END,
      rt.CampaignId,
      rt.CampDescription
    FROM dbo.Table_Telegram_ReadyTable AS rt
      LEFT JOIN dbo.Table_Telegram_MessageStatus AS ms
      ON ms.StatusId = 0
    WHERE rt.ID = @Id;

    IF @@ROWCOUNT <> 1
      THROW 51001, 'Archive failed: Ready.ID not found or duplicate insert blocked.', 1;

    DELETE FROM dbo.Table_Telegram_ReadyTable
    WHERE ID = @Id;

    IF @@ROWCOUNT <> 1
      THROW 51002, 'Delete failed: Ready.ID not found after archive.', 1;

    COMMIT TRAN;

    END TRY
    BEGIN CATCH
      IF XACT_STATE() <> 0
        ROLLBACK TRAN;
      THROW;  -- re‐raise the error
    END CATCH
  END;
GO