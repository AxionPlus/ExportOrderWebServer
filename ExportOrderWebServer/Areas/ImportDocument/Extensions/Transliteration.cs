
using System;
using System.Collections.Generic;
using System.Text;
namespace ExportOrderWebServer.Areas.ImportDocument.Extensions;
public static class Transliteration
{
    private static readonly Dictionary<string, string> TranslitMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // Отдельные буквы
        ["a"] = "а",
        ["b"] = "б",
        ["v"] = "в",
        ["g"] = "г",
        ["d"] = "д",
        ["e"] = "е",
        ["yo"] = "ё",
        ["zh"] = "ж",
        ["z"] = "з",
        ["i"] = "и",
        ["y"] = "й",
        ["k"] = "к",
        ["l"] = "л",
        ["m"] = "м",
        ["n"] = "н",
        ["o"] = "о",
        ["p"] = "п",
        ["r"] = "р",
        ["s"] = "с",
        ["t"] = "т",
        ["u"] = "у",
        ["f"] = "ф",
        ["kh"] = "х",
        ["ts"] = "ц",
        ["ch"] = "ч",
        ["sh"] = "ш",
        ["shch"] = "щ",
        ["ie"] = "ы",
        ["e"] = "э",
        ["yu"] = "ю",
        ["ya"] = "я",

        // Специальные случаи
        ["'"] = "ь",
        ["''"] = "ъ",

        // Альтернативные варианты для сложных случаев
        ["c"] = "ц",   // для имён типа "Cezar"
        ["w"] = "в",   // для имён типа "Walter"
        ["q"] = "к",   // для имён типа "Queen"
        ["x"] = "кс",  // для имён типа "Xerox"
        ["h"] = "х",   // для общего случая
    };

    /// <summary>
    /// Выполняет транслитерацию текста с латиницы на кириллицу
    /// </summary>
    /// <param name="latinText">Текст на латинице</param>
    /// <returns>Текст на кириллице</returns>
    public static string ToCyrillic(string latinText)
    {
        if (string.IsNullOrEmpty(latinText))
            return latinText;

        var result = new StringBuilder();
        int i = 0;
        int length = latinText.Length;

        while (i < length)
        {
            bool matched = false;

            // Проверяем сначала длинные комбинации (до 4 символов)
            for (int len = 4; len >= 1; len--)
            {
                if (i + len <= length)
                {
                    string substring = latinText.Substring(i, len);

                    // Проверяем совпадение с учётом регистра
                    if (TranslitMap.TryGetValue(substring, out string cyrillic))
                    {
                        // Сохраняем регистр первой буквы
                        if (char.IsUpper(substring[0]))
                        {
                            if (cyrillic.Length > 1)
                                cyrillic = char.ToUpper(cyrillic[0]) + cyrillic.Substring(1);
                            else
                                cyrillic = cyrillic.ToUpper();
                        }

                        result.Append(cyrillic);
                        i += len;
                        matched = true;
                        break;
                    }
                }
            }

            if (!matched)
            {
                // Если символ не найден в словаре, оставляем как есть
                result.Append(latinText[i]);
                i++;
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Расширенная версия с дополнительной обработкой
    /// </summary>
    public static string ToCyrillicAdvanced(string latinText)
    {
        if (string.IsNullOrEmpty(latinText))
            return latinText;

        // Предварительная обработка
        string processed = latinText
            .Replace("ye", "е")     // для случаев типа "Yeltsin"
            .Replace("YE", "Е")
            .Replace("Ye", "Е");

        var result = ToCyrillic(processed);

        // Пост-обработка для коррекции распространённых ошибок
        result = result
            .Replace("ыа", "я")      // коррекция для "ya"
            .Replace("ыу", "ю")      // коррекция для "yu"
            .Replace("иа", "я")      // альтернативный вариант
            .Replace("иу", "ю");     // альтернативный вариант

        return result;
    }
}

// Пример использования
class Program
{
    static void Main()
    {
        // Тестовые примеры
        string[] testCases = {
            "Yeltsin",           // Ельцин
            "Putin",             // Путин
            "Medvedev",          // Медведев
            "Khrushchev",        // Хрущёв
            "Dostoevsky",        // Достоевский
            "Tchaikovsky",       // Чайковский
            "Schmidt",           // Шмидт
            "Cezar",             // Цезарь
            "Xerox",             // Ксерокс
            "Walter",            // Вальтер
            "Queen",             // Квин
            "Shakespeare",       // Шекспир
        };

        Console.WriteLine("Транслитерация с латиницы на кириллицу:\n");

        foreach (var test in testCases)
        {
            string cyrillic = Transliteration.ToCyrillic(test);
            Console.WriteLine($"{test,-15} -> {cyrillic}");
        }

        // Тест со сложным предложением
        string sentence = "Alexander Pushkin wrote Eugene Onegin";
        Console.WriteLine($"\nПредложение:\n{sentence}\n-> {Transliteration.ToCyrillic(sentence)}");
    }
}