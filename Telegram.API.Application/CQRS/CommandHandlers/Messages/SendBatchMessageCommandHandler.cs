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

public class SendBatchMessageCommandHandler(
    IAuthenticationService authenticatedService,
    IMessageRepository messageRepository,
    IRecipientRepository recipientRepository)
    : IRequestHandler<SendBatchMessagesCommand, SendBatchMessageCommandResult>
{
    private readonly IAuthenticationService _authenticationService = authenticatedService;
    private readonly IMessageRepository _messageRepository = messageRepository;
    private readonly IRecipientRepository _recipientRepository = recipientRepository;
    public async Task<SendBatchMessageCommandResult> Handle(SendBatchMessagesCommand request, CancellationToken cancellationToken)
    {
        Customer customer = await _authenticationService.AuthenticateAsync(request.Username, request.Password);

        if (customer is null)
            throw new UnauthenticatedException("Invalid username or password.");

        Bot bot = await _authenticationService.ValidateBotIdAsync(request.BotId, customer.CustomerId);

        TelegramMessagePackage<BatchMessage> batchMessages = ((customer, bot), request).Adapt<TelegramMessagePackage<BatchMessage>>();

        // 1) Get all phone numbers once
        IEnumerable<string> phones = request.Items.Select(x => x.PhoneNumber)
                                                  .Where(x => !string.IsNullOrWhiteSpace(x))
                                                  .Distinct(StringComparer.Ordinal);

        // 2) One DB call
        IDictionary<string, string?> phoneToChat = await _recipientRepository.GetChatIdsAsync(phones, bot.Id);

        // 3) Build messages without further DB calls
        List<BatchMessage> messages = new(request.Items.Count());
        foreach (BatchMessageItem item in request.Items)
        {
            phoneToChat.TryGetValue(item.PhoneNumber, out string? chatId);

            messages.Add(new BatchMessage
            {
                ChatId = chatId!, // null if not mapped
                MessageText = item.MessageText,
                PhoneNumber = item.PhoneNumber
            });
        }

        // assign to your container
        batchMessages.Items = messages;

        if (!(batchMessages.Items?.Any() ?? false))
        {
            throw new EmptyMessagesPackageException($"{nameof(batchMessages.Items)} The messages collection cannot be null or empty.");
        }

        await _messageRepository.SendBatchMessagesAsync(batchMessages);

        return new SendBatchMessageCommandResult(batchMessages.CampaignId);
    }
}
