namespace MundoVs.Core.Services;

public static class RrhhMarcacionSegmentActionHelper
{
    private const string TokenPrefix = "[segment-action:";

    public static string? GetAction(string? payloadRaw)
    {
        if (string.IsNullOrWhiteSpace(payloadRaw))
        {
            return null;
        }

        var text = payloadRaw.Trim();
        var start = text.LastIndexOf(TokenPrefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return null;
        }

        start += TokenPrefix.Length;
        var end = text.IndexOf(']', start);
        if (end <= start)
        {
            return null;
        }

        var action = text[start..end].Trim();
        return string.IsNullOrWhiteSpace(action) ? null : action.ToLowerInvariant();
    }

    public static string? SetAction(string? payloadRaw, string? action)
    {
        var cleaned = RemoveAction(payloadRaw);
        if (string.IsNullOrWhiteSpace(action))
        {
            return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
        }

        var token = $"{TokenPrefix}{action.Trim().ToLowerInvariant()}]";
        return string.IsNullOrWhiteSpace(cleaned)
            ? token
            : $"{cleaned} {token}";
    }

    public static string? RemoveAction(string? payloadRaw)
    {
        if (string.IsNullOrWhiteSpace(payloadRaw))
        {
            return null;
        }

        var text = payloadRaw.Trim();
        var start = text.LastIndexOf(TokenPrefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return text;
        }

        var end = text.IndexOf(']', start);
        if (end < 0)
        {
            return text[..start].Trim();
        }

        var before = text[..start].TrimEnd();
        var after = text[(end + 1)..].TrimStart();
        var combined = string.IsNullOrWhiteSpace(before)
            ? after
            : string.IsNullOrWhiteSpace(after)
                ? before
                : $"{before} {after}";

        return string.IsNullOrWhiteSpace(combined) ? null : combined;
    }

    public static string? ResolveAction(string? startPayloadRaw, string? endPayloadRaw)
        => GetAction(startPayloadRaw) ?? GetAction(endPayloadRaw);
}