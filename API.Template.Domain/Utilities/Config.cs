namespace API.Template.Domain.Utilities;

public static class Config
{
    public static ConnectionStrings ConnectionStrings { get; set; } = default!;
    public static AppConfig AppConfig { get; set; } = default!;
    public static TelegramGatewayConfig TelegramGatewayConfig { get; set; } = default!;
}

public class ConnectionStrings
{
    public required string ConStr { get; set; } = string.Empty;
    public required string CommandTimeOut { get; set; } = "30";
}

public class AppConfig
{
    public required string LogPath { get; set; } = string.Empty;
    public required int LogFlushInterval { get; set; } = 0;
}

public class TelegramGatewayConfig
{
    public required string SendTelegramMessageUrl { get; set; } = string.Empty;
}

public class SendSMSRequest
{
    public string Senderid { get; set; } = string.Empty;
    public string Numbers { get; set; } = string.Empty;
    public string Msg { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
}
