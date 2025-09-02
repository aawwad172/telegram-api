using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Telegram.API.Application.CQRS.Commands.Bots;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Clients;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;
using Telegram.API.Domain.Settings;

namespace Telegram.API.Application.CQRS.CommandHandlers.Bots;

public class RegisterBotCommandHandler(
    IBotRepository botRepository,
    IAuthenticationService authenticationService,
    ITelegramClient telegramClient,
    IOptionsMonitor<AppSettings> options)
    : IRequestHandler<RegisterBotCommand, RegisterBotCommandResult>
{
    private readonly IBotRepository _botRepository = botRepository;
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly ITelegramClient _telegramClient = telegramClient;
    private readonly IOptionsMonitor<AppSettings> _options = options;
    public async Task<RegisterBotCommandResult> Handle(RegisterBotCommand request, CancellationToken cancellationToken)
    {
        Customer customer = await _authenticationService.AuthenticateAsync(request.Username, request.Password, cancellationToken);

        string secrete = Guid.NewGuid().ToString("N");
        string publicId = Guid.NewGuid().ToString("N");

        string baseUrl = _options.CurrentValue.DomainName.TrimEnd('/'); // e.g. https://.../telegram
        if (string.IsNullOrWhiteSpace(baseUrl) || !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            throw new ConfigurationErrorsException("Webhook base DomainName must be an HTTPS URL.");

        Uri url = new($"{baseUrl}/webhook/{publicId}");

        // Encrypt BotKey
        string encryptedBotKey = _authenticationService.Encrypt(request.BotKey);
        Bot? bot = new()
        {
            CustomerId = customer.CustomerId,
            EncryptedBotKey = encryptedBotKey,
            IsActive = false,
            WebhookUrl = url.ToString(),
            WebhookSecret = secrete,
            PublicId = publicId
        };

        try
        {
            bot = await _botRepository.CreateAsync(bot, cancellationToken);

            // Add webhook to bot through api
            bool success = await _telegramClient.SetWebhookAsync(
                botToken: request.BotKey,
                url: url,
                secretToken: secrete,
                allowedUpdates: ["message", "callback_query"],
                dropPendingUpdates: true,
                cancellationToken);

            if (!success)
                throw new TelegramApiException("Telegram setWebhook returned ok=false");

            await _botRepository.UpdateBotActivityAsync(bot!.BotId, true, cancellationToken);

            return new RegisterBotCommandResult(BotId: bot.BotId);
        }
        catch (HttpRequestException ex)
        {
            throw new TelegramApiException($"Telegram API error: {ex.Message}");
        }
        catch (SqlException sqlEx)
        {
            // DB issue after initial insert is rare; row remains for ops
            throw new ConflictException($"DB Error {sqlEx.Message}");
        }
        catch (TelegramApiException)
        {
            await _botRepository.UpdateBotActivityAsync(bot!.BotId, false, cancellationToken);
            throw;
        }
    }
}
