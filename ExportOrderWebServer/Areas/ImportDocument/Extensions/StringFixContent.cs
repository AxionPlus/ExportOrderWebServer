namespace ExportOrderWebServer.Areas.ImportDocument.Extensions;

public static class StringFixContent
{

    public static string FixCsvContent(this string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        // Убираем лишние пробелы и символы
        content = content.Trim('\uFEFF', '\u200B'); // Убираем BOM и zero-width space

        // Исправляем HTML entities
        var sb = new StringBuilder(content);

        // Заменяем некорректные последовательности
        sb.Replace("&#8220;", "\"")
            .Replace("&#65306;", ":")
            .Replace("&#8221;", "\"")
            .Replace("&quot;", "\"")
            .Replace("&#34;", "\"");

        // Исправляем неправильные переносы строк внутри полей
        content = sb.ToString().Replace("  ", " ");

        // Объединяем строки, если кавычки не закрыты
        content = FixMultilineFields(content);

        return content;
    }
    public static string FixMultilineFields(this string content)
    {
        var lines = new List<string>();
        var currentLine = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in content)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }

            if (c == '\n' && !inQuotes)
            {
                lines.Add(currentLine.ToString());
                currentLine.Clear();
            }
            else
            {
                currentLine.Append(c);
            }
        }

        if (currentLine.Length > 0)
        {
            lines.Add(currentLine.ToString());
        }

        return string.Join(Environment.NewLine, lines);
    }
}

