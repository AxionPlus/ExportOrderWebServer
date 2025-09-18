using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace ExportOrderWebServer.Service;

public interface IPdfFileReadService : IDisposable
{
    Task<IEnumerable<ReadPdfExportOrderDTO>?> ReadPdfExportOrder(string filePath);
}

public class PdfFileReadService : IPdfFileReadService
{
    private string TemporaryFilePath { get; set; } = string.Empty;
    private ShippingLines ShippingLines { get; set; } = new ShippingLines();

    public PdfFileReadService() { }

    public async Task<IEnumerable<ReadPdfExportOrderDTO>?> ReadPdfExportOrder(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Word файл не найден");

        TemporaryFilePath = filePath;

        /// Read Pdf
        var _extractedData = ExtractTextFromPdf();

        if (_extractedData is null || !_extractedData.Any() || !_extractedData.First().StartsWith("SHIPPER / ON BEHALF OF"))
            return null;

        /// Get DTO
        await Task.Delay(5);
        return GetExportOrderDTO(_extractedData);
    }

    private List<string>? ExtractTextFromPdf()
    {
        var result = new List<string>();

        try
        {
            using (var document = PdfDocument.Open(TemporaryFilePath))
            {
                foreach (var page in document.GetPages())
                {
                    /// Получаем все слова на странице с их координатами
                    var words = page.GetWords();

                    /// Группируем слова по строкам (на основе Y-координат)
                    var lines = words.GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
                                     .OrderByDescending(g => g.Key);

                    foreach (var line in lines)
                    {
                        /// Сортируем слова по X-координате и объединяем в строку
                        var row = line.OrderBy(w => w.BoundingBox.Left)
                                     .Select(w => w.Text)
                                     .ToList();

                        //var rowText = string.Join(" | ", row);
                        var rowText = string.Join(" ", row);

                        result.Add(rowText);
                    }
                }
            }

            //if (File.Exists(_pdfPath))
            //    File.Delete(_pdfPath);
            Dispose();

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            //if (File.Exists(_pdfPath))
            //    File.Delete(_pdfPath);
            Dispose();
            return result;
        }
    }

    private List<ReadPdfExportOrderDTO>? GetExportOrderDTO(List<string> extractedData)
    {
        int seq = 0;

        string[] arrayPOL = { "НОВОРОССИЙСК", "(NVRSS) NOVOROSSIYSK" };

        List<ReadPdfExportOrderDTO> exportOrders = new();

        try
        {
            /// Re-write extracted data to a new Collection groupped by Export Order
            List<List<string>> data = new();
            List<string> orderLines = new();

            foreach (string line in extractedData)
            {
                if (line.StartsWith("SHIPPER / ON BEHALF OF"))
                {
                    if (orderLines.Any())
                        data.Add(orderLines);

                    orderLines = new();
                }

                orderLines.Add(line);
            }

            if (orderLines.Any())
                data.Add(orderLines);

            /// Write DTO
            foreach (var order in data)
            {
                ReadPdfExportOrderDTO exportOrderDTO = new() { Id = ++seq };

                string[] lines = order.ToArray();

                /// SHIPPER
                int indexConsignee = Array.FindIndex(lines, s => s.StartsWith("CONSIGNEE / ON BEHALF OF"));

                StringBuilder shipper = new();

                for (int i = 1; i < indexConsignee; i++)
                {
                    if (lines[i] == "SHIPPER" ||
                        lines[i] == "ПОРУЧЕНИЕ №" ||
                        lines[i] == "____________" ||
                        lines[i] == "НА ОТГРУЗКУ ЭКСПОРТНЫХ ТОВАРОВ" ||
                        lines[i].StartsWith("Отправитель / Представитель") ||
                        lines[i].StartsWith("отправителя"))
                        continue;

                    string lineShipper = lines[i];

                    /// Номер поручения
                    if (lines[i].StartsWith("Экспортное разрешение №"))
                    {
                        Match match = Regex.Match(lines[i], @"(от )(0[1-9]|[12][0-9]|3[01])\.(0[1-9]|1[0-2])\.\d{4}$");
                        if (match.Success)
                            lineShipper = lineShipper.Replace("Экспортное разрешение №", "").Replace(match.Value, "").Trim();

                        exportOrderDTO.ExpOrderNum = lineShipper;
                        continue;
                    }
                    
                    int indexTrashShipper = lineShipper.IndexOf("Экспортное разрешение №");
                    if (indexTrashShipper >= 0)
                        lineShipper = lineShipper.Substring(0, indexTrashShipper).Trim();

                    shipper.AppendLine(lineShipper.Trim());
                }

                exportOrderDTO.Shipper = shipper.ToString().Replace("\r\n", " ").Trim();

                /// CONSIGNEE                
                int indexNotify = Array.FindIndex(lines, s => s.StartsWith("NOTIFY PARTY"));
                StringBuilder consignee = new();

                for (int i = indexConsignee + 1; i < indexNotify; i++)
                {
                    if (lines[i].StartsWith("Получатель / Представитель") ||
                        lines[i].StartsWith("получателя") ||
                        lines[i] == "НА ОТГРУЗКУ ЭКСПОРТНЫХ ТОВАРОВ" ||
                        lines[i] == "CONSIGNEE")
                        continue;

                    string lineConsignee = lines[i];

                    /// Дата possible (пример: 01.09.2025)
                    if (Regex.IsMatch(lineConsignee, @"^(0[1-9]|[12][0-9]|3[01])\.(0[1-9]|1[0-2])\.\d{4}$"))
                        continue;

                    /// Номер поручения
                    if (lines[i].StartsWith("Экспортное разрешение №"))
                    {
                        Match match = Regex.Match(lines[i], @"(от )(0[1-9]|[12][0-9]|3[01])\.(0[1-9]|1[0-2])\.\d{4}$");
                        if (match.Success)
                            lineConsignee = lineConsignee.Replace("Экспортное разрешение №", "").Replace(match.Value, "").Trim();

                        exportOrderDTO.ExpOrderNum = lineConsignee;
                        continue;
                    }

                    consignee.AppendLine(lineConsignee);
                }

                exportOrderDTO.Consignee = consignee.ToString().Replace("\r\n", " ").Trim();

                /// VESSEL & VOYAGE
                int indexPOD = Array.FindIndex(lines, s => s.StartsWith("Порт выгрузки"));
                int indexVessel = Array.FindIndex(lines, s => s.StartsWith("Название судна Рейс судна"));
                if (indexVessel < 0)
                    indexVessel = Array.FindIndex(lines, s => s.StartsWith("Название Рейс судна"));

                if (indexVessel > 0)
                {
                    StringBuilder vessel = new();
                    for (int i = indexVessel + 1; i < indexPOD - 1; i++)
                    {
                        string lineVessel = lines[i];

                        /// Clear the line
                        if (lineVessel.Contains("к/с", StringComparison.CurrentCultureIgnoreCase) ||
                            lineVessel.Contains("оригиналов к/с", StringComparison.CurrentCultureIgnoreCase) ||
                            lineVessel.Contains("судна", StringComparison.CurrentCultureIgnoreCase))
                            continue;

                        int portIndexInVessel = lineVessel.IndexOf(arrayPOL[0]);
                        if (portIndexInVessel < 0)
                            portIndexInVessel = lineVessel.IndexOf(arrayPOL[1]);                        

                        if (portIndexInVessel > 0)
                            lineVessel = lineVessel.Substring(0, portIndexInVessel);

                        /// Line is clear
                        string[] lineVesselArray = lineVessel.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        if (lineVesselArray.Length >= 2)
                        {
                            /// Voyage
                            exportOrderDTO.VesselVoyage = lineVesselArray[lineVesselArray.Length - 1];

                            /// Vessel
                            for (int j = 0; j < lineVesselArray.Length - 1; j++)
                            {
                                vessel.Append(lineVesselArray[j] + ' ');
                            }
                        }
                        else                                
                            vessel.Append(lineVesselArray[0] + ' ');    /// Vessel
                        
                        if (vessel.ToString().Length > 0)
                            exportOrderDTO.VesselName = vessel.ToString().Trim();
                    }
                }

                /// PORT OF DISCHARGE
                if (indexPOD > 0)
                    exportOrderDTO.POD = lines[indexPOD + 1];

                /// SHIPPING LINE
                string[] foundLines = new string[] { "Manager/менеджер (конт. телефон):", "Manager/менеджер (конт." };
                string lineShippingLine = "";

                int indexShippingLine = Array.FindIndex(lines, s => s.StartsWith(foundLines[0]));
                if (indexShippingLine > 0)
                    lineShippingLine = lines[indexShippingLine].Substring(foundLines[0].Length).Trim();
                else
                {
                    indexShippingLine = Array.FindIndex(lines, s => s.StartsWith(foundLines[1]));
                    if (indexShippingLine > 0)
                        lineShippingLine = lines[indexShippingLine].Substring(foundLines[1].Length).Trim();
                }

                if (!string.IsNullOrEmpty(lineShippingLine))
                {
                    int indexEnd = lineShippingLine.IndexOf(", оформил");
                    if (indexEnd >= 0)
                        lineShippingLine = lineShippingLine.Substring(0, indexEnd).Trim();
                    else
                    {
                        if (ShippingLines.Name.Any(s => lineShippingLine.ToUpper().Contains(s.ToUpper())))
                            lineShippingLine = ShippingLines.Name.FirstOrDefault(s => lineShippingLine.ToUpper().Contains(s.ToUpper())) ?? string.Empty;
                    }

                    exportOrderDTO.ShippingLine = lineShippingLine;
                }

                /// COMMODITY & Cntrs               
                int indexCommodity = Array.IndexOf(lines, "Товары") + 4;

                List<string> commodities = new();
                List<string> cntrNums = new();
                List<double> grossWeights = new();

                Regex regexAnyLetter = new Regex(@"\p{L}");             // Регулярное выражение: любая буква (Unicode, включая кириллицу)

                StringBuilder commodity = new();

                for (int i = indexCommodity; i < lines.Length; i++)
                {
                    string lineCommodity = lines[i];

                    /// Exclude of exxtra line with possible "T"
                    if (lineCommodity == "Т") continue;

                    /// Clear the line
                    Match match = Regex.Match(lineCommodity, @"\(код:\s*d{3}\)");   // Регулярное выражение для: (код: 113)
                    if (match.Success)
                        lineCommodity = lineCommodity.Replace(match.Value, "");

                    match = Regex.Match(lineCommodity, @"\(код:");   // Регулярное выражение для: (код:
                    if (match.Success)
                        lineCommodity = lineCommodity.Replace(match.Value, "");

                    match = Regex.Match(lineCommodity, @"\d{3}\)");   // Регулярное выражение для:113)
                    if (match.Success)
                        lineCommodity = lineCommodity.Replace(match.Value, "");

                    if (lineCommodity.Trim().Length == 0) continue;
                    
                    string[] lineArray = lineCommodity.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    /// First Line in Commodity - the line, that contains Declaration Num or Number of word more then 8 and one of the word is a Cntr Num
                    if (Regex.IsMatch(lineArray[lineArray.Length - 1], @"\d{8}/\d{6}/\d{7}") ||
                        Regex.IsMatch(lineArray[lineArray.Length - 1], @"\d{5}/\d{6}/\d{7}") ||
                        (lineArray.Length >= 8 && Regex.IsMatch(lineArray[lineArray.Length - 1], @"[a-zA-Z]{4}[0-9]{7}")))
                    {
                        /// Previouse Commodity
                        if (commodity.Length > 0)
                        {
                            commodities.Add(commodity.ToString().TrimEnd());
                            commodity = new();
                        }

                        /// Current Commodity                        
                        /// Поиск с получением индекса
                        var lastWordInCommodity = lineArray.SkipLast(2).Select((word, index) => new { word, index })
                                                           .LastOrDefault(x => regexAnyLetter.IsMatch(x.word));

                        if (lastWordInCommodity != null)
                        {
                            /// Commodity                        
                            for (int j = 3; j <= lastWordInCommodity.index; j++)
                            {
                                commodity.Append(lineArray[j] + " ");
                            }
                        }

                        /// Cntr                        
                        var cntrInLine = lineArray.Select((word, index) => new { word, index })
                                                  .FirstOrDefault(x => Regex.IsMatch(x.word, @"[a-zA-Z]{4}[0-9]{7}"));

                        if (cntrInLine != null)
                        {
                            /// Cntr Num
                            cntrNums.Add(cntrInLine.word);
                            /// Cntr Weight
                            double weight = double.TryParse(lineArray[cntrInLine.index - 1], CultureInfo.GetCultureInfo("ru-RU"), out double _gwt) ?
                                Math.Round(_gwt, 3, MidpointRounding.AwayFromZero) : 0;
                            grossWeights.Add(weight);
                        }
                    }
                    else
                    {
                        foreach (var word in lineArray)
                        {
                            if (Regex.IsMatch(word, "[a-zA-Z]{4}[0-9]{7}"))
                                cntrNums.Add(word);
                            else if (regexAnyLetter.IsMatch(word))
                                commodity.Append(word + ' ');
                        }
                    }
                }

                /// Last Commodity
                if (commodity.Length > 0)
                    commodities.Add(commodity.ToString().TrimEnd());

                exportOrderDTO.Commodity = string.Join(", ", commodities.Distinct());
                exportOrderDTO.CntrsCount = cntrNums.Distinct().Count();
                exportOrderDTO.GrossWeight = grossWeights.Sum();

                exportOrders.Add(exportOrderDTO);
            }

            return exportOrders;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Record {seq}: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        if (File.Exists(TemporaryFilePath))
            File.Delete(TemporaryFilePath);
    }
}
