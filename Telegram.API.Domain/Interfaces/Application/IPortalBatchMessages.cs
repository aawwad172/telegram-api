using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Domain.Interfaces.Application;

public interface IPortalBatchMessages : IHasEncryptedCustomerId, IHasBotId, IHasCampDescription, IHasSchedule, IHasPortalOptions
{

}
