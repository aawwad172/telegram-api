using Telegram.API.Domain.Entities.Bot;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface IBotRepository
{
    Task<Bot?> GetByIdAsync(int botId, int customerId, CancellationToken cancellationToken = default);
    Task<bool> UpdateBotActivityAsync(int botId, bool isActive, CancellationToken cancellationToken = default);
    Task<Bot?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
    Task<Bot?> CreateAsync(Bot entity, CancellationToken cancellationToken = default);
}
