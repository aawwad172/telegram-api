using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Infrastructure.Persistence.Repositories;

public class JsonFileRepository : IJsonFileRepository
{
    // Prevent interleaved writes to the same file from this process
    private static readonly SemaphoreSlim _gate = new(1, 1);
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false, // compact lines; still valid JSON
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public async Task SaveToFileAsync<T>(
        IEnumerable<T> items,
        string fullPath,
        CancellationToken ct = default)
    {

        if (string.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentException("Full path is required.", nameof(fullPath));

        string? folder = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrWhiteSpace(folder))
            throw new ArgumentException("Full path must include a directory.", nameof(fullPath));

        Directory.CreateDirectory(folder); // idempotent

        // Use CreateNew to avoid accidental overwrite; switch to Create to overwrite.
        await using FileStream fs = new(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(fs, items, _jsonOptions, ct);
        await fs.FlushAsync(ct);
    }
}
