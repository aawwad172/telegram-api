using Telegram.API.Domain.Entities;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface IBotRepository : ICRUD<Bot, int>
{
    Task<Bot?> GetByIdAsync(int botId, int customerId, CancellationToken cancellationToken = default);
    Task<bool> UpdateBotActivityAsync(int botId, bool isActive, CancellationToken cancellationToken = default);
}
