using Mapster;
using MediatR;
using Telegram.API.Application.CQRS.Commands.Message;
using Telegram.API.Domain.Entities.Bot;
using Telegram.API.Domain.Entities.Message;
using Telegram.API.Domain.Entities.User;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Application.CQRS.CommandHandlers;

public class SendCampaignMessageCommandHandler(
    IMessageRepository messageRepository,
    IAuthenticationService authenticationService,
    IRecipientRepository recipientRepository)
    : IRequestHandler<SendCampaignMessageCommand, SendCampaignMessageCommandResult>
{
    private readonly IMessageRepository _messageRepository = messageRepository;
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly IRecipientRepository _recipientRepository = recipientRepository;
    public async Task<SendCampaignMessageCommandResult> Handle(SendCampaignMessageCommand request, CancellationToken cancellationToken)
    {
        Customer customer = await _authenticationService.AuthenticateAsync(request.Username, request.Password);

        Bot bot = await _authenticationService.ValidateBotIdAsync(request.BotId, customer.Id);

        TelegramMessagePackage<CampaignMessage> campaignMessage = ((customer, bot), request).Adapt<TelegramMessagePackage<CampaignMessage>>();

        IEnumerable<string> phones = request.Items.Select(x => x.PhoneNumber)
                                                  .Where(x => !string.IsNullOrWhiteSpace(x))
                                                  .Distinct(StringComparer.Ordinal);

        IDictionary<string, string?> phoneToChat = await _recipientRepository.GetChatIdsAsync(phones, bot.Id);

        // 3) Build messages without further DB calls
        List<CampaignMessage> messages = new(request.Items.Count());
        foreach (CampaignMessageItem item in request.Items)
        {
            phoneToChat.TryGetValue(item.PhoneNumber, out string? chatId);

            messages.Add(new CampaignMessage
            {
                ChatId = chatId!, // null if not mapped
                PhoneNumber = item.PhoneNumber,
            });
        }

        // assign to your container
        campaignMessage.Items = messages;

        if (campaignMessage.Items.Count == 0)
        {
            throw new EmptyMessagesPackageException($"{nameof(campaignMessage.Items)} The messages collection cannot be null or empty.");
        }

        await _messageRepository.SendBatchMessagesAsync(campaignMessage);
        return new SendCampaignMessageCommandResult(campaignMessage.CampaignId);
    }
}
