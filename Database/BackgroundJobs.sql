-- Telegram Gateway Jobs

USE msdb;
GO

-- 3.1 Drop existing job & schedule if present
IF EXISTS (SELECT 1 FROM dbo.sysjobs_view WHERE name = N'Purge Table_Telegram_RecentMessages')
  EXEC dbo.sp_delete_job @job_name = N'Purge Table_Telegram_RecentMessages', @delete_unused_schedule = 1;
GO
IF EXISTS (SELECT 1 FROM dbo.sysschedules WHERE name = N'Every 1 Minute')
  EXEC dbo.sp_delete_schedule @schedule_name = N'Every 1 Minute';
GO

-- 3.2 Create the job
EXEC dbo.sp_add_job
  @job_name    = N'Purge Table_Telegram_RecentMessages',
  @enabled     = 1,
  @description = N'Delete entries older than 5 minutes from Table_Telegram_RecentMessages';
GO

-- 3.3 Add the T-SQL step
EXEC dbo.sp_add_jobstep
  @job_name      = N'Purge Table_Telegram_RecentMessages',
  @step_name     = N'Delete aged rows',
  @subsystem     = N'TSQL',
  @database_name = N'YourDatabaseName', -- Add your DB name here
  @command       = N'
    DELETE FROM dbo.Table_Telegram_RecentMessages
    WHERE ReceivedDateTime < DATEADD(MINUTE, -5, GETDATE());
  ';
GO

-- 3.4 Schedule it every minute
EXEC dbo.sp_add_schedule
  @schedule_name        = N'Every 1 Minute',
  @enabled              = 1,
  @freq_type            = 4,    -- daily
  @freq_interval        = 1,    -- every day
  @freq_subday_type     = 4,    -- minutes
  @freq_subday_interval = 2,    -- every 2 minute you can check as well
  @active_start_time    = 0;    -- midnight
GO

-- 3.5 Attach schedule to job
EXEC dbo.sp_attach_schedule
  @job_name      = N'Purge Table_Telegram_RecentMessages',
  @schedule_name = N'Every 1 Minute';
GO

-- 3.6 Target local server
EXEC dbo.sp_add_jobserver
  @job_name    = N'Purge Table_Telegram_RecentMessages',
  @server_name = N'(LOCAL)';
GO

/******************************************************/
-- Create the archiving stored procedure if not exists
/******************************************************/
-- Create a daily SQL Agent job to archive ReadyTable rows with missing ChatId
USE msdb;
-- Drop old job if exists
DECLARE @job_name       sysname = N'Archive Ready (Statuses -1,-2,-3) - Daily';
DECLARE @schedule_name  sysname = @job_name + N' Schedule';
IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = @job_name)
BEGIN
  EXEC msdb.dbo.sp_delete_job @job_name = @job_name;
END

-- Pick a valid owner: current login if it's a SQL login; else fall back to 'sa'
DECLARE @owner sysname = SUSER_SNAME();
IF NOT EXISTS (
    SELECT 1
    FROM master.sys.syslogins
    WHERE name = @owner AND isntgroup = 0  -- 0 = not a Windows group
)
    SET @owner = N'sa';

DECLARE @job_id UNIQUEIDENTIFIER;

-- Create job
EXEC msdb.dbo.sp_add_job
  @job_name          = @job_name,
  @enabled           = 1,
  @description       = N'Moves ReadyTable rows with StatusId in (-1,-2,-3) to ArchiveTable.',
  @owner_login_name  = @owner,
  @job_id            = @job_id OUTPUT;

-- Add the step
EXEC msdb.dbo.sp_add_jobstep
  @job_id          = @job_id,
  @step_id         = 1,
  @step_name       = N'Archive negative statuses',
  @subsystem       = N'TSQL',
  @database_name   = N'TelegramTestDB',      -- << your DB name
  @command         = N'
    BEGIN TRY
      EXEC dbo.usp_ArchiveReady_NegativeStatuses_Daily -- or 0 for single-shot
    END TRY
    BEGIN CATCH
      THROW;
    END CATCH
  ',
  @retry_attempts  = 3,
  @retry_interval  = 2,     -- minutes
  @on_fail_action  = 2;     -- Quit with failure

-- Create a dedicated schedule (unique name so we don’t collide)
IF EXISTS (SELECT 1 FROM msdb.dbo.sysschedules WHERE name = @schedule_name)
BEGIN
  -- detach any jobs then drop the schedule to recreate cleanly
  DECLARE @sid INT;
  SELECT @sid = schedule_id FROM msdb.dbo.sysschedules WHERE name = @schedule_name;
  EXEC msdb.dbo.sp_detach_schedule @schedule_id = @sid;
  EXEC msdb.dbo.sp_delete_schedule @schedule_id = @sid;
END

EXEC msdb.dbo.sp_add_schedule
  @schedule_name     = @schedule_name,
  @freq_type         = 4,          -- daily
  @freq_interval     = 1,
  @active_start_time = 020000;     -- 02:00:00

EXEC msdb.dbo.sp_attach_schedule
  @job_id        = @job_id,
  @schedule_name = @schedule_name;

-- Target server (use the actual server name/instance)
EXEC msdb.dbo.sp_add_jobserver
  @job_id      = @job_id,
  @server_name = @@SERVERNAME;     -- avoids (LOCAL) mismatch on named instances

