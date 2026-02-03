using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using System.Text.RegularExpressions;
namespace ExportOrderWebServer.Areas.ImportDocument.Services;

public interface IGoogleSheetsParser
{
    Task<List<BillOfLadingBaseDto>> ParseSheetViaCsv(string googleTableUrl);
}
public class GoogleSheetsParser : IGoogleSheetsParser
{
    public async Task<List<BillOfLadingBaseDto>> ParseSheetViaCsv(string googleTableUrl)
    {
        var converter = new GoogleSheetsUrlConverter();
        // Формируем ссылку для экспорта в CSV
        var csvUrl = converter.ConvertToCsvUrl(googleTableUrl); ;

        using var client = new HttpClient();
        var response = await client.GetStringAsync(csvUrl);

        var lines = response.Split('\n');
        var result = new List<string[]>();
        var billOfLadings = new List<BillOfLadingBaseDto>();

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrEmpty(line.Trim())) continue;

            // Парсим CSV строку (упрощенный вариант)
            var columns = ParseCsvLine(line);
            result.Add(columns);
        }

        foreach (var billOfLadingGroup in result.GroupBy(s => s[1]))
        {
            var billOfLading = new BillOfLadingBaseDto
            {
                Num = billOfLadingGroup.Key,
            };
            if (billOfLadingGroup.Count() == 1)
            {
                billOfLading.ConsigneeNameRu = billOfLadingGroup.First()[4];
                billOfLading.ConsigneeAddressRu = billOfLadingGroup.First()[5];
                billOfLading.ConsigneeCountryRu = billOfLadingGroup.First()[6];

                billOfLading.CargoDescriptionRu = billOfLadingGroup.First()[3];

                billOfLading.CustomsMode = billOfLadingGroup.First()[7] switch
                {
                    "ДТ" => "ГТД",
                    "ТТ" => "ВТТ",
                    _ => "ГТД",
                };
            }
            else
            {
                billOfLading.ConsigneeNameRu = billOfLadingGroup.First()[4];
                billOfLading.ConsigneeAddressRu = billOfLadingGroup.First()[5];
                billOfLading.ConsigneeCountryRu = billOfLadingGroup.First()[6];

                billOfLading.CustomsMode = billOfLadingGroup.First()[7] switch
                {
                    "ДТ" => "ГТД",
                    "ТТ" => "ВТТ",
                    _ => "ГТД",
                };


                foreach (var record in billOfLadingGroup)
                {
                    var billOfLadingRecord = new BillOfLadingContainerRecordBaseDto
                    {
                        ContainerNo = record[2],
                        CargoDescriptionRu = record[3]
                    };

                    billOfLading.ContainerRecords.Add(billOfLadingRecord);
                }
            }

            billOfLadings.Add(billOfLading);

        }

        return billOfLadings;
    }

    private string[] ParseCsvLine(string line)
    {
        var columns = new List<string>();
        bool inQuotes = false;
        string currentColumn = "";

        foreach (var c in line)
        {
            switch (c)
            {
                case '"':
                    inQuotes = !inQuotes;
                    break;
                case ',' when !inQuotes:
                    columns.Add(currentColumn);
                    currentColumn = "";
                    break;
                default:
                    currentColumn += c;
                    break;
            }
        }

        columns.Add(currentColumn);
        return columns.ToArray();
    }
}

public class GoogleSheetsUrlConverter
{
    /// <summary>
    /// Преобразует ссылку на Google Sheets в CSV ссылку
    /// </summary>
    /// <param name="sheetUrl">Ссылка на Google Sheets</param>
    /// <returns>Ссылка для загрузки в CSV формате</returns>
    public string ConvertToCsvUrl(string sheetUrl)
    {
        try
        {
            // Извлекаем ID таблицы
            string spreadsheetId = ExtractSpreadsheetId(sheetUrl);

            // Извлекаем ID листа (gid)
            string sheetId = ExtractSheetId(sheetUrl);

            // Формируем CSV ссылку
            string csvUrl = $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/gviz/tq?tqx=out:csv";

            // Если есть ID листа, добавляем его
            if (!string.IsNullOrEmpty(sheetId))
            {
                csvUrl += $"&sheet={sheetId}";
            }

            return csvUrl;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Ошибка преобразования ссылки: {ex.Message}", nameof(sheetUrl));
        }
    }

    /// <summary>
    /// Извлекает ID таблицы из ссылки
    /// </summary>
    private string ExtractSpreadsheetId(string url)
    {
        // Регулярное выражение для поиска ID таблицы
        // Паттерн: /d/{id}/
        string pattern = @"/d/([a-zA-Z0-9-_]+)";
        var match = Regex.Match(url, pattern);

        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value;
        }

        throw new ArgumentException("Не удалось извлечь ID таблицы из ссылки");
    }

    /// <summary>
    /// Извлекает ID листа (gid) из ссылки
    /// </summary>
    private string ExtractSheetId(string url)
    {
        // Ищем gid= или #gid= в ссылке
        string pattern = @"[#&?]gid=(\d+)";
        var match = Regex.Match(url, pattern);

        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value;
        }

        // Если gid не найден, возвращаем null или пустую строку
        return null;
    }

    /// <summary>
    /// Альтернативный метод: преобразование с указанием имени листа вместо ID
    /// </summary>
    public string ConvertToCsvUrlWithSheetName(string sheetUrl, string sheetName = null)
    {
        try
        {
            string spreadsheetId = ExtractSpreadsheetId(sheetUrl);
            string csvUrl = $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/gviz/tq?tqx=out:csv";

            // Если указано имя листа, используем его
            if (!string.IsNullOrEmpty(sheetName))
            {
                csvUrl += $"&sheet={Uri.EscapeDataString(sheetName)}";
            }
            else
            {
                // Пытаемся использовать ID листа
                string sheetId = ExtractSheetId(sheetUrl);
                if (!string.IsNullOrEmpty(sheetId))
                {
                    csvUrl += $"&sheet={sheetId}";
                }
            }

            return csvUrl;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Ошибка преобразования ссылки: {ex.Message}", nameof(sheetUrl));
        }
    }
}
