Telegram Gateway Jobs

USE msdb;
GO

-- 3.1 Drop existing job & schedule if present
IF EXISTS (SELECT 1 FROM dbo.sysjobs_view WHERE name = N'Purge RecentMessages')
  EXEC dbo.sp_delete_job @job_name = N'Purge RecentMessages', @delete_unused_schedule = 1;
GO
IF EXISTS (SELECT 1 FROM dbo.sysschedules WHERE name = N'Every 1 Minute')
  EXEC dbo.sp_delete_schedule @schedule_name = N'Every 1 Minute';
GO

-- 3.2 Create the job
EXEC dbo.sp_add_job
  @job_name    = N'Purge RecentMessages',
  @enabled     = 1,
  @description = N'Delete entries older than 5 minutes from RecentMessages';
GO

-- 3.3 Add the T-SQL step
EXEC dbo.sp_add_jobstep
  @job_name      = N'Purge RecentMessages',
  @step_name     = N'Delete aged rows',
  @subsystem     = N'TSQL',
  @database_name = N'YourDatabaseName', -- Add your DB name here
  @command       = N'
    DELETE FROM dbo.RecentMessages
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
  @job_name      = N'Purge RecentMessages',
  @schedule_name = N'Every 1 Minute';
GO

-- 3.6 Target local server
EXEC dbo.sp_add_jobserver
  @job_name    = N'Purge RecentMessages',
  @server_name = N'(LOCAL)';
GO
