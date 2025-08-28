using Mapster;
using MediatR;
using Telegram.API.Application.CQRS.Commands;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Application.CQRS.CommandHandlers;

public class SendCampaignMessageCommandHandler(
    IMessageRepository messageRepository,
    IAuthenticationService authenticationService,
    IUserRepository userRepository)
    : IRequestHandler<SendCampaignMessageCommand, SendCampaignMessageCommandResult>
{
    private readonly IMessageRepository _messageRepository = messageRepository;
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly IUserRepository _userRepository = userRepository;
    public async Task<SendCampaignMessageCommandResult> Handle(SendCampaignMessageCommand request, CancellationToken cancellationToken)
    {
        Customer customer = await _authenticationService.AuthenticateAsync(request.Username, request.Password);

        if (customer is null)
            throw new UnauthenticatedException("Invalid username or password.");

        Bot? bot = await _authenticationService.ValidateBotKeyAsync(request.BotKey, customer.CustomerId);
        if (bot is null)
            throw new InvalidBotKeyException($"Invalid Bot Key {request.BotKey} for customer {customer.CustomerId}");

        TelegramMessagePackage<CampaignMessage> campaignMessage = ((customer, bot), request).Adapt<TelegramMessagePackage<CampaignMessage>>();

        IEnumerable<string> phones = request.Items.Select(x => x.PhoneNumber);

        IDictionary<string, string?> phoneToChat = await _userRepository.GetChatIdsAsync(phones, bot.BotId);

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
