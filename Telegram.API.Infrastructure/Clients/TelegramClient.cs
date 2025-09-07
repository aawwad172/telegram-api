using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Entities.Telegram;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Infrastructure.Clients;
using Telegram.API.Domain.Settings;

namespace Telegram.API.Infrastructure.Clients;

public class TelegramClient(
    HttpClient httpClient,
    IOptionsMonitor<TelegramOptions> options)
    : ITelegramClient
{
    private readonly HttpClient _httpClient = httpClient;
    private static readonly Regex SecretAllowed = new("^[A-Za-z0-9_-]{1,256}$", RegexOptions.Compiled);
    private readonly IOptionsMonitor<TelegramOptions> _options = options;

    public Task<bool> DeleteWebhookAsync(string botToken, bool dropPendingUpdates, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<TelegramResponse<WebhookInfo?>> GetWebhookInfoAsync(string botToken, CancellationToken ct = default)
    {
        string url = $"/bot{botToken}/getWebhookInfo";

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url, ct);

            string jsonResponse = await response.Content.ReadAsStringAsync(ct);

            // 2) Deserialize with System.Text.Json
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
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

    public async Task<bool> SetWebhookAsync(
        string botToken,
        Uri url,
        string secretToken,
        IReadOnlyCollection<string> allowedUpdates,
        bool dropPendingUpdates,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(botToken))
            throw new ArgumentException("botToken is required.", nameof(botToken));

        if (url is null || !url.IsAbsoluteUri || !url.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("url must be an absolute HTTPS URI.", nameof(url));

        if (string.IsNullOrWhiteSpace(secretToken) || !SecretAllowed.IsMatch(secretToken))
            throw new ArgumentException("secretToken must match [A-Za-z0-9_-] and be 1..256 chars.", nameof(secretToken));

        string path = $"/bot{botToken}/setWebhook";

        // Telegram expects:
        // - url (string)
        // - secret_token (string)           // echoed back in X-Telegram-Bot-Api-Secret-Token
        // - allowed_updates (JSON array)    // optional
        // - drop_pending_updates ("true"/"false") // optional
        List<KeyValuePair<string, string>> form = new()
        {
            new("url", url.ToString()),
            new("secret_token", secretToken),
            new("drop_pending_updates", dropPendingUpdates ? "true" : "false"),
        };

        if (allowedUpdates is not null && allowedUpdates.Count > 0)
        {
            // must be a JSON array of strings
            form.Add(new("allowed_updates", JsonSerializer.Serialize(allowedUpdates)));
        }

        using HttpRequestMessage request = new(HttpMethod.Post, path)
        {
            Content = new FormUrlEncodedContent(form)
        };

        using HttpResponseMessage response = await _httpClient.SendAsync(request, ct);
        string body = await response.Content.ReadAsStringAsync(ct);

        // Telegram returns 200 even on logical errors; check { ok, description }
        try
        {
            using JsonDocument doc = JsonDocument.Parse(body);
            JsonElement root = doc.RootElement;

            bool ok = root.TryGetProperty("ok", out JsonElement okEl) && okEl.GetBoolean();
            if (ok) return true;

            // optional: surface description via logs if you have them
            string? desc = root.TryGetProperty("description", out JsonElement d) ? d.GetString() : "Unknown Telegram error";
            // e.g., LoggerService.Warning("setWebhook failed: {Desc}", desc);
            return false;
        }
        catch (JsonException)
        {
            // Unexpected response shape; treat non-success HTTP as failure
            if (!response.IsSuccessStatusCode) return false;

            // If HTTP 200 but not JSON, also treat as failure
            return false;
        }
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
