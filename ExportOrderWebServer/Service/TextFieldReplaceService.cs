using System.Text.RegularExpressions;

namespace ExportOrderWebServer.Service;

public class TextFieldReplaceService
{
    public string ReplaceExtraSymbols(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        /// замена символа переноса строки одиночным пробелом
        string result = text.Replace("\n", " ");

        /// замена множества последовательных символов пробела одиночным пробелом
        result = Regex.Replace(result, @"\s+", " ").Trim();

        return result;
    }
}
