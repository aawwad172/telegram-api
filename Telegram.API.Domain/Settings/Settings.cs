namespace Telegram.API.Domain.Settings;

public class DbSettings
{
    public string ConStr { get; set; } = string.Empty;
    public string CommandTimeOut { get; set; } = "30";
}

public class AppSettings
{
    public string LogPath { get; set; } = string.Empty;
    public int LogFlushInterval { get; set; } = 0;
    public string DomainName { get; set; } = string.Empty;
}

public class TelegramOptions
{
    public string BulkFolderPath { get; set; } = string.Empty;
    public string TelegramApiBaseUrl { get; set; } = string.Empty;
}