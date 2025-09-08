using Telegram.API.Domain.Entities;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface ITelegramUserChatsRepository
{
    /// <summary>
    /// Insert a new chat or update it if it already exists.
    /// </summary>
    Task AddAsync(
        int botId,
        string chatId,
        string? phoneNumber,
        long telegramUserId,
        string? username,
        string? firstName,
        bool isActive,
        CancellationToken ct);

    /// <summary>
    /// Mark a chat as inactive (e.g., user blocked the bot).
    /// </summary>
    Task DeactivateAsync(
        int botId,
        string chatId,
        CancellationToken ct);

    /// <summary>
    /// Get a chat by BotId + ChatId.
    /// </summary>
    Task<TelegramUserChat?> GetAsync(
        int botId,
        string chatId,
        CancellationToken ct);

    /// <summary>
    /// Get all active chats for a bot (for broadcast/campaigns).
    /// </summary>
    Task<IReadOnlyCollection<TelegramUserChat>> GetActiveChatsAsync(
        int botId,
        CancellationToken ct);
}

