namespace BookingAdmin.Web.Services;

public static class ChannelParser
{
    public static (string Channel, string? Customer, bool IsPackage) Parse(string raw)
    {
        var s = (raw ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(s)) return ("Empty", null, false);

        var isPackage = false;
        if (s.StartsWith("3D2N", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("2D1N", StringComparison.OrdinalIgnoreCase))
        {
            isPackage = true;
            var idx = s.IndexOf(' ');
            s = idx > 0 ? s[(idx + 1)..].Trim() : string.Empty;
        }

        if (string.IsNullOrEmpty(s)) return ("Package", null, isPackage);

        // Channel prefix detection. Order matters.
        var prefixes = new (string Prefix, string Channel)[]
        {
            ("BO ", "Booking.com"),
            ("BO\t", "Booking.com"),
            ("TA ", "TA"),
            ("Ta ", "TA"),
            ("TA\t", "TA"),
        };

        foreach (var (prefix, channel) in prefixes)
        {
            if (s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var customer = s[prefix.Length..].Trim();
                return (channel, customer, isPackage);
            }
        }

        // Bare channel keywords
        if (s.StartsWith("Expedia", StringComparison.OrdinalIgnoreCase)) return ("Expedia", null, isPackage);
        if (s.StartsWith("Online", StringComparison.OrdinalIgnoreCase)) return ("Online", null, isPackage);
        if (s.StartsWith("Agoda", StringComparison.OrdinalIgnoreCase)) return ("Agoda", null, isPackage);
        if (s.StartsWith("Block", StringComparison.OrdinalIgnoreCase)) return ("Block", null, isPackage);
        if (s.StartsWith("BL ", StringComparison.OrdinalIgnoreCase)) return ("Block", s[3..].Trim(), isPackage);
        if (s.StartsWith("BO", StringComparison.OrdinalIgnoreCase) && s.Length > 2 && char.IsDigit(s[2]))
            return ("Booking.com", s[2..].Trim(), isPackage);

        return ("Other", s, isPackage);
    }
}
