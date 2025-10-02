using Telegram.API.Application.CQRS.Commands.Message;
using Telegram.API.Domain.Entities.Message;
using Telegram.API.Domain.Enums;

namespace Telegram.API.Application.Utilities;

public class BulkHelpers
{
    public static IEnumerable<CampaignMessageItem> TryRemoveDuplicates(PortalSendCampaignCommand request, IEnumerable<CampaignMessageItem> items)
    {
        if (request.RemoveDuplicates is not null
                && request.RemoveDuplicates.HasValue
                && request.RemoveDuplicates is true
        )
        {
            items = items.Select(i => i.PhoneNumber)
                .Distinct()
                .Select(pn => new CampaignMessageItem(pn));
        }

        return items;
    }

    public static List<TelegramMessagePackage<CampaignMessage>> SplitList(
        PortalSendCampaignCommand command,
        IDictionary<string, string?> phoneToChat,
        int batchSize,
        int minutesGap,
        int decryptedCustomerId)
    {
        List<TelegramMessagePackage<CampaignMessage>> batches = new();

        List<CampaignMessage> fullList = phoneToChat
            .Select(kv => new CampaignMessage
            {
                PhoneNumber = kv.Key,
                ChatId = kv.Value ?? null!,
            })
            .ToList();

        int totalItems = fullList.Count;
        int totalBatches = (int)Math.Ceiling((double)totalItems / batchSize);

        for (int i = 0; i < totalBatches; i++)
        {
            List<CampaignMessage> batchItems = fullList.Skip(i * batchSize).Take(batchSize).ToList();

            DateTime time = command.ScheduledDatetime is not null
                    ? command.ScheduledDatetime.Value
                    : DateTime.Now;

            DateTime scheduledTime = minutesGap > 0 ? time.AddMinutes(i * minutesGap) : time;

            string campaignId = $"{decryptedCustomerId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}_Batch#{i + 1}";

            TelegramMessagePackage<CampaignMessage> batch = new()
            {
                CustomerId = decryptedCustomerId,
                MessageText = command.MessageText,
                BotId = command.BotId,
                CampaignId = campaignId,
                CampDescription = command.CampDescription ?? null!,
                ScheduledSendDateTime = scheduledTime,
                IsSystemApproved = true,
                MessageType = MessageTypeEnum.C.ToString(),
                Priority = (int)MessagePriorityEnum.PortalCampaignMessage,
                Items = batchItems
            };

            batches.Add(batch);
        }

        return batches;
    }
}
