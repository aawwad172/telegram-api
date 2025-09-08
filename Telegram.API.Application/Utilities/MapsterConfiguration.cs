using Mapster;
using Telegram.API.Application.CQRS.Commands;
using Telegram.API.Application.CQRS.Queries;
using Telegram.API.Domain.Entities;
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

        TypeAdapterConfig<TelegramUserChat, SubscriptionInfoQueryResult>
            .NewConfig()
            .Map(dest => dest.ChatId, src => src.ChatId)
            .Map(dest => dest.CreationDate, src => src.CreationDateTime)
            .Map(dest => dest.Subscribed, _ => true);

        TypeAdapterConfig<(((Customer customer, TelegramUserChat user) customerUser, Bot bot) data, SendMessageCommand request), TelegramMessage>
            .NewConfig()
            .Map(dest => dest.CustomerId, src => src.data.customerUser.customer.CustomerId)
            .Map(dest => dest.ChatId, src => src.data.customerUser.user.ChatId!)
            .Map(dest => dest.BotId, src => src.data.bot.BotId)
            .Map(dest => dest.MessageText, src => src.request.MessageText)
            .Map(dest => dest.PhoneNumber, src => src.request.PhoneNumber)
            .Map(dest => dest.MessageType, _ => MessageTypeEnum.A.ToString())
            .Map(dest => dest.IsSystemApproved, _ => true)
            .Map(dest => dest.Priority, _ => MessagePriorityEnum.SingleMessage);

        TypeAdapterConfig<((Customer customer, Bot bot) customerBot, SendBatchMessagesCommand request), TelegramMessagePackage<BatchMessage>>
            .NewConfig()
            .Map(dest => dest.CustomerId, src => src.customerBot.customer.CustomerId)
            .Map(dest => dest.BotId, src => src.customerBot.bot.BotId)
            .Map(dest => dest.IsSystemApproved, _ => true)
            .Map(dest => dest.MessageType, _ => MessageTypeEnum.AF.ToString())
            .Map(dest => dest.CampaignId, src => $"{src.customerBot.customer.CustomerId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}")
            .Map(dest => dest.CampDescription, src => src.request.CampDescription ?? string.Empty)
            .Map(dest => dest.ScheduledSendDateTime, src => src.request.ScheduledDatetime)
            .Map(dest => dest.Priority, _ => MessagePriorityEnum.BatchMessage);

        TypeAdapterConfig<((Customer customer, Bot bot) customerBot, SendCampaignMessageCommand request), TelegramMessagePackage<CampaignMessage>>
            .NewConfig()
            .Map(dest => dest.CustomerId, src => src.customerBot.customer.CustomerId)
            .Map(dest => dest.BotId, src => src.customerBot.bot.BotId)
            .Map(dest => dest.IsSystemApproved, _ => true)
            .Map(dest => dest.MessageText, src => src.request.MessageText)
            .Map(dest => dest.MessageType, _ => MessageTypeEnum.AC.ToString())
            .Map(dest => dest.CampaignId, src => $"{src.customerBot.customer.CustomerId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}")
            .Map(dest => dest.CampDescription, src => src.request.CampDescription ?? string.Empty)
            .Map(dest => dest.ScheduledSendDateTime, src => src.request.ScheduledDatetime)
            .Map(dest => dest.Priority, _ => MessagePriorityEnum.CampaignMessage);
    }
}