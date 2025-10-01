use TelegramTestDB;
GO

/*******************************************
 * 0) Dropping Tables
 *******************************************/

IF OBJECT_ID('dbo.Table_Telegram_ReadyTable','U') IS NOT NULL
  DROP TABLE dbo.Table_Telegram_ReadyTable;
GO

IF OBJECT_ID('dbo.Table_Telegram_ArchiveTable', 'U') IS NOT NULL
  DROP TABLE dbo.Table_Telegram_ArchiveTable;
GO

 IF OBJECT_ID('dbo.Table_Telegram_MessageStatus','U') IS NOT NULL
  DROP TABLE dbo.Table_Telegram_MessageStatus;
GO

IF OBJECT_ID('dbo.Table_Telegram_RecentMessages','U') IS NOT NULL
  DROP TABLE dbo.Table_Telegram_RecentMessages;
GO

IF OBJECT_ID('dbo.Table_Telegram_TelegramSentFiles','U') IS NOT NULL
  DROP TABLE dbo.Table_Telegram_TelegramSentFiles;
GO

-- Drop TVPs safely (use TYPE_ID for types)
IF TYPE_ID(N'dbo.PhoneList') IS NOT NULL
  DROP TYPE dbo.PhoneList;
GO

IF TYPE_ID(N'dbo.TelegramMessage_Tvp') IS NOT NULL
  DROP TYPE dbo.TelegramMessage_Tvp;
GO

IF OBJECT_ID('dbo.Table_Telegram_TelegramFiles','U') IS NOT NULL
  DROP TABLE dbo.Table_Telegram_TelegramFiles;
GO

IF OBJECT_ID('dbo.Table_Telegram_Recipients','U') IS NOT NULL
  DROP TABLE dbo.Table_Telegram_Recipients;
GO

IF OBJECT_ID('dbo.Table_Telegram_Bots','U') IS NOT NULL
  DROP TABLE dbo.Table_Telegram_Bots;
GO

IF OBJECT_ID('dbo.Table_Telegram_SuperAdminTelegramProfiles', 'U') IS NOT NULL
   DROP TABLE dbo.Table_Telegram_SuperAdminTelegramProfiles; 
GO

IF OBJECT_ID('dbo.Table_Telegram_AdminTelegramProfiles', 'U') IS NOT NULL
  DROP TABLE dbo.Table_Telegram_AdminTelegramProfiles;
GO

IF OBJECT_ID('dbo.Table_Telegram_UserTelegramProfiles', 'U') IS NOT NULL
  DROP TABLE dbo.Table_Telegram_UserTelegramProfiles;
GO


/*******************************************
 * 1.1) Table_Telegram_MessageStatus: Enum for message status
 *******************************************/
CREATE TABLE dbo.Table_Telegram_MessageStatus (
    Id                SMALLINT NOT NULL PRIMARY KEY,
    StatusDescription NVARCHAR(50) NOT NULL UNIQUE
);

-- Insert your enum values
INSERT INTO dbo.Table_Telegram_MessageStatus (Id, StatusDescription)
VALUES 
    (4, 'In Flight'),
    (3, 'Failed'),
    (2, 'Ready'),
    (1, 'Pending'),
    (0, 'Sent'),
    (-1, 'Blocked'),
    (-2, 'Not Subscribed'),
    (20, 'Duplicate');

/*******************************************
 * 1.2) Table_Telegram_ReadyTable: pending messages queue
 *******************************************/
CREATE TABLE dbo.Table_Telegram_ReadyTable
(
  [Id]           			    INT					          IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReadyTable PRIMARY KEY CLUSTERED,
  [CustomerId]			      INT				 	          NOT NULL,
  [ChatId]       			    NVARCHAR(50)          NULL,
  [BotId]                 INT  		              NOT NULL,
  [PhoneNumber]           NVARCHAR(32)          NOT NULL,
  [MessageText]  			    NVARCHAR(MAX)  		    NOT NULL,
  [MsgType]				        NVARCHAR(10)		      NOT NULL,
  [ReceivedDateTime]		  DATETIME2             NOT NULL, -- Auto Generated using GETDATE() in the SP.
  [ScheduledSendDateTime] DATETIME2             NOT NULL, -- Auto Generated using GETDATE() in the SP.
  [MessageHash]  			    BINARY(32)            NOT NULL, -- Auto Generated from the SP.
  [Priority]     			    SMALLINT       	      NOT NULL,
  [StatusId]              SMALLINT              NOT NULL,
  [CampaignId]			      NVARCHAR(128)	        NULL,
  [CampDescription]		    NVARCHAR(512)		      NULL,
  [IsSystemApproved]		  BIT					          NOT NULL,
  [Paused]				        BIT					          NOT NULL,

  CONSTRAINT FK_ReadyTable_Status FOREIGN KEY (StatusId) 
      REFERENCES dbo.Table_Telegram_MessageStatus(Id)
  );
GO

CREATE NONCLUSTERED INDEX IX_ReadyTable_MessageHash_ReadyDate
  ON dbo.Table_Telegram_ReadyTable (MessageHash, Id);
GO

CREATE NONCLUSTERED INDEX IX_ReadyTable_ID_Priority
  ON dbo.Table_Telegram_ReadyTable (Id, Priority);
GO

CREATE NONCLUSTERED INDEX IX_ReadyTable_ChatId_Null
    ON dbo.Table_Telegram_ReadyTable (Id)
    WHERE ChatId IS NULL;
GO

CREATE NONCLUSTERED INDEX IX_Ready_Pending_ID 
    ON dbo.Table_Telegram_ReadyTable(Id) 
  WHERE StatusId = 1;  -- Pending
GO


/*******************************************
 * 1.3) Table_Telegram_ArchiveTable: all sent or deduped msgs
 *******************************************/
CREATE TABLE dbo.Table_Telegram_ArchiveTable
(
  [Id]                      INT             NOT NULL,  -- surrogate PK
  [CustomerId]              INT             NOT NULL,
  [ChatId]                  NVARCHAR(50)    NULL,
  [BotId]                   INT             NOT NULL,
  [PhoneNumber]             NVARCHAR(32)    NOT NULL,
  [MessageText]             NVARCHAR(MAX)   NOT NULL,
  [MsgType]                 NVARCHAR(10)    NOT NULL,
  [ReceivedDateTime]        DATETIME2       NOT NULL,
  [ScheduledSendDateTime]   DATETIME2       NOT NULL, -- set by SP
  [GatewayDateTime]         DATETIME2       NOT NULL,
  [MessageHash]             BINARY(32)      NOT NULL,
  [Priority]                SMALLINT        NOT NULL,
  -- Enum  denormalized text
  [StatusId]                SMALLINT        NOT NULL,
  [StatusDescription]       NVARCHAR(512)   NULL,  -- No default, will be NULL until set

  [MobileCountry]           NVARCHAR(10)    NOT NULL,
  [CampaignId]              NVARCHAR(128)    NULL,
  [CampDescription]         NVARCHAR(512)   NULL,

  CONSTRAINT PK_ArchiveTable_ID PRIMARY KEY CLUSTERED (Id),
  CONSTRAINT FK_ArchiveTable_Status FOREIGN KEY (StatusId) 
      REFERENCES dbo.Table_Telegram_MessageStatus(Id)
);
GO

CREATE NONCLUSTERED INDEX IX_ArchiveTable_MessageHash
  ON dbo.Table_Telegram_ArchiveTable (MessageHash);
GO

/*******************************************
 * 1.4) Table_Telegram_RecentMessages: 5-minute dedupe window
 *******************************************/
CREATE TABLE dbo.Table_Telegram_RecentMessages
(
  MessageHash  		    BINARY(32)     NOT NULL,
  ReceivedDateTime      DATETIME      NOT NULL,
  ReadyId      		    INT            NULL,
);
GO

CREATE NONCLUSTERED INDEX IX_RecentMessages_ReadyDate
  ON dbo.Table_Telegram_RecentMessages (ReceivedDateTime);
GO

CREATE NONCLUSTERED INDEX IX_RecentMessages_ReadyId
  ON dbo.Table_Telegram_RecentMessages (ReadyId);
GO

CREATE UNIQUE INDEX UX_RecentMessages_MessageHash 
  ON dbo.Table_Telegram_RecentMessages(MessageHash) WITH (IGNORE_DUP_KEY = ON);
GO

/*******************************************
 * 1.6) Table_Telegram_TelegramSentFiles: Table for files sent via Telegram (Batch or Campaign)
 *******************************************/
CREATE TABLE dbo.Table_Telegram_TelegramSentFiles
(
  [Id]                            BIGINT IDENTITY(1,1) NOT NULL
      CONSTRAINT PK_TelegramSentFiles PRIMARY KEY,

  [CustomerId]                    INT            NOT NULL,
  [BotId]                         INT            NOT NULL,   -- e.g., BotId or sender alias
  [MsgText]                       NVARCHAR(MAX)  NULL,       -- NULL WHEN BATCH
  [MsgType]                       NVARCHAR(10)   NOT NULL,   -- e.g., 'AF'
  [Priority]                      SMALLINT       NOT NULL,
  [FilePath]                      NVARCHAR(260)  NOT NULL,       -- Windows path max
  [FileType]                      NVARCHAR(16)   NOT NULL,       -- Batch or Campaign.
  [CampaignId]                    NVARCHAR(128)   NOT NULL UNIQUE,
  [CampDescription]               NVARCHAR(256)  NULL,
  [ScheduledSendDateTime]         DATETIME2      NOT NULL,       -- NULL = send ASAP
  [CreationDate]                  DATETIME2      NOT NULL,
  [IsProcessed]                   BIT            NOT NULL
  );
GO

-- Helpful indexes
CREATE INDEX IX_TelegramSentFiles_Campaign
  ON dbo.Table_Telegram_TelegramSentFiles (CampaignId);


 /*******************************************
 * 1.7) PhoneList: Table type for passing phone numbers
 *******************************************/
  -- Table type to pass phone numbers
CREATE TYPE dbo.PhoneList AS TABLE
(
  PhoneNumber NVARCHAR(32) NOT NULL PRIMARY KEY
);
GO

/*******************************************
* 1.8) TelegramMessage_Tvp: Table type for passing batch messages
*******************************************/
  -- Table type to pass batch messages
  -- 1) Table type for TVP (what C# will send)
CREATE TYPE dbo.TelegramMessage_Tvp AS TABLE
(
  [CustomerId]              INT             NOT NULL,
  [ChatId]                  NVARCHAR(50)    NULL,
  [BotId]                   INT             NOT NULL,
  [PhoneNumber]             NVARCHAR(32)    NOT NULL,
  [MessageText]             NVARCHAR(4000)   NOT NULL,
  [MessageType]             NVARCHAR(10)    NOT NULL,
  [ScheduledSendDateTime]   DATETIME2       NULL,      -- optional; defaulted in proc when NULL
  [Priority]                SMALLINT        NOT NULL,
  [CampaignId]              NVARCHAR(128)    NULL,
  [CampDescription]         NVARCHAR(512)   NULL,
  [IsSystemApproved]        BIT             NOT NULL
);
GO

/*******************************************
 * 1.9) Table_Telegram_TelegramFiles: Table for files to be processed via Telegram (Batch or Campaign)
 *******************************************/
  -- Table to store batch files to be processed

CREATE TABLE dbo.Table_Telegram_TelegramFiles
(
  [Id]                     BIGINT IDENTITY(1,1) NOT NULL
      CONSTRAINT PK_TelegramFiles PRIMARY KEY,

  [CustomerId]                    INT            NOT NULL,
  [BotId]                         INT            NOT NULL,   -- e.g., bot key or sender alias
  [MsgText]                       NVARCHAR(MAX)  NULL,       -- NULL WHEN BATCH
  [MsgType]                       NVARCHAR(10)   NOT NULL,   -- e.g., 'AF'
  [Priority]                      SMALLINT       NOT NULL,
  [FilePath]                      NVARCHAR(260)  NOT NULL,   -- Windows path max
  [FileType]                      NVARCHAR(16)   NOT NULL,   -- Batch or Campaign
  [CampaignId]                    NVARCHAR(128)   NOT NULL UNIQUE,
  [CampDescription]               NVARCHAR(256)  NULL,
  [ScheduledSendDateTime]         DATETIME2      NOT NULL,
  [CreationDate]                  DATETIME2      NOT NULL,
  [isSystemApproved]              BIT            NOT NULL,
  [isAdminApproved]               BIT            NOT NULL,
  [IsProcessed]                   BIT            NOT NULL DEFAULT 0
);
GO

/*******************************************
 * 1.10) Table_Telegram_Bots: Table for storing bot information
 *******************************************/

CREATE TABLE dbo.Table_Telegram_Bots
(
  [Id]                        INT           IDENTITY PRIMARY KEY,
  [Name]                      NVARCHAR(50)  NOT NULL,                          -- Bot name
  [CustomerId]                INT           NOT NULL, -- FK to Table_UserSMSProfile.CustomerId
  [EncryptedBotKey]           NVARCHAR(256) NOT NULL  UNIQUE,                        -- encrypted token
  [PublicId]                  NVARCHAR(128) NOT NULL  UNIQUE,
  [WebhookSecret]             NVARCHAR(128) NOT NULL  UNIQUE,                -- per-bot secret_token
  [WebhookUrl]                NVARCHAR(512) NOT NULL  UNIQUE,
  [IsActive]                  BIT           NOT NULL DEFAULT 1,
  [CreationDateTime]          DATETIME2     NOT NULL DEFAULT GETDATE()
);

CREATE INDEX IX_Bots_CustomerId ON dbo.Table_Telegram_Bots(CustomerId);

/*******************************************
 * 1.11) Table_Telegram_Recipients (Final schema)
 *******************************************/
CREATE TABLE dbo.Table_Telegram_Recipients
(
  [BotId]                 INT          NOT NULL 
    CONSTRAINT FK_Recipient_Bots REFERENCES dbo.Table_Telegram_Bots(Id),

  [ChatId]                NVARCHAR(50) NOT NULL,         -- Telegram DM chat id (user id in DMs, may be negative for groups if reused)
  [TelegramUserId]        BIGINT       NOT NULL,         -- Id of the user in Telegram application.

  [PhoneNumber]           NVARCHAR(32) NULL,             -- e.g., 9627... (nullable to match SP)
  [FirstName]             NVARCHAR(64) NULL,
  [Username]              NVARCHAR(64) NULL,             -- Telegram username limit is 32, keeping 64 for buffer

  [CreationDateTime]      DATETIME2(3) NOT NULL DEFAULT GETDATE(),
  [LastSeenDateTime]      DATETIME2(3) NOT NULL DEFAULT GETDATE(),
  [LastUpdatedDateTime]         DATETIME2(3) NOT NULL DEFAULT GETDATE(),
  [IsActive]              BIT          NOT NULL DEFAULT 1,

  CONSTRAINT PK_Recipient PRIMARY KEY (BotId, ChatId)
);

-- Recent/active fetch
CREATE INDEX IX_Recipient_Bot_LastSeen
  ON dbo.Table_Telegram_Recipients (BotId, LastSeenDateTime DESC);

-- Helpful lookups
CREATE UNIQUE INDEX IX_Recipient_Bot_TelegramUserId
  ON dbo.Table_Telegram_Recipients (BotId, TelegramUserId);

CREATE INDEX IX_Recipient_Bot_Username
  ON dbo.Table_Telegram_Recipients (BotId, Username);


/*******************************************
  * 4) Portal tables
  *******************************************/
/*==============================
  Admin
==============================*/
CREATE TABLE dbo.Table_Telegram_AdminTelegramProfiles
  (
      Id              INT PRIMARY KEY,
      CanViewContent  BIT NOT NULL CONSTRAINT DF_AdminTP_CanViewContent  DEFAULT (0),
      IsPrepaid       BIT NOT NULL CONSTRAINT DF_AdminTP_IsPrepaid       DEFAULT (0),
      CanViewReports  BIT NOT NULL CONSTRAINT DF_AdminTP_CanViewReports  DEFAULT (0),
      HasOTP          BIT NOT NULL CONSTRAINT DF_AdminTP_HasOTP          DEFAULT (0),
      CanManageUsers  BIT NOT NULL CONSTRAINT DF_AdminTP_CanManageUsers  DEFAULT (0),
      HasOutbox       BIT NOT NULL CONSTRAINT DF_AdminTP_HasOutbox       DEFAULT (0),
      HasSurvey       BIT NOT NULL CONSTRAINT DF_AdminTP_HasSurvey       DEFAULT (0),

      -- hygiene (safe additions)
      IsActive        BIT NOT NULL CONSTRAINT DF_AdminTP_IsActive        DEFAULT (1),
      CreatedAt       DATETIME2(0)  NOT NULL CONSTRAINT DF_AdminTP_CreatedAt DEFAULT GETDATE(),
      CreatedBy       NVARCHAR(128) NULL,
      UpdatedAt       DATETIME2(0)  NULL,
      UpdatedBy       NVARCHAR(128) NULL,
      RowVer          ROWVERSION,

      -- keep bits strictly 0/1 (paranoid)
      CONSTRAINT CK_AdminTP_Bits CHECK (
          CanViewContent IN (0,1) AND IsPrepaid IN (0,1) AND CanViewReports IN (0,1) AND
          HasOTP IN (0,1) AND CanManageUsers IN (0,1) AND HasOutbox IN (0,1) AND HasSurvey IN (0,1) AND
          IsActive IN (0,1)
      )
  );

 CREATE INDEX IX_AdminTP_IsActive        ON dbo.Table_Telegram_AdminTelegramProfiles(IsActive);

/*==============================
  SuperAdmin
==============================*/
CREATE TABLE dbo.Table_Telegram_SuperAdminTelegramProfiles
  (
      Id              INT PRIMARY KEY,
      CanViewContent  BIT NOT NULL CONSTRAINT DF_SuperTP_CanViewContent  DEFAULT (1),
      IsPrepaid       BIT NOT NULL CONSTRAINT DF_SuperTP_IsPrepaid       DEFAULT (0),
      CanViewReports  BIT NOT NULL CONSTRAINT DF_SuperTP_CanViewReports  DEFAULT (1),
      HasOTP          BIT NOT NULL CONSTRAINT DF_SuperTP_HasOTP          DEFAULT (1),
      CanManageUsers  BIT NOT NULL CONSTRAINT DF_SuperTP_CanManageUsers  DEFAULT (1),
      HasOutbox       BIT NOT NULL CONSTRAINT DF_SuperTP_HasOutbox       DEFAULT (1),
      HasSurvey       BIT NOT NULL CONSTRAINT DF_SuperTP_HasSurvey       DEFAULT (0),

      IsActive        BIT NOT NULL CONSTRAINT DF_SuperTP_IsActive        DEFAULT (1),
      CreatedAt       DATETIME2(0)  NOT NULL CONSTRAINT DF_SuperTP_CreatedAt DEFAULT GETDATE(),
      CreatedBy       NVARCHAR(128) NULL,
      UpdatedAt       DATETIME2(0)  NULL,
      UpdatedBy       NVARCHAR(128) NULL,
      RowVer          ROWVERSION,

      CONSTRAINT CK_SuperTP_Bits CHECK (
          CanViewContent IN (0,1) AND IsPrepaid IN (0,1) AND CanViewReports IN (0,1) AND
          HasOTP IN (0,1) AND CanManageUsers IN (0,1) AND HasOutbox IN (0,1) AND HasSurvey IN (0,1) AND
          IsActive IN (0,1)
      )
  );

CREATE INDEX IX_SuperTP_IsActive        ON dbo.Table_Telegram_SuperAdminTelegramProfiles(IsActive);
GO