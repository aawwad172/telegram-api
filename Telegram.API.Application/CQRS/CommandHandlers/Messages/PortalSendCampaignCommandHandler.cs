using System.Net.NetworkInformation;
using MediatR;
using Telegram.API.Application.CQRS.Commands.Message;
using Telegram.API.Application.Utilities;
using Telegram.API.Domain.Entities.Bot;
using Telegram.API.Domain.Entities.Message;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Application.CQRS.CommandHandlers.Messages;

public class PortalSendCampaignCommandHandler(
    IRecipientRepository recipientRepository,
    IMessageRepository messageRepository,
    IAuthenticationService authenticationService)
    : IRequestHandler<PortalSendCampaignCommand, PortalSendCampaignCommandResult>
{
    private readonly IRecipientRepository _recipientRepository = recipientRepository;
    private readonly IMessageRepository _messageRepository = messageRepository;
    private readonly IAuthenticationService _authenticationService = authenticationService;
    public async Task<PortalSendCampaignCommandResult> Handle(PortalSendCampaignCommand request, CancellationToken cancellationToken)
    {
        string stringCustomerId = _authenticationService.Decrypt(request.EncryptedCustomerId);

        if (!int.TryParse(_authenticationService.Decrypt(request.EncryptedCustomerId), out var customerId))
            throw new UnauthenticatedException("Invalid customer id.");

        Bot bot = await _authenticationService.ValidateBotIdAsync(request.BotId, customerId, cancellationToken);

        // 0) Validate split options
        int batchSize = request.SplitBulk?.BatchSize > 0 ? request.SplitBulk.BatchSize : int.MaxValue; // no split if not set
        int minutesGap = Math.Max(0, request.SplitBulk?.MinutesBetweenBatches ?? 0);
        TimeSpan interval = TimeSpan.FromMinutes(minutesGap);

        // Represent the full list batch with or without duplicates removed depending on the flag
        IEnumerable<CampaignMessageItem> fullList = BulkHelpers.TryRemoveDuplicates(request, request.Items);

        IEnumerable<string> phones = fullList.Select(x => x.PhoneNumber).Where(x => !string.IsNullOrWhiteSpace(x));

        IDictionary<string, string?> phoneToChat = await _recipientRepository.GetChatIdsAsync(phones, bot.Id);

        List<TelegramMessagePackage<CampaignMessage>> batches = BulkHelpers.SplitList(request, phoneToChat, batchSize, minutesGap, customerId);

        if (batches.Count == 0)
        {
            throw new EmptyMessagesPackageException($"{nameof(batches)} The batches collection cannot be null or empty.");
        }

        foreach (TelegramMessagePackage<CampaignMessage> batch in batches)
        {
            await _messageRepository.SendBatchMessagesAsync(batch);
        }

        return new PortalSendCampaignCommandResult(batches.First().CampaignId);
    }
}
