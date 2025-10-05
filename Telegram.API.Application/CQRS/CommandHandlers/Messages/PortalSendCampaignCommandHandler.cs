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
    : IRequestHandler<PortalSendCampaignMessageCommand, PortalSendCampaignCommandResult>
{
    private readonly IRecipientRepository _recipientRepository = recipientRepository;
    private readonly IMessageRepository _messageRepository = messageRepository;
    private readonly IAuthenticationService _authenticationService = authenticationService;
    public async Task<PortalSendCampaignCommandResult> Handle(PortalSendCampaignMessageCommand request, CancellationToken cancellationToken)
    {
        string decrypted = _authenticationService.Decrypt(request.EncryptedCustomerId);

        if (!int.TryParse(decrypted, out var customerId))
            throw new UnauthenticatedException("Invalid customer id.");

        Bot bot = await _authenticationService.ValidateBotIdAsync(request.BotId, customerId, cancellationToken);

        // 0) Validate split options
        int batchSize = request.SplitBulk is { BatchSize: > 0 } s ? s.BatchSize : int.MaxValue; // no split if not set
        int minutesGap = Math.Max(0, request.SplitBulk?.MinutesBetweenBatches ?? 0);

        // Represent the full list batch with or without duplicates removed depending on the flag
        List<CampaignMessageItem> fullList = BulkHelpers.TryRemoveDuplicates(request, request.Items).ToList();
        PortalSendCampaignMessageCommand effectiveRequest = request with { Items = fullList };

        IEnumerable<string> phones = fullList.Select(x => x.PhoneNumber).Where(x => !string.IsNullOrWhiteSpace(x));

        IDictionary<string, string?> phoneToChat = await _recipientRepository.GetChatIdsAsync(phones, bot.Id, cancellationToken);

        List<TelegramMessagePackage<CampaignMessage>> batches = BulkHelpers.SplitList(effectiveRequest, phoneToChat, batchSize, minutesGap, customerId);

        if (batches.Count == 0)
        {
            throw new EmptyMessagesPackageException($"{nameof(batches)} The batches collection cannot be null or empty.");
        }

        foreach (TelegramMessagePackage<CampaignMessage> batch in batches)
        {
            await _messageRepository.SendBatchMessagesAsync(batch, cancellationToken);
        }

        return new PortalSendCampaignCommandResult(batches.First().CampaignId);
    }
}
