using Telegram.API.Domain.Entities;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface IBotRepository : ICRUD<Bot, int>
{
    Task<Bot?> GetBotByKeyAsync(string EncryptedBotKey, int customerId);
}
