namespace Telegram.API.Domain.Interfaces.Application;

public interface IBatch<TItem>
{
    IEnumerable<TItem> Items { get; }
}
