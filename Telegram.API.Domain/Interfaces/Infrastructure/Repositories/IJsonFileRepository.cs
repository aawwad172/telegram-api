namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface IJsonFileRepository
{
    Task SaveToFileAsync<T>(IEnumerable<T> items, string fullPath, CancellationToken ct = default);
}
