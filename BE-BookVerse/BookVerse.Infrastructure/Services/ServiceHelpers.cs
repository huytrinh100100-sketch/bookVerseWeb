using System.Text.RegularExpressions;

namespace BookVerse.Infrastructure.Services;

/// <summary>Tiện ích dùng chung cho các service nghiệp vụ.</summary>
public static partial class ServiceHelpers
{
    [GeneratedRegex("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"^[0-9]{9,11}$")]
    private static partial Regex PhoneRegex();

    public static bool IsValidEmail(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && EmailRegex().IsMatch(value.Trim());
    }

    public static bool IsValidPhone(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && PhoneRegex().IsMatch(value.Trim());
    }

    /// <summary>Chuyển chuỗi có dấu/khoảng trắng thành slug dùng cho URL.</summary>
    public static string Slugify(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }

        var slug = sb.ToString().Normalize(System.Text.NormalizationForm.FormC).ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");
        return slug.Trim('-');
    }

    /// <summary>Sinh mã đơn hàng dạng: OD-20261105-0001.</summary>
    public static string BuildOrderCode(DateTime utcNow, int sequence)
    {
        return $"OD-{utcNow:yyyyMMdd}-{sequence:D4}";
    }

    /// <summary>Sinh mã sách dạng: BK-000001.</summary>
    public static string BuildBookCode(int sequence)
    {
        return $"BK-{sequence:D6}";
    }

    public static string BuildBarcode(Guid bookId, int sequence)
    {
        return $"BC-{bookId.ToString("N")[..6].ToUpperInvariant()}-{sequence:D4}";
    }

    /// <summary>Chuyển trọng lượng dạng chuỗi ("0.3 kg") sang số.</summary>
    public static decimal? ParseWeight(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var raw = value.Trim();
        var separator = raw.IndexOfAny(new[] { ',', '.' });
        if (separator < 0)
        {
            return decimal.TryParse(raw, out var plain) ? plain : null;
        }

        // Chuỗi dạng "0,3 kg" -> "0.3"
        var normalized = raw.Replace(',', '.');
        var end = normalized.IndexOfAny(new[] { ' ', 'k', 'K', 'g', 'G' });
        if (end > separator)
        {
            normalized = normalized[..end];
        }

        return decimal.TryParse(normalized, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }

    /// <summary>Tách chuỗi CSV thành danh sách (bỏ khoảng trắng và rỗng).</summary>
    public static IReadOnlyCollection<string> SplitCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    /// <summary>Gộp danh sách thành chuỗi CSV để lưu vào cột text.</summary>
    public static string JoinCsv(IEnumerable<string>? values)
    {
        return values is null ? string.Empty : string.Join(",", values);
    }

    /// <summary>Chuẩn hóa số tiền làm tròn 2 chữ số thập phân.</summary>
    public static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
