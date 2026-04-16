namespace WJb.Helpers;

/// <summary>
/// 5-field cron matcher (free version).
/// </summary>
public static class CronHelper
{
    /// <summary>
    /// Validates whether a cron expression matches the specified time.
    /// </summary>
    public static bool CronValidate(string? expression, DateTime now)
    {
        // Empty or missing cron expression is never due
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        // Expect exactly 5 cron fields:
        // minute hour day month day-of-week
        var parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
            return false;

        // Match all cron fields against current DateTime components
        return
            Match(parts[0], now.Minute, 0, 59) &&
            Match(parts[1], now.Hour, 0, 23) &&
            Match(parts[2], now.Day, 1, 31) &&
            Match(parts[3], now.Month, 1, 12) &&
            Match(parts[4], (int)now.DayOfWeek, 0, 6);
    }

    private static bool Match(string field, int value, int min, int max)
    {
        // A field may contain multiple comma-separated tokens
        foreach (var token in field.Split(','))
        {
            // Wildcard matches any value
            if (token == "*")
                return true;

            if (token.Contains('-'))
            {
                // Range token: e.g. 10-20
                var parts = token.Split('-');
                if (parts.Length != 2)
                    return false;

                // Parse range bounds
                if (!int.TryParse(parts[0], out var start) || !int.TryParse(parts[1], out var end))
                    return false;

                // Validate range boundaries
                if (start < min || end > max)
                    return false;

                // Match if value falls within the range (inclusive)
                if (value >= start && value <= end)
                    return true;
            }
            else
            {
                // Single numeric value
                if (!int.TryParse(token, out var single))
                    return false;

                if (single == value)
                    return true;
            }
        }

        // No token in this field matched the value
        return false;
    }
}
