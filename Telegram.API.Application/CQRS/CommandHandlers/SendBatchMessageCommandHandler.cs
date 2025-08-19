using MediatR;
using Telegram.API.Application.CQRS.Commands;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Application.CQRS.CommandHandlers;

public class SendBatchMessageCommandHandler(
    IAuthenticationService authenticatedService,
    IMessageRepository messageRepository,
    IUserRepository userRepository)
    : IRequestHandler<SendBatchMessagesCommand, SendBatchMessageCommandResult>
{
    private readonly IAuthenticationService _authenticationService = authenticatedService;
    private readonly IMessageRepository _messageRepository = messageRepository;
    private readonly IUserRepository _userRepository = userRepository;
    public async Task<SendBatchMessageCommandResult> Handle(SendBatchMessagesCommand request, CancellationToken cancellationToken)
    {
        Customer customer = await _authenticationService.AuthenticateAsync(request.Username, request.Password);

        if (customer is null)
            throw new UnauthenticatedException("Invalid username or password.");

        TelegramMessagePackage<BatchMessage> batchMessages = new()
        {
            CustomerId = customer.CustomerId,
            BotKey = request.BotKey,
            IsSystemApproved = true,
            MessageType = "AF",
            CampaignId = $"{customer.CustomerId}_{DateTime.Now:yyyyMMddHHmmss}",
            CampDescription = request.CampDescription!,
            Priority = 2,
        };

        // 1) Get all phone numbers once
        IEnumerable<string> phones = request.Items.Select(x => x.PhoneNumber);

        // 2) One DB call
        IDictionary<string, string?> phoneToChat = await _userRepository.GetChatIdsAsync(phones, request.BotKey);

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

        if (batchMessages.Items is null || !batchMessages.Items.Any())
        {
            throw new EmptyMessagesPackageException($"{nameof(batchMessages.Items)} The messages collection cannot be null or empty.");
        }

        await _messageRepository.SendBatchMessagesAsync(batchMessages);

        return new SendBatchMessageCommandResult(batchMessages.CampaignId);
    }
}
