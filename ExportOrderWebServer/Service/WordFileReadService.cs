using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ExportOrderWebServer.Service;

public interface IWordFileReadService : IDisposable
{
    Task<IEnumerable<ReadPdfExportOrderDTO>?> ReadWordExportOrder(string filePath);
}

public class WordFileReadService : IWordFileReadService
{
    //private readonly static string DirTemporary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TempFiles");
    //private string TemporaryFilePath { get; set; } = Path.Combine(DirTemporary, $"{Path.GetRandomFileName()}.docx");
    private string TemporaryFilePath { get; set; } = string.Empty;
    private ShippingLines ShippingLines { get; set; } = new ShippingLines();
    public WordFileReadService() { }

    public async Task<IEnumerable<ReadPdfExportOrderDTO>?> ReadWordExportOrder(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Word файл не найден");

        //CreateTemporaryFile(filePath);
        TemporaryFilePath = filePath;

        //if (!File.Exists(TemporaryFilePath))
        //    throw new FileNotFoundException("Word файл не скопирован");

        using (WordprocessingDocument doc = WordprocessingDocument.Open(TemporaryFilePath, false))
        {
            var body = doc.MainDocumentPart?.Document.Body;
            var tables = body?.Elements<Table>().ToList();

            if (body is null || tables is null || !tables.Any())
                return null;

            int seq = 0;
            List<ReadPdfExportOrderDTO> exportOrdersDTO = new();
            ReadPdfExportOrderDTO dtoRecord = new();

            try
            {
                if (!tables[0].Elements<TableRow>().ToArray()[0].InnerText.Contains("SHIPPER / ON BEHALF OF", StringComparison.InvariantCultureIgnoreCase))
                    return null;

                for (int i = 0; i < tables.Count; i++)
                {
                    var arrayRows = tables[i].Elements<TableRow>().ToArray();

                    bool isCntrsTable = arrayRows[0].Elements<TableCell>().ToArray()[0]
                        .InnerText.Contains("№ по ДТ", StringComparison.InvariantCultureIgnoreCase);
                    
                    if (!isCntrsTable)   /// Table with Shipper and ExpOrder Num (first table)
                    {
                        dtoRecord = CreateExportOrderRecord(arrayRows);
                        dtoRecord.Id = ++seq;
                    }
                    else
                    {
                        /// CONTAINERS TABLE - supply an existed "dtoRecord" with containers data
                        List<string> commodities = new();                        
                        List<string> cntrNums = new();
                        List<double> grossWeights = new();

                        foreach (var row in arrayRows.Skip(1))
                        {
                            var cells = row.Elements<TableCell>().ToArray();

                            if (cells[0].InnerText.StartsWith("SHIPPER / ON BEHALF OF"))
                            {
                                dtoRecord = CreateExportOrderRecord(arrayRows);
                                dtoRecord.Id = ++seq;
                                break;
                            }                                

                            /// COMMODITY                            
                            commodities.Add(cells[5].InnerText
                                .Replace("\r", "")
                                .Replace("\a", "")
                                .Trim());

                            cntrNums.Add(cells[14].InnerText);

                            double grossWeight = double.TryParse(cells[12].InnerText,
                                                                    CultureInfo.GetCultureInfo("ru-RU"),
                                                                    out double _gwt) ?
                                Math.Round(_gwt, 3, MidpointRounding.AwayFromZero) : 0;

                            if (grossWeight > 0)
                                grossWeights.Add(grossWeight);
                        }

                        dtoRecord.Commodity = string.Join(", ", commodities.Distinct());
                        dtoRecord.GrossWeight = grossWeights.Sum();
                        dtoRecord.CntrsCount = cntrNums.Distinct().Count();

                        exportOrdersDTO.Add(dtoRecord);
                    }             
                }

                await Task.Delay(5);
                return exportOrdersDTO;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Dispose();
                return null;
            }
        }
    }

    private ReadPdfExportOrderDTO CreateExportOrderRecord(TableRow[] arrayRows)
    {
        ReadPdfExportOrderDTO record = new();

        try
        {
            /// SHIPPER
            int indexShipper = Array.FindIndex(arrayRows,
                        s => s.Elements<TableCell>().ToArray()[0].InnerText.StartsWith("SHIPPER / ON BEHALF OF"));

            if (indexShipper > -1)
            {
                record.Shipper = arrayRows[indexShipper + 1]
                        .Elements<TableCell>().ToArray()[0].InnerText
                        .Replace("\r", "")
                        .Replace("\a", "")
                        .Trim(); ;

                /// NUM
                string num = arrayRows[indexShipper]
                                .Elements<TableCell>().ToArray()[1].InnerText
                                .Replace("\r", "")
                                .Replace("\a", "")
                                .Trim();

                if (!string.IsNullOrWhiteSpace(num))
                {
                    num = num.Replace("ПОРУЧЕНИЕ № ____________", "").Replace("НА ОТГРУЗКУ ЭКСПОРТНЫХ ТОВАРОВ", "").Replace("Экспортное разрешение №", "");

                    num = num.Substring(0, num.IndexOf("от")).Trim();

                    record.ExpOrderNum = num;
                }
            }

            /// CONSIGNEE
            int indexConsignee = Array.FindIndex(arrayRows,
                s => s.Elements<TableCell>().ToArray()[0].InnerText.StartsWith("CONSIGNEE / ON BEHALF OF"));

            if (indexConsignee > -1)
                record.Consignee = arrayRows[indexConsignee + 1]
                                        .Elements<TableCell>().ToArray()[0].InnerText
                                        .Replace("\r", "")
                                        .Replace("\a", "")
                                        .Trim();

            /// VESSEL
            int indexVessel = Array.FindIndex(arrayRows,
                s => s.Elements<TableCell>().ToArray()[0].InnerText.StartsWith("VESSEL"));

            if (indexVessel > -1)
            {
                record.VesselName = arrayRows[indexVessel + 1]
                                    .Elements<TableCell>().ToArray()[0].InnerText
                                    .Replace("\r", "")
                                    .Replace("\a", "")
                                    .Trim();

                /// VOYAGE
                record.VesselVoyage = arrayRows[indexVessel + 1]
                                            .Elements<TableCell>().ToArray()[1].InnerText
                                            .Replace("\r", "")
                                            .Replace("\a", "")
                                            .Trim();
            }

            /// PORT OF DISCHARGE
            int indexPOD = Array.FindIndex(arrayRows,
                s => s.Elements<TableCell>().ToArray()[0].InnerText.StartsWith("PORT OF DISCHARGE"));

            if (indexPOD > -1)
                record.POD = arrayRows[indexPOD + 1]
                                            .Elements<TableCell>().ToArray()[0].InnerText
                                            .Replace("\r", "")
                                            .Replace("\a", "")
                                            .Trim();

            /// SHIPPING LINE
            int indexShippingLine = Array.FindIndex(arrayRows,
                s => s.Elements<TableCell>().ToArray()[0].InnerText.StartsWith("Manager/менеджер"));

            if (indexShippingLine > -1)
            {
                string lineText = arrayRows[indexShippingLine]
                                        .Elements<TableCell>().ToArray()[1].InnerText
                                        .Replace("\r", "")
                                        .Replace("\a", "")
                                        .Trim();

                var catalogShippingLine = ShippingLines.Name.FirstOrDefault(n => lineText.Contains(n, StringComparison.InvariantCultureIgnoreCase));

                if (!string.IsNullOrEmpty(catalogShippingLine))
                    record.ShippingLine = catalogShippingLine;
                else
                {
                    int indexEnd = lineText.IndexOf(", оформил");
                    if (indexEnd > -1)
                        lineText = lineText.Substring(0, indexEnd);

                    indexEnd = lineText.IndexOf(", телефон:");
                    if (indexEnd > -1)
                        lineText = lineText.Substring(0, indexEnd);

                    indexEnd = lineText.IndexOf("телефон:");
                    if (indexEnd > -1)
                        lineText = lineText.Substring(0, indexEnd);

                    Match match = Regex.Match(lineText, @"(0[1-9]|[12][0-9]|3[01])\.(0[1-9]|1[0-2])\.\d{4}$");   // Дата: 01.08.2025
                    if (match.Success)
                        lineText = lineText.Replace(match.Value, "");

                    match = Regex.Match(lineText, @"[\(]?(\+7|8|7)[\(\s\-]?[\(]?(\d{3})[\)\s\-]?[\s\-]?(\d{3})[\s\-]?(\d{2})[\s\-]?(\d{2})[\)]?");   // Номер телефона: (+7-918-0000000), 8-960-000-0000, +7 (905) 000-00-00, +7-964-000-00-00, и т.п. с пробелом вместо '-'
                    if (match.Success)
                        lineText = lineText.Replace(match.Value, "");

                    match = Regex.Match(lineText, @"\,\s(\d{2})\-(\d{2})\-(\d{2})$");   // Номер телефона: , 00-00-00
                    if (match.Success)
                        lineText = lineText.Replace(match.Value, "");

                    match = Regex.Match(lineText, @"\,\s(\d{2})\-(\d{2})\-(\d{2})$");   // Номер телефона: , 00-00-00
                    if (match.Success)
                        lineText = lineText.Replace(match.Value, "");

                    record.ShippingLine = lineText?.Trim();
                }
            }
            
            return record;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return record;
        }        
    }

    //private void CreateTemporaryFile(string templateFilePath)
    //{
    //    if (File.Exists(templateFilePath))
    //        File.Copy(templateFilePath, TemporaryFilePath);
    //}

    public void Dispose()
    {
        if (File.Exists(TemporaryFilePath))
            File.Delete(TemporaryFilePath);
    }
}

