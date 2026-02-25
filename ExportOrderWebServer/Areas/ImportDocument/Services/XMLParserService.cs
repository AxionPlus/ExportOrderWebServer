
using System.Globalization;
using System.Xml.Linq;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Port.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.Services
{
    public interface IXMLParserService
    {
        List<BillOfLadingBaseDto> ParseManifest(string xmlContent);
        List<BillOfLadingBaseDto> ParseManifestFromFile(string filePath);
        List<BillOfLadingBaseDto> ParseAllManifestsInDirectory(string directoryPath);
        BillOfLadingBaseDto ParseSingleBLFromXml(string xmlContent);
        void ExportToCsv(List<BillOfLadingBaseDto> bills, string outputPath);
        ParserStatistics GetParserStatistics();
    }

    public class ParserStatistics
    {
        public int TotalFilesParsed { get; set; }
        public int TotalBillsParsed { get; set; }
        public int TotalContainersParsed { get; set; }
        public int ParseErrors { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
        public DateTime LastParseTime { get; set; }
    }

    public class ParserConfiguration
    {
        public bool IgnoreParseErrors { get; set; } = true;
        public bool LogErrors { get; set; } = true;
        public bool LogToConsole { get; set; } = true;
        public bool LogToFile { get; set; } = false;
        public string LogFilePath { get; set; } = "parser_log.txt";
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public CultureInfo NumberCulture { get; set; } = CultureInfo.InvariantCulture;
        public Dictionary<string, string> FieldMappings { get; set; } = new()
        {
            { "BLNo", "Num" },
            { "Booking_Party", "BookingParty" },
            { "Shipper_name", "ShipperName" },
            { "Consignee_name", "ConsigneeName" },
            { "Part_BL", "PartBl" }
        };
    }

    public class XMLParserService : IXMLParserService
    {
        private readonly ParserConfiguration _configuration;
        private readonly ParserStatistics _statistics;
        private readonly ILogger<XMLParserService> _logger;
        private readonly ManifestParser _parser;

        public XMLParserService(ParserConfiguration configuration = null, ILogger<XMLParserService> logger = null)
        {
            _configuration = configuration ?? new ParserConfiguration();
            _statistics = new ParserStatistics();
            _logger = logger;
            _parser = new ManifestParser(_configuration, _statistics);
        }

        public List<BillOfLadingBaseDto> ParseManifest(string xmlContent)
        {
            _statistics.LastParseTime = DateTime.UtcNow;

            try
            {
                var result = _parser.ParseManifest(xmlContent);
                _statistics.TotalBillsParsed += result.Count;
                _statistics.TotalContainersParsed += result.Sum(b => b.TotalContainers);

                LogInfo($"Successfully parsed {result.Count} bill(s) from XML content");
                return result;
            }
            catch (Exception ex)
            {
                HandleError("Error parsing manifest from content", ex);
                throw;
            }
        }

        public List<BillOfLadingBaseDto> ParseManifestFromFile(string filePath)
        {
            _statistics.LastParseTime = DateTime.UtcNow;

            try
            {
                if (!File.Exists(filePath))
                {
                    var error = $"File not found: {filePath}";
                    HandleError(error, null);
                    throw new FileNotFoundException(error);
                }

                string xmlContent = File.ReadAllText(filePath);
                var result = ParseManifest(xmlContent);
                _statistics.TotalFilesParsed++;

                LogInfo($"Successfully parsed file: {Path.GetFileName(filePath)}");
                return result;
            }
            catch (Exception ex)
            {
                HandleError($"Error parsing file: {filePath}", ex);
                throw;
            }
        }

        public List<BillOfLadingBaseDto> ParseAllManifestsInDirectory(string directoryPath)
        {
            var allBills = new List<BillOfLadingBaseDto>();

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    var error = $"Directory not found: {directoryPath}";
                    HandleError(error, null);
                    throw new DirectoryNotFoundException(error);
                }

                var xmlFiles = Directory.GetFiles(directoryPath, "*.XML", SearchOption.AllDirectories)
                                       .Concat(Directory.GetFiles(directoryPath, "*.xml", SearchOption.AllDirectories))
                                       .ToList();

                LogInfo($"Found {xmlFiles.Count} XML files in directory: {directoryPath}");

                foreach (var file in xmlFiles)
                {
                    try
                    {
                        var bills = ParseManifestFromFile(file);
                        allBills.AddRange(bills);

                        LogInfo($"  ✓ {Path.GetFileName(file)}: {bills.Count} bill(s), {bills.Sum(b => b.TotalContainers)} container(s)");
                    }
                    catch (Exception ex) when (_configuration.IgnoreParseErrors)
                    {
                        HandleError($"Failed to parse file: {file}", ex);
                        // Продолжаем обработку других файлов
                    }
                }

                LogInfo($"Total parsed: {allBills.Count} bill(s), {allBills.Sum(b => b.TotalContainers)} container(s) from {xmlFiles.Count} file(s)");
                return allBills;
            }
            catch (Exception ex)
            {
                HandleError($"Error processing directory: {directoryPath}", ex);
                throw;
            }
        }

        public BillOfLadingBaseDto ParseSingleBLFromXml(string xmlContent)
        {
            try
            {
                var xmlDoc = XDocument.Parse(xmlContent);
                var blElement = xmlDoc.Root?.Element("BL");

                if (blElement == null)
                {
                    var error = "No BL element found in XML";
                    HandleError(error, null);
                    throw new InvalidOperationException(error);
                }

                var result = _parser.ParseSingleBL(blElement);
                _statistics.TotalBillsParsed++;
                _statistics.TotalContainersParsed += result.TotalContainers;

                LogInfo($"Successfully parsed single BL: {result.Num}");
                return result;
            }
            catch (Exception ex)
            {
                HandleError("Error parsing single BL from XML", ex);
                throw;
            }
        }

        public void ExportToCsv(List<BillOfLadingBaseDto> bills, string outputPath)
        {
            try
            {
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8);

                // Заголовок CSV
                writer.WriteLine("BL Number,BL Date,Shipper Code,Shipper Name,Consignee Code,Consignee Name,Origin,POL,POD,Final POD,Total Containers,Total Packages,Total Weight");

                foreach (var bill in bills)
                {
                    var totalPackages = bill.ContainerRecords.Sum(c => c.NoOfPackage);
                    var totalWeight = bill.ContainerRecords.Sum(c => c.GrossWeight);

                    var line = $"\"{bill.Num}\"," +
                              $"\"{bill.Date:yyyy-MM-dd}\"," +
                              $"\"{bill.ShipperCode}\"," +
                              $"\"{EscapeCsvField(bill.ShipperName)}\"," +
                              $"\"{bill.ConsigneeCode}\"," +
                              $"\"{EscapeCsvField(bill.ConsigneeName)}\"," +
                              $"\"{bill.Origin}\"," +
                              $"\"{bill.Pol}\"," +
                              $"\"{bill.Pod}\"," +
                              $"\"{bill.FinalPod}\"," +
                              $"{bill.TotalContainers}," +
                              $"{totalPackages}," +
                              $"{totalWeight}";

                    writer.WriteLine(line);
                }

                LogInfo($"Exported {bills.Count} bill(s) to CSV: {outputPath}");
            }
            catch (Exception ex)
            {
                HandleError($"Error exporting to CSV: {outputPath}", ex);
                throw;
            }
        }

        public ParserStatistics GetParserStatistics()
        {
            return new ParserStatistics
            {
                TotalFilesParsed = _statistics.TotalFilesParsed,
                TotalBillsParsed = _statistics.TotalBillsParsed,
                TotalContainersParsed = _statistics.TotalContainersParsed,
                ParseErrors = _statistics.ParseErrors,
                ErrorMessages = new List<string>(_statistics.ErrorMessages),
                LastParseTime = _statistics.LastParseTime
            };
        }

        public void ResetStatistics()
        {
            _statistics.TotalFilesParsed = 0;
            _statistics.TotalBillsParsed = 0;
            _statistics.TotalContainersParsed = 0;
            _statistics.ParseErrors = 0;
            _statistics.ErrorMessages.Clear();
            _statistics.LastParseTime = DateTime.MinValue;

            LogInfo("Parser statistics reset");
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            // Экранируем кавычки
            return field.Replace("\"", "\"\"");
        }

        private void HandleError(string message, Exception ex)
        {
            _statistics.ParseErrors++;
            var errorMessage = $"{message}: {ex?.Message}";
            _statistics.ErrorMessages.Add(errorMessage);

            if (_configuration.LogErrors)
            {
                if (_configuration.LogToConsole)
                {
                    Console.WriteLine($"[ERROR] {errorMessage}");
                    if (ex != null)
                        Console.WriteLine($"[STACKTRACE] {ex.StackTrace}");
                }

                if (_configuration.LogToFile)
                {
                    try
                    {
                        File.AppendAllText(_configuration.LogFilePath,
                            $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ERROR: {errorMessage}\n");
                    }
                    catch
                    {
                        // Игнорируем ошибки записи в лог-файл
                    }
                }

                _logger?.LogError(ex, message);
            }
        }

        private void LogInfo(string message)
        {
            if (_configuration.LogToConsole)
            {
                Console.WriteLine($"[INFO] {message}");
            }

            if (_configuration.LogToFile)
            {
                try
                {
                    File.AppendAllText(_configuration.LogFilePath,
                        $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] INFO: {message}\n");
                }
                catch
                {
                    // Игнорируем ошибки записи в лог-файл
                }
            }

            _logger?.LogInformation(message);
        }
    }

    internal class ManifestParser
    {
        private readonly ParserConfiguration _configuration;
        private readonly ParserStatistics _statistics;

        public ManifestParser(ParserConfiguration configuration, ParserStatistics statistics)
        {
            _configuration = configuration;
            _statistics = statistics;
        }

        public List<BillOfLadingBaseDto> ParseManifest(string xmlContent)
        {
            var xmlDoc = XDocument.Parse(xmlContent);
            var blElements = xmlDoc.Root?.Elements("BL");
            var carrier = xmlDoc.Root?.Element("Carrier")?.Value;

            if (blElements == null || !blElements.Any())
                return new List<BillOfLadingBaseDto>();

            var result = new List<BillOfLadingBaseDto>();

            foreach (var blElement in blElements)
            {
                try
                {
                    var dto = ParseSingleBL(blElement);
                    dto.Carrier = carrier;
                    result.Add(dto);
                }
                catch (Exception ex) when (_configuration.IgnoreParseErrors)
                {
                    _statistics.ParseErrors++;
                    var errorMessage = $"Error parsing BL element: {ex.Message}";
                    _statistics.ErrorMessages.Add(errorMessage);
                    // Продолжаем обработку других BL
                }
            }

            return result;
        }

        public BillOfLadingBaseDto ParseSingleBL(XElement blElement)
        {
            // Получаем TS порты из XML
            //var tsPorts = new List<string>();
            //for (int i = 1; i <= 4; i++)
            //{
            //    var tsPort = GetElementValue(blElement, $"TS{i}");
            //    if (!string.IsNullOrEmpty(tsPort) && tsPort != ".")
            //        tsPorts.Add(tsPort);
            //}

            var dto = new BillOfLadingBaseDto
            {
                Num = GetElementValue(blElement, "BLNo"),
                Date = ParseDate(GetElementValue(blElement, "BLDate")),
                TsPort = null,
                TsDate = ParseDate(GetElementValue(blElement, "FirstPOLLoadDate")),
                CustomerCode = GetElementValue(blElement, "CustomerCode"),
                BookingParty = GetElementValue(blElement, "Booking_Party"),
                Origin = GetElementValue(blElement, "Origin"),
                Pol = new PortDto{IsoCode = GetElementValue(blElement, "POL") } ,
                Pod = GetElementValue(blElement, "POD"),
                FinalPod = GetElementValue(blElement, "FPOD") ?? GetElementValue(blElement, "Place_of_Final_Delivery"),
                Shipper = GetElementValue(blElement, "Shipper"),
                ShipperCode = GetElementValue(blElement, "Shipper_code"),
                ShipperName = GetElementValue(blElement, "Shipper_name"),
                ShipperAddress = GetElementValue(blElement, "Shipper_address"),
                Consignee = GetElementValue(blElement, "Consignee"),
                ConsigneeCode = GetElementValue(blElement, "Consignee_code"),
                ConsigneeName = GetElementValue(blElement, "Consignee_name"),
                ConsigneeAddress = GetElementValue(blElement, "Consignee_address"),
                PartBl = GetElementValue(blElement, "Part_BL"),
                CargoDescription = GetElementValue(blElement, "CargoDescription"),
                ContainerRecords = ParseContainers(blElement),
                HandledBySystem=true,
            };

            ProcessAdditionalFields(blElement, dto);
            return dto;
        }

        private List<BillOfLadingContainerRecordBaseDto> ParseContainers(XElement blElement)
        {
            var containers = new List<BillOfLadingContainerRecordBaseDto>();
            var containersElement = blElement.Element("CONTAINERS");

            if (containersElement == null)
                return containers;

            foreach (var containerElement in containersElement.Elements("CONTAINER"))
            {
                try
                {
                    var containerDto = new BillOfLadingContainerRecordBaseDto
                    {
                        ContainerNo = GetElementValue(containerElement, "ContainerNo"),
                        ContainerTypeId = GetElementValue(containerElement, "ContainerTypeId"),
                        IsoCode = GetElementValue(containerElement, "ISOCode"),
                        TareWt = ParseInt(GetElementValue(containerElement, "TareWt")),
                        FullOrEmpty = GetElementValue(containerElement, "FullOrEmpty"),
                        IsSoc = GetElementValue(containerElement, "Ownership")?.Equals("SOC", StringComparison.OrdinalIgnoreCase) ?? false,
                        SealNo = GetElementValue(containerElement, "SealNo")?.Trim(),
                        PackageType = GetElementValue(containerElement, "PackageType"),
                        NoOfPackage = ParseInt(GetElementValue(containerElement, "NoOfPackage")),
                        GrossWeight = ParseDouble(GetElementValue(containerElement, "GrossWeight")),
                        GrossWeightUOM = GetElementValue(containerElement, "GrossWeightUOM"),
                        Volume = ParseInt(GetElementValue(containerElement, "Volume")),
                        OutOfGauge = ParseOutOfGauge(GetElementValue(containerElement, "OutOfGauge")),
                        IMCOClass = GetElementValue(containerElement, "IMCOClass"),
                        IMCONumber = GetElementValue(containerElement, "IMCONumber")?.Trim(),
                        ReeferTempSign = GetElementValue(containerElement, "CNTRReeferTempSign"), // знак
                        ReeferTemp = GetElementValue(containerElement, "CNTRReeferTemp"),   // температура цифра
                        ReeferTempUOM = GetElementValue(containerElement, "CNTRReeferTempUOM"),
                        ReeferHumidity = GetElementValue(containerElement, "ReeferHumidity") ?? GetElementValue(containerElement, "CNTRReeferHumidity"),
                        ReeferVentilation = GetElementValue(containerElement, "ReeferVentilation") ?? GetElementValue(containerElement, "CNTRReeferVentilation"),
                        BookingNo = GetElementValue(containerElement, "BookingNo"),
                        ContainerAsCargo = false,
                        HandledBySystem = true,
                    };

                    // ProcessReeferContainer(containerElement, containerDto); -не пойму для чего
                    containers.Add(containerDto);
                }
                catch (Exception ex) when (_configuration.IgnoreParseErrors)
                {
                    _statistics.ParseErrors++;
                    var errorMessage = $"Error parsing container {GetElementValue(containerElement, "ContainerNo")}: {ex.Message}";
                    _statistics.ErrorMessages.Add(errorMessage);
                }
            }

            return containers;
        }

        private void ProcessAdditionalFields(XElement blElement, BillOfLadingBaseDto baseDto)
        {
            // Обработка Notify полей
            var notify1Name = GetElementValue(blElement, "Notify1_name");
            var notify1Address = GetElementValue(blElement, "Notify1_address");

            if (!string.IsNullOrEmpty(notify1Name) && string.IsNullOrEmpty(baseDto.ConsigneeName))
            {
                baseDto.ConsigneeName = notify1Name;
                baseDto.ConsigneeAddress = notify1Address;
            }

            // Обработка дополнительных полей
            var additionalShipper = GetElementValue(blElement, "Shipper");
            var additionalConsignee = GetElementValue(blElement, "ConsigneeAddress");

            if (!string.IsNullOrEmpty(additionalShipper) && string.IsNullOrEmpty(baseDto.ShipperAddress))
            {
                baseDto.ShipperAddress = additionalShipper;
            }

            if (!string.IsNullOrEmpty(additionalConsignee) && string.IsNullOrEmpty(baseDto.ConsigneeAddress))
            {
                baseDto.ConsigneeAddress = additionalConsignee;
            }
        }

        private void ProcessReeferContainer(XElement containerElement, BillOfLadingContainerRecordBaseDto baseDto)
        {
            if (baseDto.ContainerTypeId?.StartsWith("R", StringComparison.OrdinalIgnoreCase) == true ||
                baseDto.ContainerTypeId?.Contains("H", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (string.IsNullOrEmpty(baseDto.ReeferTemp) && baseDto.ReeferTempSign != null)
                {
                    baseDto.ReeferTemp = "18";
                }
            }
        }

        private bool ParseOutOfGauge(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return value.Equals("YES", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ||
                   value == "1";
        }

        private string GetElementValue(XElement parent, string elementName)
        {
            // Используем маппинг из конфигурации
            var mappedName = _configuration.FieldMappings.ContainsKey(elementName)
                ? _configuration.FieldMappings[elementName]
                : elementName;

            var element = parent.Element(elementName);
            if (element == null)
                return string.Empty;

            var value = element.Value?.Trim();
            return string.IsNullOrEmpty(value) || value == "." ? string.Empty : value;
        }

        private DateTime? ParseDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString) ||
                dateString == "." ||
                dateString == ".." ||
                dateString.Trim() == "")
                return null;

            dateString = dateString.Trim();

            // Пробуем разные форматы
            string[] formats = {
                "yyyy-MM-dd",
                "dd.MM.yyyy",
                "MM/dd/yyyy",
                "yyyy/MM/dd",
                "dd-MM-yyyy",
                "yyyyMMdd",
                "ddMMyyyy",
                "MMddyyyy",
                "dd-MMM-yyyy",
                "dd MMM yyyy",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd HH:mm:ss",
                "dd.MM.yyyy HH:mm:ss"
            };

            // Сначала пробуем стандартный парсинг
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return result;

            // Пробуем точные форматы
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime exactResult))
                    return exactResult;
            }

            // Пробуем удалить время если есть
            var datePart = dateString.Split(' ', 'T')[0];
            if (DateTime.TryParse(datePart, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateOnlyResult))
                return dateOnlyResult;

          //  _logger?.LogDebug("Failed to parse date string: '{DateString}'", dateString);
            return null;
        }

        private int ParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == ".")
                return 0;

            var cleanValue = new string(value.Where(c => char.IsDigit(c) || c == '-' || c == '.').ToArray());

            if (int.TryParse(cleanValue, NumberStyles.Any, _configuration.NumberCulture, out int result))
                return result;

            if (double.TryParse(cleanValue, NumberStyles.Any, _configuration.NumberCulture, out double doubleResult))
                return (int)Math.Round(doubleResult);

            return 0;
        }

        private double ParseDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == ".")
                return 0;

            var cleanValue = new string(value.Where(c => char.IsDigit(c) || c == '-' || c == '.' || c == ',').ToArray());

            cleanValue = cleanValue.Replace(',', '.');

            if (double.TryParse(cleanValue, NumberStyles.Any, _configuration.NumberCulture, out double result))
                return result;

            return 0;
        }
    }
}

