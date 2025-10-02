using Mapster;
using Telegram.API.Application.CQRS.Commands.Message;
using Telegram.API.Application.CQRS.Queries;
using Telegram.API.Domain.Entities.Bot;
using Telegram.API.Domain.Entities.Message;
using Telegram.API.Domain.Entities.User;
using Telegram.API.Domain.Enums;

namespace Telegram.API.Application.Utilities;

public static class MapsterConfiguration
{
    public static void RegisterMappings()
    {
        // Example: Mapping a User entity to a UserDto

        // TypeAdapterConfig<Entity, EntityDTO>.NewConfig()
        //     .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}")
        //     .Ignore(dest => dest.PasswordHash); // Ignore sensitive data

        //  Add additional mappings as needed

        // SubscriptionInfoQueryResult Mapping
        TypeAdapterConfig<Recipient, SubscriptionInfoQueryResult>
            .NewConfig()
            .Map(dest => dest.ChatId, src => src.ChatId)
            .Map(dest => dest.CreationDate, src => src.CreationDateTime)
            .Map(dest => dest.Subscribed, _ => true);

        // SendMessageCommand to TelegramMessage Mapping
        TypeAdapterConfig<(((Customer customer, Recipient recipient) customerUser, Bot bot) data, SendMessageCommand request), TelegramMessage>
            .NewConfig()
            .Map(dest => dest.CustomerId, src => src.data.customerUser.customer.Id)
            .Map(dest => dest.ChatId, src => src.data.customerUser.recipient.ChatId!)
            .Map(dest => dest.BotId, src => src.data.bot.Id)
            .Map(dest => dest.MessageText, src => src.request.MessageText)
            .Map(dest => dest.PhoneNumber, src => src.request.PhoneNumber)
            .Map(dest => dest.MessageType, _ => MessageTypeEnum.A.ToString())
            .Map(dest => dest.IsSystemApproved, _ => true)
            .Map(dest => dest.Priority, _ => MessagePriorityEnum.SingleMessage);

        // SendBatchMessagesCommand to TelegramMessagePackage<BatchMessage> Mapping
        TypeAdapterConfig<((Customer customer, Bot bot) customerBot, SendBatchMessagesCommand request), TelegramMessagePackage<BatchMessage>>
            .NewConfig()
            .Map(dest => dest.CustomerId, src => src.customerBot.customer.Id)
            .Map(dest => dest.BotId, src => src.customerBot.bot.Id)
            .Map(dest => dest.IsSystemApproved, _ => true)
            .Map(dest => dest.MessageType, _ => MessageTypeEnum.AF.ToString())
            .Map(dest => dest.CampaignId, src => $"{src.customerBot.customer.Id}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}")
            .Map(dest => dest.CampDescription, src => src.request.CampDescription ?? string.Empty)
            .Map(dest => dest.ScheduledSendDateTime, src => src.request.ScheduledDatetime)
            .Map(dest => dest.Priority, _ => MessagePriorityEnum.BatchMessage);

        // SendCampaignMessageCommand to TelegramMessagePackage<CampaignMessage> Mapping
        TypeAdapterConfig<((Customer customer, Bot bot) customerBot, SendCampaignMessageCommand request), TelegramMessagePackage<CampaignMessage>>
            .NewConfig()
            .Map(dest => dest.CustomerId, src => src.customerBot.customer.Id)
            .Map(dest => dest.BotId, src => src.customerBot.bot.Id)
            .Map(dest => dest.IsSystemApproved, _ => true)
            .Map(dest => dest.MessageText, src => src.request.MessageText)
            .Map(dest => dest.MessageType, _ => MessageTypeEnum.AC.ToString())
            .Map(dest => dest.CampaignId, src => $"{src.customerBot.customer.Id}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}")
            .Map(dest => dest.CampDescription, src => src.request.CampDescription ?? string.Empty)
            .Map(dest => dest.ScheduledSendDateTime, src => src.request.ScheduledDatetime)
            .Map(dest => dest.Priority, _ => MessagePriorityEnum.CampaignMessage);

        // PortalSendCampaignCommand to TelegramMessagePackage<CampaignMessage> Mapping
        TypeAdapterConfig<((int customerId, Bot bot) customerBot, PortalSendCampaignCommand request), TelegramMessagePackage<CampaignMessage>>
            .NewConfig()
            .Map(dest => dest.CustomerId, src => src.customerBot.customerId)
            .Map(dest => dest.BotId, src => src.customerBot.bot.Id)
            .Map(dest => dest.IsSystemApproved, _ => true)
            .Map(dest => dest.MessageText, src => src.request.MessageText)
            .Map(dest => dest.MessageType, _ => MessageTypeEnum.C.ToString())
            .Map(dest => dest.CampaignId, src => $"{src.customerBot.customerId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}")
            .Map(dest => dest.CampDescription, src => src.request.CampDescription ?? string.Empty)
            .Map(dest => dest.ScheduledSendDateTime, src => src.request.ScheduledDatetime)
            .Map(dest => dest.Priority, _ => MessagePriorityEnum.PortalCampaignMessage);

        // PortalSendBatchMessagesCommand to TelegramMessagePackage<BatchMessage> Mapping
        // TypeAdapterConfig<((int customerId, Bot bot) customerBot, PortalSendBatchCommand request), TelegramMessagePackage<BatchMessage>>
        //     .NewConfig()
        //     .Map(dest => dest.CustomerId, src => src.customerBot.customerId)
        //     .Map(dest => dest.BotId, src => src.customerBot.bot.Id)
        //     .Map(dest => dest.IsSystemApproved, _ => true)
        //     .Map(dest => dest.MessageType, _ => MessageTypeEnum.CF.ToString())
        //     .Map(dest => dest.CampaignId, src => $"{src.customerBot.customerId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}")
        //     .Map(dest => dest.CampDescription, src => src.request.CampDescription ?? string.Empty)
        //     .Map(dest => dest.ScheduledSendDateTime, src => src.request.ScheduledDatetime)
        //     .Map(dest => dest.Priority, _ => MessagePriorityEnum.PortalBatchMessage);
    }
}