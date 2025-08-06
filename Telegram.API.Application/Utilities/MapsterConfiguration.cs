using Mapster;
using Telegram.API.Application.CQRS.Queries;
using Telegram.API.Domain.Entities;

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

        TypeAdapterConfig<User, SubscriptionInfoQueryResult>.NewConfig()
            .Map(dest => dest.ChatId, src => src.ChatId)
            .Map(dest => dest.CreationDate, src => src.CreationDate)
            .Map(dest => dest.Subscribed, src => !string.IsNullOrWhiteSpace(src.ChatId));
    }
}