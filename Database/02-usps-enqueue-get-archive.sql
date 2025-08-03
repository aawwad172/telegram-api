/*******************************************
 * 2.1) usp_EnqueueOrArchiveIfDuplicate
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_EnqueueOrArchiveIfDuplicate
  @CustomerId  		INT,
  @ChatId      		NVARCHAR(50),
  @BotKey      		NVARCHAR(100),
  @MessageText 		NVARCHAR(MAX),
  -- @PhoneNumber      NVARCHAR(20),
  @MsgType     		CHAR, 
  @CampaignId  		NVARCHAR(50), -- Empty String if not required
  @CampDescription 	NVARCHAR(512), -- Empty String if not required
  @Priority    		SMALLINT,
  @ScheduledSendDateTime DATETIME = NULL,  -- Auto inserted in case of one message
  @IsSystemApproved BIT
--@Paused = 0 always zero and it will change when the portal change them
AS
BEGIN
  SET NOCOUNT ON;
  SET @ScheduledSendDateTime = ISNULL(@ScheduledSendDateTime, GETDATE());

  DECLARE 
    @hashedMsg   BINARY(32) = HASHBYTES(
             'SHA2_256',
             @ChatId + N'|' + @BotKey + N'|' + @MessageText
           );
    -- If caller omitted it, fill it with GETDATE()

  -- always enqueue; trigger will handle RecentMessages & archiving
 INSERT INTO dbo.ReadyTable
    (ChatId
    ,MessageText
    ,BotKey
    ,ScheduledSendDateTime     -- ← add this
    ,ReceivedDateTime
    ,MessageHash
    ,Priority
    ,CustomerId
    ,MsgType
    ,CampaignId
    ,CampDescription
    ,IsSystemApproved
    ,Paused)
  VALUES
    (@ChatId
    ,@MessageText
    ,@BotKey
    ,@ScheduledSendDateTime   -- ← use your variable here
    ,GETDATE()
    ,@hashedMsg
    ,@Priority
    ,@CustomerId
    ,@MsgType
    ,@CampaignId
    ,@CampDescription
    ,@IsSystemApproved
    ,0);

  SELECT SCOPE_IDENTITY() AS NewId;
END;
GO



/*******************************************
 * 2.4) usp_GetUserByUsername
 *******************************************/
ALTER PROCEDURE [dbo].[usp_GetUserByUsername]
    @Username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CustId, UserName, Password, RequireSystemApprove,
                     RequireAdminApprove, IsActive, IsBlocked, IsTelegramActive
                     FROM A2A_iMessaging.dbo.Table_UserSMSProfile
                     WHERE UserName = @Username
END;
GO


/*******************************************
 * 2.5) usp_GetChatId
 *******************************************/
CREATE OR ALTER PROCEDURE dbo.usp_GetChatId
  @BotKey       NVARCHAR(100),
  @PhoneNumber  NVARCHAR(20),
  @ChatId       NVARCHAR(50) OUTPUT
AS
BEGIN
  SET NOCOUNT ON;

  -- assign the output param directly
  SELECT
    @ChatId = ChatId
  FROM 
    dbo.BotChatMapping WITH (INDEX(PK_BotChatMapping))
  WHERE 
    PhoneNumber = @PhoneNumber
    AND BotKey   = @BotKey;
END;
GO

