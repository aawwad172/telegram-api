using MediatR;
using Telegram.API.Application.CQRS.Queries.Bots;
using Telegram.API.Domain.Entities.Bot;
using Telegram.API.Domain.Entities.Message;
using Telegram.API.Domain.Entities.User;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Clients;

namespace Telegram.API.Application.CQRS.QueryHandlers.Bots;

public class GetWebhookInfoQueryHandler(
    ITelegramClient telegramClient,
    IAuthenticationService authenticationService)
    : IRequestHandler<GetWebhookInfoQuery, GetWebhookInfoQueryResult>
{
    private readonly ITelegramClient _telegramClient = telegramClient;
    private readonly IAuthenticationService _authenticationService = authenticationService;
    public async Task<GetWebhookInfoQueryResult> Handle(GetWebhookInfoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Authenticate the Customer using the provided username and password
            Customer? customer = await _authenticationService.AuthenticateAsync(request.Username, request.Password, cancellationToken);

            if (customer is null)
                throw new UnauthorizedException("Invalid username or password.");

            Bot? bot = await _authenticationService.ValidateBotIdAsync(request.BotId, customer.CustomerId, cancellationToken);
            if (bot is null)
                throw new UnauthorizedException("Invalid Bot Key.");

            string decryptedBotToken = _authenticationService.Decrypt(bot.EncryptedBotKey);

            TelegramResponse<WebhookInfo?> response = await _telegramClient.GetWebhookInfoAsync(decryptedBotToken, cancellationToken);

            if (!response.Ok || response.Result is null)
                throw new TelegramApiException($"Telegram API Error: {response.ErrorCode}, {response.Description}");

            return new GetWebhookInfoQueryResult(response);
        }
        catch (HttpRequestException ex)
        {
            throw new TelegramApiException($"Telegram API Error: {ex.Message}");
        }
    }
}
