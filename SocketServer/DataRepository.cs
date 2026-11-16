public static class DataRepository
{
    private static readonly Dictionary<string, Dictionary<string, int>> Data =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["SetA"] = new(StringComparer.OrdinalIgnoreCase) { ["One"] = 1, ["Two"] = 2 },
            ["SetB"] = new(StringComparer.OrdinalIgnoreCase) { ["Three"] = 3, ["Four"] = 4 },
            ["SetC"] = new(StringComparer.OrdinalIgnoreCase) { ["Five"] = 5, ["Six"] = 6 },
            ["SetD"] = new(StringComparer.OrdinalIgnoreCase) { ["Seven"] = 7, ["Eight"] = 8 },
            ["SetE"] = new(StringComparer.OrdinalIgnoreCase) { ["Nine"] = 9, ["Ten"] = 10 }
        };

    // Expected input format: SetA-Two. The first hyphen separates the two keys.
    public static int? GetResponseCount(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        int separator = input.IndexOf('-');
        if (separator <= 0 || separator == input.Length - 1) return null;

        string setKey = input[..separator].Trim();
        string itemKey = input[(separator + 1)..].Trim();

        return Data.TryGetValue(setKey, out var subset) &&
               subset.TryGetValue(itemKey, out int value) ? value : null;
    }
}
