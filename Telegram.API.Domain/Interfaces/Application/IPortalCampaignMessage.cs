using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Domain.Interfaces.Application;

public interface IPortalCampaignMessage : IHasEncryptedCustomerId, IHasBotId, IHasCampDescription, IHasSchedule, IHasPortalOptions, IHasMessageText
{

}
