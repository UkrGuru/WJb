namespace WJb.Helpers;

/// <summary>
/// 5-field cron match (free version).
/// </summary>
public static class CronHelper
{
    public static bool CronValidate(string? expression, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        var parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
            return false;

        return
            Match(parts[0], now.Minute, 0, 59) &&
            Match(parts[1], now.Hour, 0, 23) &&
            Match(parts[2], now.Day, 1, 31) &&
            Match(parts[3], now.Month, 1, 12) &&
            Match(parts[4], (int)now.DayOfWeek, 0, 6);
    }

    private static bool Match(string field, int value, int min, int max)
    {
        foreach (var token in field.Split(','))
        {
            if (token == "*")
                return true;

            if (token.Contains('-'))
            {
                var parts = token.Split('-');
                if (parts.Length != 2)
                    return false;

                if (!int.TryParse(parts[0], out var start) ||
                    !int.TryParse(parts[1], out var end))
                    return false;

                if (start < min || end > max)
                    return false;

                if (value >= start && value <= end)
                    return true;
            }
            else
            {
                if (!int.TryParse(token, out var single))
                    return false;

                if (single == value)
                    return true;
            }
        }

        return false;
    }
}