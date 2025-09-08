using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Entities.Telegram;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Clients;

public interface ITelegramClient
{
    /// <summary>
    /// Set the webhook for this bot (Telegram Bot API: setWebhook).
    /// </summary>
    Task<bool> SetWebhookAsync(
        string botToken,
        Uri url,
        string secretToken,
        IReadOnlyCollection<string> allowedUpdates,
        bool dropPendingUpdates,
        CancellationToken ct = default);

    /// <summary>
    /// Update/rotate the webhook (calls setWebhook under the hood).
    /// Use to change URL and/or rotate the secret.
    /// </summary>
    Task<bool> UpdateWebhookAsync(
        string botToken,
        Uri url,
        string newSecretToken,
        IReadOnlyCollection<string> allowedUpdates,
        bool dropPendingUpdates,
        CancellationToken ct = default);

    /// <summary>
    /// Delete the webhook (Telegram Bot API: deleteWebhook).
    /// </summary>
    Task<bool> DeleteWebhookAsync(
        string botToken,
        bool dropPendingUpdates,
        CancellationToken ct = default);

    /// <summary>b
    /// Get current webhook info (Telegram Bot API: getWebhookInfo).
    /// </summary>
    Task<TelegramResponse<WebhookInfo?>> GetWebhookInfoAsync(
        string botToken,
        CancellationToken ct = default);

    public Task<bool> SendTextWithContactButtonAsync(
        string botToken,
        string chatId,
        string text,
        string buttonText,
        CancellationToken ct = default);

    public Task<bool> SendTextAsync(
        string botToken,
        string chatId,
        string text,
        CancellationToken ct = default);
}
