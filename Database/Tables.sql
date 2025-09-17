/*******************************************
 * 0) Dropping Tables
 *******************************************/

IF OBJECT_ID('dbo.ReadyTable','U') IS NOT NULL
  DROP TABLE dbo.ReadyTable;
GO

IF OBJECT_ID('dbo.ArchiveTable', 'U') IS NOT NULL
  DROP TABLE dbo.ArchiveTable;
GO

 IF OBJECT_ID('dbo.MessageStatus','U') IS NOT NULL
  DROP TABLE dbo.MessageStatus;
GO

IF OBJECT_ID('dbo.RecentMessages','U') IS NOT NULL
  DROP TABLE dbo.RecentMessages;
GO

IF OBJECT_ID('dbo.TelegramSentFiles','U') IS NOT NULL
  DROP TABLE dbo.TelegramSentFiles;
GO

-- Drop TVPs safely (use TYPE_ID for types)
IF TYPE_ID(N'dbo.PhoneList') IS NOT NULL
  DROP TYPE dbo.PhoneList;
GO

IF TYPE_ID(N'dbo.TelegramMessage_Tvp') IS NOT NULL
  DROP TYPE dbo.TelegramMessage_Tvp;
GO

IF OBJECT_ID('dbo.TelegramFiles','U') IS NOT NULL
  DROP TABLE dbo.TelegramFiles;
GO

IF OBJECT_ID('dbo.TelegramUserChats','U') IS NOT NULL
  DROP TABLE dbo.TelegramUserChats;
GO

IF OBJECT_ID('dbo.Bots','U') IS NOT NULL
  DROP TABLE dbo.Bots;
GO


/*******************************************
 * 1.1) MessageStatus: Enum for message status
 *******************************************/
CREATE TABLE dbo.MessageStatus (
    StatusID SMALLINT NOT NULL PRIMARY KEY,
    StatusDescription NVARCHAR(50) NOT NULL UNIQUE
);

-- Insert your enum values
INSERT INTO dbo.MessageStatus (StatusID, StatusDescription)
VALUES 
    (4, 'In Flight'),
    (3, 'Failed'),
    (1, 'Sent'),
    (2, 'Ready'),
    (0, 'Pending'),
    (-1, 'Blocked'),
    (-2, 'Not Subscribed'),
    (-3, 'Duplicate');

/*******************************************
 * 1.2) ReadyTable: pending messages queue
 *******************************************/
CREATE TABLE dbo.ReadyTable
(
  [ID]           			    INT					          IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReadyTable PRIMARY KEY CLUSTERED,
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
      REFERENCES dbo.MessageStatus(StatusID)
  );
GO

CREATE NONCLUSTERED INDEX IX_ReadyTable_MessageHash_ReadyDate
  ON dbo.ReadyTable (MessageHash, ID);
GO

CREATE NONCLUSTERED INDEX IX_ReadyTable_ID_Priority
  ON dbo.ReadyTable (ID, Priority);
GO

CREATE NONCLUSTERED INDEX IX_ReadyTable_ChatId_Null
    ON dbo.ReadyTable (ID)
    WHERE ChatId IS NULL;
GO

CREATE NONCLUSTERED INDEX IX_Ready_Pending_ID 
    ON dbo.ReadyTable(ID) 
  WHERE StatusId = 0;
GO


/*******************************************
 * 1.3) ArchiveTable: all sent or deduped msgs
 *******************************************/
CREATE TABLE dbo.ArchiveTable
(
  [ID]                      INT             NOT NULL,  -- surrogate PK
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
  [IsSystemApproved]        BIT             NOT NULL,
  [Paused]                  BIT             NOT NULL,

  CONSTRAINT PK_ArchiveTable_ID PRIMARY KEY CLUSTERED (ID),
  CONSTRAINT FK_ArchiveTable_Status FOREIGN KEY (StatusId) 
      REFERENCES dbo.MessageStatus(StatusID)
);
GO

CREATE NONCLUSTERED INDEX IX_ArchiveTable_MessageHash
  ON dbo.ArchiveTable (MessageHash);
GO

/*******************************************
 * 1.4) RecentMessages: 5-minute dedupe window
 *******************************************/
CREATE TABLE dbo.RecentMessages
(
  MessageHash  		    BINARY(32)     NOT NULL,
  ReceivedDateTime      DATETIME      NOT NULL,
  ReadyId      		    INT            NULL,
);
GO

CREATE NONCLUSTERED INDEX IX_RecentMessages_ReadyDate
  ON dbo.RecentMessages (ReceivedDateTime);
GO

CREATE NONCLUSTERED INDEX IX_RecentMessages_ReadyId
  ON dbo.RecentMessages (ReadyId);
GO

CREATE UNIQUE INDEX UX_RecentMessages_MessageHash 
  ON dbo.RecentMessages(MessageHash) WITH (IGNORE_DUP_KEY = ON);
GO

/*******************************************
 * 1.6) TelegramSentFiles: Table for files sent via Telegram (Batch or Campaign)
 *******************************************/
CREATE TABLE dbo.TelegramSentFiles
(
  [ID]                            BIGINT IDENTITY(1,1) NOT NULL
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
  [isSystemApproved]              BIT            NOT NULL,
  [isAdminApproved]               BIT            NOT NULL,
  [IsProcessed]                   BIT            NOT NULL
  
  );
GO

-- Helpful indexes
CREATE INDEX IX_TelegramSentFiles_Campaign
  ON dbo.TelegramSentFiles (CampaignId);


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
 * 1.9) TelegramFiles: Table for files to be processed via Telegram (Batch or Campaign)
 *******************************************/
  -- Table to store batch files to be processed

CREATE TABLE dbo.TelegramFiles
(
  [ID]                     BIGINT IDENTITY(1,1) NOT NULL
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
 * 1.10) Bots: Table for storing bot information
 *******************************************/

CREATE TABLE dbo.Bots
(
  [BotId]                     INT           IDENTITY PRIMARY KEY,
  [CustomerId]                INT           NOT NULL,                          -- FK to Table_UserSMSProfile.CustomerId
  [EncryptedBotKey]           NVARCHAR(256) NOT NULL  UNIQUE,                        -- encrypted token
  [PublicId]                  NVARCHAR(128) NOT NULL  UNIQUE,
  [WebhookSecret]             NVARCHAR(128) NOT NULL  UNIQUE,                -- per-bot secret_token
  [WebhookUrl]                NVARCHAR(512) NOT NULL  UNIQUE,
  [IsActive]                  BIT           NOT NULL DEFAULT 1,
  [CreationDateTime]          DATETIME2     NOT NULL DEFAULT GETDATE()
);

CREATE INDEX IX_Bots_CustomerId ON dbo.Bots(CustomerId);

/*******************************************
 * 1.11) TelegramUserChats (Final schema)
 *******************************************/
CREATE TABLE dbo.TelegramUserChats
(
  [BotId]                 INT          NOT NULL 
    CONSTRAINT FK_TelegramUserChats_Bots REFERENCES dbo.Bots(BotId),

  [ChatId]                NVARCHAR(50) NOT NULL,         -- Telegram DM chat id (user id in DMs, may be negative for groups if reused)
  [TelegramUserId]        BIGINT       NOT NULL,         -- Id of the user in Telegram application.

  [PhoneNumber]           NVARCHAR(32) NULL,             -- e.g., 9627... (nullable to match SP)
  [FirstName]             NVARCHAR(64) NULL,
  [Username]              NVARCHAR(64) NULL,             -- Telegram username limit is 32, keeping 64 for buffer

  [CreationDateTime]      DATETIME2(3) NOT NULL DEFAULT GETDATE(),
  [LastSeenDateTime]      DATETIME2(3) NOT NULL DEFAULT GETDATE(),
  [LastUpdatedDateTime]         DATETIME2(3) NOT NULL DEFAULT GETDATE(),
  [IsActive]              BIT          NOT NULL DEFAULT 1,

  CONSTRAINT PK_TelegramUserChats PRIMARY KEY (BotId, ChatId)
);

-- Recent/active fetch
CREATE INDEX IX_TelegramUserChats_Bot_LastSeen
  ON dbo.TelegramUserChats (BotId, LastSeenDateTime DESC);

-- Helpful lookups
CREATE UNIQUE INDEX IX_TelegramUserChats_Bot_TelegramUserId
  ON dbo.TelegramUserChats (BotId, TelegramUserId);

CREATE INDEX IX_TelegramUserChats_Bot_Username
  ON dbo.TelegramUserChats (BotId, Username);
