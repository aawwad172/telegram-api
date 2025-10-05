using Telegram.API.Application.CQRS.Commands.Message;
using Telegram.API.Domain.Entities.Message;
using Telegram.API.Domain.Enums;

namespace Telegram.API.Application.Utilities;

public static class BulkHelpers
{
    public static IEnumerable<CampaignMessageItem> TryRemoveDuplicates(PortalSendCampaignMessageCommand request, IEnumerable<CampaignMessageItem> items)
    {
        if (request.RemoveDuplicates is null
                || !request.RemoveDuplicates.HasValue
                || request.RemoveDuplicates is not true
        )
        {
            return items;
        }

        items = items.Select(i => i.PhoneNumber)
            .Distinct()
            .Select(pn => new CampaignMessageItem(pn));

        return items;
    }

    public static IEnumerable<BatchMessageItem> TryRemoveDuplicates(PortalSendBatchMessageCommand request, IEnumerable<BatchMessageItem> items)
    {
        if (request.RemoveDuplicates is null
                || !request.RemoveDuplicates.HasValue
                || request.RemoveDuplicates is not true
        )
        {
            return items;
        }
        items = items.Select(p => new { p.PhoneNumber, p.MessageText })
            .Distinct()
            .Select(bm => new BatchMessageItem(bm.PhoneNumber, bm.MessageText));

        return items;
    }

    public static List<TelegramMessagePackage<CampaignMessage>> SplitList(
        PortalSendCampaignMessageCommand command,
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

            string campaignId = $"{decryptedCustomerId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}_Campaign#{i + 1}";

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


    public static List<TelegramMessagePackage<BatchMessage>> SplitList(
        PortalSendBatchMessageCommand command,
        IDictionary<string, string?> phoneToChat,
        int batchSize,
        int minutesGap,
        int decryptedCustomerId)
    {
        List<TelegramMessagePackage<BatchMessage>> batches = new();

        List<BatchMessage> fullList = new(command.Items.Count());
        foreach (BatchMessageItem item in command.Items)
        {
            phoneToChat.TryGetValue(item.PhoneNumber, out string? chatId);

            fullList.Add(new BatchMessage
            {
                // Intentionally allow null ChatId to mark unsubscribed numbers
                // for reporting and invoice exclusion purposes
                ChatId = chatId!,
                MessageText = item.MessageText,
                PhoneNumber = item.PhoneNumber
            });
        }

        int totalItems = fullList.Count;
        int totalBatches = (int)Math.Ceiling((double)totalItems / batchSize);

        for (int i = 0; i < totalBatches; i++)
        {
            List<BatchMessage> batchItems = fullList.Skip(i * batchSize).Take(batchSize).ToList();

            DateTime time = command.ScheduledDatetime is not null
                    ? command.ScheduledDatetime.Value
                    : DateTime.Now;

            DateTime scheduledTime = minutesGap > 0 ? time.AddMinutes(i * minutesGap) : time;

            string campaignId = $"{decryptedCustomerId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}_Batch#{i + 1}";

            TelegramMessagePackage<BatchMessage> batch = new()
            {
                CustomerId = decryptedCustomerId,
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

