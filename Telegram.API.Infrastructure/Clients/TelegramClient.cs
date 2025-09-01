using System.Text.Json;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Entities.Telegram;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Infrastructure.Clients;

namespace Telegram.API.Infrastructure.Clients;

public class TelegramClient(HttpClient telegramClient) : ITelegramClient
{
    private readonly HttpClient _telegramClient = telegramClient;

    public Task<bool> DeleteWebhookAsync(string botToken, bool dropPendingUpdates, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<TelegramResponse<WebhookInfo?>> GetWebhookInfoAsync(string botToken, CancellationToken ct = default)
    {
        string url = $"/{botToken}/getWebhookInfo";

        try
        {
            HttpResponseMessage response = await _telegramClient.GetAsync(url);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            // 2) Deserialize with System.Text.Json
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            TelegramResponse<WebhookInfo?> telegramResponse = JsonSerializer
                .Deserialize<TelegramResponse<WebhookInfo?>>(jsonResponse, options)
                ?? throw new InvalidOperationException("Empty response when getting webhook information");

            return telegramResponse!;
        }
        catch (HttpRequestException ex)
        {
            throw new TelegramApiException($"HTTP Request Error while Sending Message to Telegram: {ex.Message}");
        }
        catch (JsonException ex)
        {
            throw new TelegramApiException($"JSON Parsing Error while parsing Message: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            throw new TelegramApiException($"Invalid Operation Error: {ex.Message}");
        }
    }

    public Task<bool> SetWebhookAsync(
        string botToken,
        Uri url,
        string secretToken,
        IReadOnlyCollection<string> allowedUpdates,
        bool dropPendingUpdates,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateWebhookAsync(
        string botToken,
        Uri url,
        string newSecretToken,
        IReadOnlyCollection<string> allowedUpdates,
        bool dropPendingUpdates,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
