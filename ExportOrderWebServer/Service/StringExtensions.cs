namespace ExportOrderWebServer.Service;

public static class StringExtensions
{
    public static string ReplaceT(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        value = value.Replace("\n", "");
        value = value.Trim();

        return value;
    }
    
    public static string ReplaceSymbols(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        value = value.Replace("\n", "");
        value = value.Replace("$", "");
        value = value.Replace("&", "AND");
        value = value.Replace("«", "\"");
        value = value.Replace("»", "\"");
        value = value.Replace(";", ",");
        value = value.Trim();

        return value;
    }

    public static string DeleteExtraSymbols(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        value = value.Replace("\n", "");
        value = value.Replace("$", "");
        value = value.Replace("&", "");
        value = value.Replace(";", "");
        value = value.Replace(",", "");
        value = value.Trim();

        return value;
    }
}
