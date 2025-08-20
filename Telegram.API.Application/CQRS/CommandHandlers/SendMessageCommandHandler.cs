using Mapster;
using MediatR;
using Microsoft.Data.SqlClient;
using Telegram.API.Application.CQRS.Commands;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Application.CQRS.CommandHandlers;

public class SendMessageCommandHandler(
    IUserRepository userRepository,
    IMessageRepository messageRepository,
    IAuthenticationService authenticationService)
    : IRequestHandler<SendMessageCommand, SendMessageCommandResult>
{
    private readonly IUserRepository _userRepository = userRepository;
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
            // Get Chat Id depending on the phone number
            User? user = await _userRepository.GetUserAsync(request.PhoneNumber, request.BotKey);

            if (string.IsNullOrEmpty(user!.ChatId))
            {
                // If chatId is null or empty, throw an exception or the BotKey is wrong
                throw new ChatIdNotFoundException($"Chat ID not found for phone number {request.PhoneNumber} and bot key {request.BotKey}. Or the BotKey is Wrong");
            }

            // Create the TelegramMessage object and Map it
            // Adapt the tuple to TelegramMessage using Mapster
            TelegramMessage message = ((customer, user), request).Adapt<TelegramMessage>();

            // Call the repository to send the message
            int referenceNumber = await _messageRepository.SendMessageAsync(message);

            return new SendMessageCommandResult(referenceNumber.ToString());
        }
        catch (Exception ex) when (ex is SqlException sqlEx)
        {
            // Translate to domain-specific exception
            throw new DatabaseException($"DB Error: {sqlEx.Message}");
        }
    }
}
