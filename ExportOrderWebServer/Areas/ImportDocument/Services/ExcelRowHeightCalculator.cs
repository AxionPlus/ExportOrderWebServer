
namespace ExportOrderWebServer.Areas.ImportDocument.Services;
public class ExcelRowHeightCalculator
{
    // Константы для расчета высоты строк
    private const double PointsPerInch = 72.0; // 1 дюйм = 72 пункта
    private const double PixelsPerInch = 96.0; // Стандартное разрешение
    private const double MaxRowHeight = 409.0; // Максимальная высота строки в Excel
    private const double MinRowHeight = 0.0; // Минимальная высота строки в Excel

    // Основной метод для расчета высоты строки на основе текста
    public double CalculateRowHeightForCell(string text, double columnWidth, double fontSize = 11)
    {
        if (string.IsNullOrEmpty(text))
            return GetDefaultRowHeight(fontSize);

        // Расчет базовых параметров
        double charsPerLine = CalculateCharsPerLine(columnWidth, fontSize);
        int lineCount = CalculateLineCount(text, charsPerLine);

        // Высота строки
        double rowHeight = CalculateHeightFromLineCount(lineCount, fontSize);

        // Ограничиваем допустимыми пределами
        return Math.Max(MinRowHeight, Math.Min(MaxRowHeight, rowHeight));
    }

    // Метод для расчета с учетом переноса слов
    public double CalculateRowHeightWithWordWrap(string text, double columnWidth, double fontSize = 11,
                                                string fontName = "Calibri", bool isBold = false)
    {
        if (string.IsNullOrEmpty(text))
            return GetDefaultRowHeight(fontSize);

        // Получаем среднюю ширину символа с учетом шрифта
        double avgCharWidth = GetAverageCharWidth(fontSize, fontName, isBold);

        // Конвертируем ширину столбца в пиксели
        double columnWidthPixels = ColumnWidthToPixels(columnWidth, fontSize);

        // Расчет количества строк с учетом переноса слов
        int lineCount = CalculateLineCountWithWordWrap(text, columnWidthPixels, avgCharWidth);

        // Высота строки
        double rowHeight = CalculateHeightFromLineCount(lineCount, fontSize);

        // Добавляем отступы
        rowHeight += GetPaddingHeight(fontSize);

        return Math.Max(MinRowHeight, Math.Min(MaxRowHeight, rowHeight));
    }

    // Расчет количества символов в строке
    private double CalculateCharsPerLine(double columnWidth, double fontSize)
    {
        // В Excel ширина столбца измеряется в количестве символов стандартного шрифта
        // Ширина символа зависит от размера шрифта
        double charWidthFactor = fontSize / 11.0; // Относительно стандартного размера 11

        // Формула для расчета количества символов в строке
        // columnWidth - ширина в символах для шрифта 11pt
        // Учитываем, что не все символы одинаковой ширины
        return columnWidth * (11.0 / fontSize) * 0.9; // Коэффициент 0.9 для учета пробелов и отступов
    }

    // Расчет количества строк на основе длины текста
    private int CalculateLineCount(string text, double charsPerLine)
    {
        if (charsPerLine <= 0)
            return 1;

        // Учитываем явные переносы строк
        int explicitLineBreaks = CountLineBreaks(text);

        // Расчет строк для оставшегося текста
        string textWithoutBreaks = RemoveLineBreaks(text);
        int calculatedLines = (int)Math.Ceiling(textWithoutBreaks.Length / charsPerLine);

        // Суммируем
        return Math.Max(1, explicitLineBreaks + calculatedLines);
    }

    // Расчет количества строк с учетом переноса слов
    private int CalculateLineCountWithWordWrap(string text, double columnWidthPixels, double avgCharWidth)
    {
        if (columnWidthPixels <= 0 || avgCharWidth <= 0)
            return 1;

        // Разбиваем текст на слова
        string[] words = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        int lineCount = 0;
        double currentLineWidth = 0;

        foreach (var word in words)
        {
            double wordWidth = word.Length * avgCharWidth;
            double spaceWidth = avgCharWidth * 0.5; // Ширина пробела

            if (currentLineWidth + wordWidth > columnWidthPixels && currentLineWidth > 0)
            {
                // Переносим на новую строку
                lineCount++;
                currentLineWidth = wordWidth + spaceWidth;
            }
            else
            {
                // Добавляем к текущей строке
                currentLineWidth += wordWidth + spaceWidth;
            }
        }

        // Учитываем последнюю строку
        if (currentLineWidth > 0)
            lineCount++;

        // Учитываем явные переносы строк
        lineCount += CountLineBreaks(text);

        return Math.Max(1, lineCount);
    }

    // Расчет высоты на основе количества строк
    private double CalculateHeightFromLineCount(int lineCount, double fontSize)
    {
        // Высота одной строки текста (в пунктах)
        double singleLineHeight = GetSingleLineHeight(fontSize);

        // Межстрочный интервал (1.2 - стандартный для Excel)
        double lineSpacing = 1.2;

        // Общая высота
        return singleLineHeight * lineCount * lineSpacing;
    }

    // Получение высоты одной строки текста
    private double GetSingleLineHeight(double fontSize)
    {
        // Высота текста в пунктах (fontSize) плюс небольшой отступ
        return fontSize + 4.0;
    }

    // Получение высоты строки по умолчанию
    private double GetDefaultRowHeight(double fontSize)
    {
        return GetSingleLineHeight(fontSize);
    }

    // Получение высоты отступов
    private double GetPaddingHeight(double fontSize)
    {
        // Отступы сверху и снизу (в пунктах)
        return fontSize * 0.5;
    }

    // Получение средней ширины символа
    private double GetAverageCharWidth(double fontSize, string fontName, bool isBold)
    {
        // Базовые значения для Calibri 11pt
        double baseWidth = 7.0; // пикселей

        // Корректировка для размера шрифта
        double sizeFactor = fontSize / 11.0;

        // Корректировка для жирного шрифта
        double boldFactor = isBold ? 1.1 : 1.0;

        // Корректировка для шрифта
        double fontFactor = 1.0;
        if (fontName.Equals("Arial", StringComparison.OrdinalIgnoreCase))
            fontFactor = 0.95;
        else if (fontName.Equals("Times New Roman", StringComparison.OrdinalIgnoreCase))
            fontFactor = 0.9;

        return baseWidth * sizeFactor * boldFactor * fontFactor;
    }

    // Конвертация ширины столбца Excel в пиксели
    private double ColumnWidthToPixels(double columnWidth, double fontSize)
    {
        // В Excel ширина столбца измеряется в символах стандартного шрифта (Calibri 11pt)
        // 1 символ ≈ 7 пикселей для Calibri 11pt
        double pixelsPerChar = 7.0;

        // Корректировка для размера шрифта
        double sizeFactor = fontSize / 11.0;

        return columnWidth * pixelsPerChar * sizeFactor;
    }

    // Подсчет явных переносов строк
    private int CountLineBreaks(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        return text.Count(c => c == '\n');
    }

    // Удаление символов переноса строк из текста
    private string RemoveLineBreaks(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text.Replace("\n", " ").Replace("\r", " ");
    }

    // Упрощенный метод для быстрого расчета
    public double QuickCalculateHeight(string text, double columnWidth, double fontSize = 11)
    {
        if (string.IsNullOrEmpty(text))
            return fontSize + 4.0;

        // Простая формула
        double charsPerLine = columnWidth * (11.0 / fontSize) * 0.85;
        int lineCount = (int)Math.Ceiling(text.Length / Math.Max(1, charsPerLine));

        // Учитываем переносы строк
        lineCount += text.Count(c => c == '\n');
        lineCount = Math.Max(1, lineCount);

        // Высота
        double height = (fontSize + 4.0) * lineCount * 1.15;

        return Math.Min(MaxRowHeight, Math.Max(fontSize + 4.0, height));
    }

    // Метод для расчета высоты с учетом дополнительных параметров отображения
    public double CalculateRowHeightAdvanced(string text, double columnWidth, double fontSize = 11,
                                            bool wrapText = true, double lineSpacing = 1.2,
                                            double verticalPadding = 2.0)
    {
        if (string.IsNullOrEmpty(text))
            return fontSize + 4.0 + (verticalPadding * 2);

        if (!wrapText)
        {
            // Без переноса текста - одна строка
            return fontSize + 4.0 + (verticalPadding * 2);
        }

        // С переносом текста
        double charsPerLine = CalculateCharsPerLine(columnWidth, fontSize);
        int lineCount = CalculateLineCount(text, charsPerLine);

        // Высота
        double singleLineHeight = fontSize + 4.0;
        double totalHeight = (singleLineHeight * lineCount * lineSpacing) + (verticalPadding * 2);

        return Math.Max(MinRowHeight, Math.Min(MaxRowHeight, totalHeight));
    }
}