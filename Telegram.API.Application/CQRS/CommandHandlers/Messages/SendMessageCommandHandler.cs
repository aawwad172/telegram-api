using Mapster;
using MediatR;
using Microsoft.Data.SqlClient;
using Telegram.API.Application.CQRS.Commands.Message;
using Telegram.API.Domain.Entities.Bot;
using Telegram.API.Domain.Entities.Message;
using Telegram.API.Domain.Entities.User;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Application.CQRS.CommandHandlers;

public class SendMessageCommandHandler(
    IRecipientRepository recipientRepository,
    IMessageRepository messageRepository,
    IAuthenticationService authenticationService)
    : IRequestHandler<SendMessageCommand, SendMessageCommandResult>
{
    private readonly IRecipientRepository _recipientRepository = recipientRepository;
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly IMessageRepository _messageRepository = messageRepository;
    public async Task<SendMessageCommandResult> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate username, and password and return the customer ID
            Customer customer = await _authenticationService.AuthenticateAsync(request.Username, request.Password);

            // Should authenticate the bot key too through the bot repository
            Bot bot = await _authenticationService.ValidateBotIdAsync(request.BotId, customer.CustomerId);

            // Get Chat Id depending on the phone number
            Recipient? recipient = await _recipientRepository.GetRecipientAsync(request.PhoneNumber, bot.BotId);
            if (recipient is null || string.IsNullOrWhiteSpace(recipient.ChatId))
                // If chatId is null or empty, throw an exception or the BotKey is wrong
                throw new ChatIdNotFoundException($"Chat ID not found for phone number {request.PhoneNumber}.");

            // Create the TelegramMessage object and Map it
            // Adapt the tuple to TelegramMessage using Mapster
            TelegramMessage message = (((customer, recipient), bot), request).Adapt<TelegramMessage>();

            // Call the repository to send the message
            int referenceNumber = await _messageRepository.SendMessageAsync(message);

            return new SendMessageCommandResult(referenceNumber.ToString());
        }
        catch (SqlException sqlEx)
        {
            // Translate to domain-specific exception
            throw new DatabaseException("A database error occurred.", sqlEx);
        }
    }
}
