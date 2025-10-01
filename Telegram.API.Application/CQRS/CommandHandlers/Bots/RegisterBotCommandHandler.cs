using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Telegram.API.Application.CQRS.Commands.Bots;
using Telegram.API.Domain.Entities.Bot;
using Telegram.API.Domain.Entities.User;
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

        string? domain = _options.CurrentValue.DomainName;
        if (string.IsNullOrEmpty(domain))
            throw new InvalidConfigurationException("Webhook base DomainName is missing.");

        string baseUrl = domain.TrimEnd('/'); // e.g. https://.../telegram

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? baseUri) || !baseUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            throw new InvalidConfigurationException("Webhook base DomainName must be an absolute HTTPS URL.");

        // Ensure trailing slash on base URL
        baseUri = new Uri(baseUrl + "/");

        // Use relative path without leading slash
        Uri url = new(baseUri, $"bot/webhook/{publicId}");

        // Encrypt BotKey
        string encryptedBotKey = _authenticationService.Encrypt(request.BotKey, cancellationToken);
        Bot? bot = new()
        {
            Name = request.BotName,
            CustomerId = customer.CustomerId,
            EncryptedBotKey = encryptedBotKey,
            IsActive = false,
            WebhookUrl = url.ToString(),
            WebhookSecret = secrete,
            PublicId = publicId
        };

        int botId = 0;
        try
        {
            bot = await _botRepository.CreateAsync(bot, cancellationToken);
            if (bot is null)
                throw new ConflictException("Failed to persist bot.");

            botId = bot.Id; // Extract the ID only after confirming bot is not null

            // Add webhook to bot through api
            bool success = await _telegramClient.SetWebhookAsync(
                botToken: request.BotKey,
                url: url,
                secretToken: secrete,
                allowedUpdates: ["message"],
                dropPendingUpdates: true,
                cancellationToken);

            if (!success)
                throw new TelegramApiException("Telegram setWebhook returned ok=false");

            await _botRepository.UpdateBotActivityAsync(botId, true, cancellationToken);

            return new RegisterBotCommandResult(BotId: bot.Id);
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
            if (botId != 0) // Only try to update if we actually have a valid bot ID
                await _botRepository.UpdateBotActivityAsync(botId, false, cancellationToken);
            throw;
        }
    }
}
