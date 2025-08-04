using MediatR;
using Microsoft.Data.SqlClient;
using Telegram.API.Application.CQRS.Commands;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Application.CQRS.CommandHandlers;

public class SendMessageCommandHandler(
    IMessageRepository messageRepository,
    IAuthenticationService authenticationService)
    : IRequestHandler<SendMessageCommand, SendMessageCommandResult>
{
    private readonly IMessageRepository _messageRepository = messageRepository;
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<SendMessageCommandResult> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate username, and password and return the customer ID
            User user = await _authenticationService.AuthenticateAsync(request.Username, request.Password);
            // Get Chat Id depending on the phone number
            string? chatId = await _messageRepository.GetChatId(request.PhoneNumber, request.BotKey);

            if (string.IsNullOrEmpty(chatId))
            {
                // If chatId is null or empty, throw an exception or the BotKey is wrong
                throw new ChatIdNotFoundException($"Chat ID not found for phone number {request.PhoneNumber} and bot key {request.BotKey}. Or the BotKey is Wrong");
            }

            // Create the TelegramMessage object
            TelegramMessage message = new()
            {
                CustomerId = user.CustomerId.ToString(),
                ChatId = chatId,
                BotKey = request.BotKey,
                MessageText = request.MessageText,
                PhoneNumber = request.PhoneNumber,
                MessageType = 'A', // Always 'A' for API messages
                Priority = 6,
                IsSystemApproved = !user.RequirSystemApprove
            };

            // Call the repository to send the message
            int referenceNumber = await _messageRepository.SendMessage(message);

            return new SendMessageCommandResult(referenceNumber.ToString());
        }
        catch (Exception ex) when (ex is SqlException sqlEx)
        {
            // Translate to domain-specific exception
            throw new DatabaseException($"DB Error: {sqlEx.Message}");
        }
    }
}
