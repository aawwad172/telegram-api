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
        try
        {
            Customer customer = await _authenticationService.AuthenticateAsync(request.Username, request.Password);

            if (customer is null)
            {
                throw new UnauthenticatedException("Invalid username or password.");
            }

            TelegramMessagePackage<BatchMessage> batchMessages = new()
            {
                CustomerId = customer.CustomerId.ToString(),
                BotKey = request.BotKey,
                IsSystemApproved = true,
                MessageType = "AF",
                CampaignId = $"{customer.CustomerId}_{DateTime.Now:yyyyMMddHHmmss}",
                CampDescription = request.CampDescription!,
                Priority = 6,
            };

            foreach (BatchMessageItem item in request.Items)
            {
                User? user = await _userRepository.GetUserAsync(item.PhoneNumber, request.BotKey);

                BatchMessage message = new()
                {
                    ChatId = user is not null ? user.ChatId : null!,
                    MessageText = item.MessageText,
                };

                batchMessages.Items.Add(message);
            }

            if (batchMessages.Items is null || !batchMessages.Items.Any())
            {
                throw new EmptyMessagesPackageException(nameof(batchMessages.Items) + "The messages collection cannot be null or empty.");
            }

            await _messageRepository.SendBatchMessagesAsync(batchMessages);

            return new SendBatchMessageCommandResult(batchMessages.CampaignId);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
