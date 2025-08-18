namespace Telegram.API.Domain.Interfaces.Application;

public interface IBatch<TItem>
{
    List<TItem> Items { get; }
}
