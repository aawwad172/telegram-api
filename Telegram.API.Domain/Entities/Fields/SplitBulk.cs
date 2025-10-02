namespace Telegram.API.Domain.Entities.Fields;

public class SplitBulk
{
    public int BatchSize { get; set; }                // phones per batch
    public int MinutesBetweenBatches { get; set; }    // delay between batches
}
