using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Domain.Interfaces.Application;

public interface IAuthenticatedBotRequest : ICredentials, IHasBotKey { }
