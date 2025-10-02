using System.Globalization;
using System.Text.RegularExpressions;

namespace Telegram.API.Application.Utilities;

public class FileNameHelper
{
    public static string MakeSafeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "campaign";
        string invalid = new(Path.GetInvalidFileNameChars());
        string pattern = $"[{Regex.Escape(invalid)}]+";
        string cleaned = Regex.Replace(name, pattern, "_");
        cleaned = Regex.Replace(cleaned, @"\s+", "_").Trim('_');
        return string.IsNullOrWhiteSpace(cleaned) ? "campaign" : cleaned;
    }

    /// <summary>
    /// If campaignId already ends with _yyyyMMddHHmmss, keep it.
    /// Otherwise append a new timestamp. Always returns "*.json".
    /// </summary>
    public static string ComposeCampaignFileName(string campaignId, DateTime? now = null)
    {
        string baseName = Path.GetFileNameWithoutExtension(campaignId ?? "");
        baseName = MakeSafeFileName(baseName);

        // matches "..._20250817170250" at the END
        bool hasTrailingStamp = Regex.IsMatch(
            baseName,
            @"(_(\d{14}(_[0-9a-fA-F]{32})?|[0-9a-fA-F]{32}))$"
        );

        if (!hasTrailingStamp)
        {
            string stamp = (now ?? DateTime.Now).ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            baseName = $"{baseName}_{stamp}_{Guid.NewGuid():N}";
        }

        return baseName + ".json";
    }
}
