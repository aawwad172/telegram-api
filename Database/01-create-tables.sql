/*******************************************
 * 1.1) ReadyTable: pending messages queue
 *******************************************/
IF OBJECT_ID('dbo.ReadyTable','U') IS NOT NULL
  DROP TABLE dbo.ReadyTable;
GO

CREATE TABLE dbo.ReadyTable
(
  ID           			INT					IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReadyTable PRIMARY KEY CLUSTERED,
  CustId			    INT				 	NOT NULL,
  ChatId       			NVARCHAR(50)   		NOT NULL,
  BotKey       			NVARCHAR(100)  		NOT NULL,
  PhoneNumber           NVARCHAR(20)        NOT NULL,
  MessageText  			NVARCHAR(MAX)  		NOT NULL,
  MsgType				CHAR(1)				NOT NULL,
  ReceivedDateTime		DATETIME       		NOT NULL, -- Auto Generated using GETDATE() in the SP.
  ScheduledSendDateTime DATETIME       		NOT NULL, -- Auto Generated using GETDATE() in the SP.
  MessageHash  			BINARY(32)     		NOT NULL, -- Auto Generated from the SP.
  Priority     			SMALLINT       		NOT NULL,
  CampaignId			NVARCHAR(50)				,
  CampDescription		NVARCHAR(512)				,
  IsSystemApproved		BIT					NOT NULL,
  Paused				BIT					NOT NULL,
);
GO

CREATE NONCLUSTERED INDEX IX_ReadyTable_MessageHash_ReadyDate
  ON dbo.ReadyTable (MessageHash, ID);
GO

CREATE NONCLUSTERED INDEX IX_ReadyTable_ID_Priority
  ON dbo.ReadyTable (ID, Priority);
GO


/*******************************************
 * 1.2) ArchiveTable: all sent or deduped msgs
 *******************************************/
IF OBJECT_ID('dbo.ArchiveTable', 'U') IS NOT NULL
  DROP TABLE dbo.ArchiveTable;
GO

-- 2) Recreate ArchiveTable without any FK constraint
CREATE TABLE dbo.ArchiveTable
(
  ID           		     INT    	      NOT NULL,  -- surrogate PK
  CustId		         INT	          NOT NULL,
  ChatId       		     NVARCHAR(50)     NOT NULL,
  BotKey       		     NVARCHAR(100)    NOT NULL,
  PhoneNumber            NVARCHAR(20)     NOT NULL,
  MessageText  		     NVARCHAR(MAX)    NOT NULL,
  MsgType		         CHAR(1)	      NOT NULL,
  ReceivedDateTime    	 DATETIME         NOT NULL,
  ScheduledSendDateTime  DATETIME         NOT NULL, -- Auto Generated using GETDATE() in the SP.
  GatewayDateTime        DATETIME         NOT NULL,
  MessageHash  		     BINARY(32)       NOT NULL,
  Priority     	         SMALLINT         NOT NULL,
  MobileCountry          NVARCHAR(10)     NOT NULL,
  CampaignId		     NVARCHAR(50)             ,
  CampDescription	     NVARCHAR(512)            ,
  IsSystemApproved	     BIT   	          NOT NULL,
  Paused		         BIT	          NOT NULL,
  CONSTRAINT PK_ArchiveTable_ID PRIMARY KEY CLUSTERED (ID)
);
GO

CREATE NONCLUSTERED INDEX IX_ArchiveTable_MessageHash
  ON dbo.ArchiveTable (MessageHash);
GO

/*******************************************
 * 1.3) RecentMessages: 5-minute dedupe window
 *******************************************/
IF OBJECT_ID('dbo.RecentMessages','U') IS NOT NULL
  DROP TABLE dbo.RecentMessages;
GO

CREATE TABLE dbo.RecentMessages
(
  MessageHash  		BINARY(32)     NOT NULL,
  ReceivedDateTime      DATETIME       NOT NULL,
  ReadyId      		INT            NOT NULL,
  CONSTRAINT PK_RecentMessages PRIMARY KEY CLUSTERED (MessageHash, ReadyId)
);
GO

CREATE NONCLUSTERED INDEX IX_RecentMessages_ReadyDate
  ON dbo.RecentMessages (ReceivedDateTime);
GO

CREATE NONCLUSTERED INDEX IX_RecentMessages_ReadyId
  ON dbo.RecentMessages (ReadyId);
GO


/*******************************************
 * 1.4) BotChatMapping: Mapping for ChatId along with PhoneNumber and BotKey
 *******************************************/
IF OBJECT_ID('dbo.BotChatMapping','U') IS NOT NULL
  DROP TABLE dbo.BotChatMapping;
GO

CREATE TABLE dbo.BotChatMapping
(
  PhoneNumber    NVARCHAR(20)    NOT NULL,
  BotKey         NVARCHAR(100)   NOT NULL,
  ChatId         NVARCHAR(50)    NOT NULL,
  CreationDate    DATETIME        NOT NULL,

  CONSTRAINT PK_BotChatMapping PRIMARY KEY CLUSTERED
    (PhoneNumber, BotKey)
);
GO

-- if you need to look up by ChatId later:
CREATE NONCLUSTERED INDEX IX_BotChatMapping_ChatId
  ON dbo.BotChatMapping(ChatId);
GO



