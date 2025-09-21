using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Telegram.API.Domain.Entities.Bot;
using Telegram.API.Domain.Entities.Message;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Infrastructure.Clients;

namespace Telegram.API.Infrastructure.Clients;

public class TelegramClient : ITelegramClient
{
    private readonly HttpClient _http;
    private static readonly Regex SecretAllowed = new("^[A-Za-z0-9_-]{1,256}$", RegexOptions.Compiled);
    private readonly JsonSerializerOptions _json; // snake_case in/out

    public TelegramClient(HttpClient httpClient)
    {
        _http = httpClient;
        _json = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<TelegramResponse<WebhookInfo?>> GetWebhookInfoAsync(string botToken, CancellationToken ct = default)
           => await GetAsync<WebhookInfo?>($"/bot{botToken}/getWebhookInfo", ct);

    public async Task<bool> SetWebhookAsync(
           string botToken,
           Uri url,
           string secretToken,
           IReadOnlyCollection<string> allowedUpdates,
           bool dropPendingUpdates,
           CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(botToken)) throw new ArgumentException("Required.", nameof(botToken));
        if (url is null || !url.IsAbsoluteUri || !url.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Must be absolute HTTPS URI.", nameof(url));
        if (string.IsNullOrWhiteSpace(secretToken) || !SecretAllowed.IsMatch(secretToken))
            throw new ArgumentException("Secret token invalid format.", nameof(secretToken));

        // Telegram accepts JSON too; keep url-encoded if you like. Here: JSON for clarity.
        var payload = new
        {
            url = url.ToString(),
            secret_token = secretToken,
            allowed_updates = allowedUpdates?.Count > 0 ? allowedUpdates : null,
            drop_pending_updates = dropPendingUpdates
        };

        TelegramResponse<JsonElement> resp = await PostJsonAsync<JsonElement>($"/bot{botToken}/setWebhook", payload, ct);
        return resp.Ok;
    }

    public async Task<bool> UpdateWebhookAsync(
            string botToken,
            Uri url,
            string newSecretToken,
            IReadOnlyCollection<string> allowedUpdates,
            bool dropPendingUpdates,
            CancellationToken ct = default)
                => await SetWebhookAsync(botToken, url, newSecretToken, allowedUpdates, dropPendingUpdates, ct);

    public async Task<bool> DeleteWebhookAsync(string botToken, bool dropPendingUpdates, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(botToken)) throw new ArgumentException("Required.", nameof(botToken));

        var payload = new { drop_pending_updates = dropPendingUpdates };

        TelegramResponse<JsonElement> resp = await PostJsonAsync<JsonElement>($"/bot{botToken}/deleteWebhook", payload, ct);
        return resp.Ok;
    }

    public async Task<bool> SendTextAsync(
        string botToken,
        string chatId,
        string text,
        object? replyMarkup = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(botToken)) throw new ArgumentException("Required.", nameof(botToken));
        if (string.IsNullOrWhiteSpace(chatId)) throw new ArgumentException("Required.", nameof(chatId));
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Required.", nameof(text));

        var payload = new
        {
            chat_id = chatId,
            text,
            reply_markup = replyMarkup
        };

        TelegramResponse<JsonElement> resp =
            await PostJsonAsync<JsonElement>($"/bot{botToken}/sendMessage", payload, ct);
        return resp.Ok;
    }

    private async Task<TelegramResponse<T>> GetAsync<T>(string path, CancellationToken ct)
    {
        using HttpResponseMessage res = await _http.GetAsync(path, ct);
        string body = await res.Content.ReadAsStringAsync(ct);

        TelegramResponse<T>? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<TelegramResponse<T>>(body, _json);
        }
        catch (JsonException ex)
        {
            throw new JsonSerializationException($"Telegram JSON parse error: {ex.Message}");
        }

        if (parsed is null)
            throw new TelegramApiException("Empty response from Telegram.");

        if (!parsed.Ok)
            throw new TelegramApiException($"Telegram Error request: {parsed.ErrorCode} {parsed.Description}");

        return parsed;
    }

    private async Task<TelegramResponse<T>> PostJsonAsync<T>(string path, object payload, CancellationToken ct)
    {
        using StringContent content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
        using HttpResponseMessage res = await _http.PostAsync(path, content, ct);
        string body = await res.Content.ReadAsStringAsync(ct);

        TelegramResponse<T>? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<TelegramResponse<T>>(body, _json);
        }
        catch (JsonException ex)
        {
            throw new TelegramApiException($"Telegram JSON parse error: {ex.Message}");
        }

        if (parsed is null)
            throw new TelegramApiException("Empty response from Telegram.");

        if (!parsed.Ok)
            throw new TelegramApiException($"Telegram Error request: {parsed.ErrorCode} {parsed.Description}");

        return parsed;
    }
}
