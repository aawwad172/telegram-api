using Telegram.API.Domain.Entities;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface IBotRepository
{
    Task<Bot?> GetBotByKey(string EncryptedBotKey, int customerId);
}
