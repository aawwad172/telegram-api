using System;
using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Domain.Interfaces.Application;

public interface IPortalCampaignMessage : IHasCustomerId, IHasBotId, IHasCampDescription, IHasRemoveDuplicates, IHasSchedule, IHasSplitBulk
{

}
