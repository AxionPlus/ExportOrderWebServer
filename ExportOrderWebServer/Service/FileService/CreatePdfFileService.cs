using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Globalization;
using System.IO.Compression;
using DFD = DocumentFormat.OpenXml.Drawing;
using DFDWP = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using SpireDoc = Spire.Doc; // Converter: .docx => .pdf

namespace ExportOrderWebServer.Service.FileService;

public interface ICreatePdfFileService : IDisposable
{
    Task<KeyValuePair<string, byte[]>> CreateFilesExportOrder(IEnumerable<ExportOrderFileDto> Items);
    Task<KeyValuePair<string, byte[]>> CreateFilesBillOfLading(IEnumerable<ExportOrderFileDto> Items);
    Task<byte[]> CreateFileCustoms(IEnumerable<ExportOrderFileDto> Items);
    Task<byte[]> CreateFileManifest(IEnumerable<ExportOrderFileDto> Items, bool isIMO = false);
}

public class CreatePdfFileService : ICreatePdfFileService
{
    private static readonly string DirResources = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
    private string DirTemporary { get; set; } = string.Empty;
    private string FilePath { get; set; } = string.Empty;
    private string FileName { get; set; } = string.Empty;

    public CreatePdfFileService()
    {
        DirTemporary = Path.Combine(DirResources, "TempFiles", Path.GetRandomFileName());

        if (!Directory.Exists(DirTemporary))
            Directory.CreateDirectory(DirTemporary);
    }

    public void Dispose()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);

        if (Directory.Exists(DirTemporary))
            Directory.Delete(DirTemporary, true);
    }

    #region CREATE FILES

    public async Task<KeyValuePair<string, byte[]>> CreateFilesExportOrder(IEnumerable<ExportOrderFileDto> Items)
    {
        try
        {
            foreach (var item in Items)
            {
                /// Копирование файла из образца
                FileName = $"{item.Num}_{item.VoyageNum}.docx";

                CreateTemporaryFile("ExportOrder.docx");

                /// CREATE a SINGLE FILE
                CreateExportOrderSingleFile(item);

                /// CONVERT WORD TO PDF
                //ConvertWordToPdf(FilePath);
            }

            /// DELETE all Word files before zip creation                
            //foreach (var file in Directory.GetFiles(DirTemporary))
            //{
            //    if (Path.GetExtension(file).Equals(".docx", StringComparison.OrdinalIgnoreCase))
            //        File.Delete(file);
            //}

            /// CREATE ZIP-FILE if any and return it's bytes
            if (Items.Count() > 1)
            {
                FilePath = $"{DirTemporary}.zip";
                ZipFile.CreateFromDirectory(DirTemporary, FilePath);
                FileName = $"Voyage_{Items.Select(s => s.VoyageNum).FirstOrDefault()!}.zip";
            }

            byte[] buffer = await File.ReadAllBytesAsync(FilePath);

            return new(FileName, buffer);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
            return new(string.Empty, []);
            throw new ApplicationException($"Не удалось создать Excel файл.", ex);
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
            return new(string.Empty, []);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return new(string.Empty, []);
            throw new ArgumentException(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return new(string.Empty, []);
        }
    }

    public async Task<KeyValuePair<string, byte[]>> CreateFilesBillOfLading(IEnumerable<ExportOrderFileDto> Items)
    {
        try
        {
            foreach (var item in Items)
            {
                /// Копирование файла из образца
                FileName = $"BL_{item.Num}_{item.VoyageNum}.docx";

                CreateTemporaryFile(Path.Combine("BillOfLading",
                    string.Concat(
                        "BillOfLading",
                        item.BLtemplate == BLTemplate.Standard || item.BLtemplate == BLTemplate.Lotka ? "" : item.BLtemplate.ToString(),
                        ".docx"
                    )
                ));

                /// CREATE a SINGLE FILE
                CreateBillOfLadingSingleFile(item);

                /// CONVERT WORD TO PDF
                //ConvertWordToPdf(FilePath);
            }

            /// DELETE all Word files before zip creation                
            //foreach (var file in Directory.GetFiles(DirTemporary))
            //{
            //    if (Path.GetExtension(file).Equals(".docx", StringComparison.OrdinalIgnoreCase))
            //        File.Delete(file);
            //}

            if (Items.Count() > 1)
            {
                /// CREATE ZIP-FILE if any and return it's bytes
                FilePath = $"{DirTemporary}.zip";
                ZipFile.CreateFromDirectory(DirTemporary, FilePath);
                FileName = $"Voyage_{Items.Select(s => s.VoyageNum).FirstOrDefault()!}.zip";
            }

            byte[] buffer = await File.ReadAllBytesAsync(FilePath);

            return new(FileName, buffer);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return new(string.Empty, []);
            throw new ApplicationException($"Не удалось создать Pdf файл.", ex);
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return new(string.Empty, []);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return new(string.Empty, []);
            throw new ArgumentException(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return new(string.Empty, []);
        }
    }

    public async Task<byte[]> CreateFileManifest(IEnumerable<ExportOrderFileDto> Items, bool isIMO = false)
    {
        try
        {
            /// Копирование файла из образца
            FileName = $"{Guid.NewGuid}.docx";

            CreateTemporaryFile("ExportManifest.docx");

            /// CREATE a SINGLE FILE
            CreateManifestSingleFile(Items, isIMO);

            /// CONVERT WORD TO PDF
            //ConvertWordToPdf(FilePath);

            byte[] buffer = await File.ReadAllBytesAsync(FilePath);

            return buffer;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return [];
            throw new ApplicationException($"Не удалось создать Pdf файл.", ex);
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return [];
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return [];
            throw new ArgumentException(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return [];
        }             
    }

    public async Task<byte[]> CreateFileCustoms(IEnumerable<ExportOrderFileDto> Items)
    {
        try
        {
            /// Копирование файла из образца
            FileName = $"{Guid.NewGuid}.docx";

            CreateTemporaryFile("CustomsLetter.docx");

            /// CREATE a SINGLE FILE
            CreateCustomsSingleFile(Items);

            /// CONVERT WORD TO PDF
            //ConvertWordToPdf(FilePath);

            byte[] buffer = await File.ReadAllBytesAsync(FilePath);

            return buffer;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return [];
            throw new ApplicationException($"Не удалось создать Pdf файл.", ex);
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return [];
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return [];
            throw new ArgumentException(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return [];
        }        
    }

    #endregion

    #region SINGLE FILE
    private void CreateExportOrderSingleFile(ExportOrderFileDto item)
    {
        try
        {
            using WordprocessingDocument doc = WordprocessingDocument.Open(FilePath, true);

            var mainPart = doc.MainDocumentPart;
            var body = mainPart?.Document?.Body;
            //var sections = body?.Descendants<SectionProperties>();

            //if (mainPart is null || body is null || sections is null)
            //    return;

            if (mainPart is null || body is null)
                return;
            
            /// Bookmarks
            Dictionary<string, string> BookmarkReplacements = FuncGetReplacementsExportOrder(item);

            foreach (var replacement in BookmarkReplacements)
            {
                /// Ищем закладку по имени
                var bookmarkStart = body.Descendants<BookmarkStart>()
                    .FirstOrDefault(b => b.Name == replacement.Key);

                if (bookmarkStart != null)
                    ReplaceBookmarkWithText(bookmarkStart, replacement.Value);
            }

            /// TABLES
            Table tableVoyage = body.Elements<Table>().ElementAt(1); // zero based index
            Table tableShipper = body.Elements<Table>().ElementAt(2);
            Table tableConsignee = body.Elements<Table>().ElementAt(3);
            Table tableCommodity = body.Elements<Table>().ElementAt(4);
            Table tableDeclaration = body.Elements<Table>().ElementAt(5);
            Table tableCntrs = body.Elements<Table>().ElementAt(6);              

            List<string> commodities = item.Commodities.ToList();

            if (!string.IsNullOrWhiteSpace(item.CommodityShort))
                commodities.Add(item.CommodityShort);

            InsertOrderVoyageData(tableVoyage, item);
            InsertOrderShippersData(tableShipper, item.Shippers);
            InsertOrderShippersData(tableConsignee, item.Consignees);
            InsertOrderShippersData(tableCommodity, commodities.ToArray());
            InsertOrderDeclarationsData(tableDeclaration, item.Records);
            InsertOrderContainersData(tableCntrs, item.Records);

            mainPart.Document?.Save();
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException(ex.Message);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private void CreateBillOfLadingSingleFile(ExportOrderFileDto item)
    {
        try
        {
            using WordprocessingDocument doc = WordprocessingDocument.Open(FilePath, true);

            var mainPart = doc.MainDocumentPart;
            var body = mainPart?.Document?.Body;

            if (mainPart is null || body is null)
                return;

            /// TABLES
            Table tableMain = body.Elements<Table>().ElementAt(0); // zero based index

            switch (item.BLtemplate)
            {
                case BLTemplate.Safetrans:
                    InsertBillSafetransMainData(tableMain, item);
                    /// Заполняем данные таблиц нижнего колонтитула: таблица 2 и далее
                    InsertBillSafetransFooterTable(mainPart, item);
                    break;
                case BLTemplate.Nca:
                    InsertBillNcaMainData(tableMain, item);
                    break;
                case BLTemplate.Lotka:
                    InsertBillLotkaMainData(mainPart, tableMain, item);
                    Table tableCntrs = body.Elements<Table>().ElementAt(1);
                    InsertBillContainersData(tableCntrs, item.Records);
                    break;
                default:
                    InsertBillMainData(tableMain, item);
                    tableCntrs = body.Elements<Table>().ElementAt(1);
                    InsertBillContainersData(tableCntrs, item.Records);
                    break;
            };

            mainPart.Document?.Save();
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            //Dispose();
            return;
        }
    }

    private void CreateManifestSingleFile(IEnumerable<ExportOrderFileDto> items, bool isIMO)
    {
        try
        {
            using WordprocessingDocument doc = WordprocessingDocument.Open(FilePath, true);

            var mainPart = doc.MainDocumentPart;
            var body = mainPart?.Document?.Body;
            var sections = body?.Descendants<SectionProperties>();

            if (mainPart is null || body is null)
                return;

            /// BOOKMARK in Header
            if (sections is not null)
            {
                foreach (var section in sections)
                {     
                    var headerReferences = section.Descendants<HeaderReference>();
                    foreach (var headerRef in headerReferences)
                    {
                        if (mainPart.GetPartById(headerRef.Id!) is HeaderPart headerPart)
                        {
                            var bookmarkStart = headerPart.Header?.Descendants<BookmarkStart>()
                                    .FirstOrDefault(b => b.Name == "header");

                            if (bookmarkStart != null)
                                ReplaceBookmarkWithText(bookmarkStart, isIMO ? "I M O" : "C A R G O");
                        }
                    }                    
                }
            }

            /// TABLES
            int indexTable = 0;

            /// Получаем первую строку данных для копирования стиля в новые строки (вторая строка в талице файла-шаблона)
            Table sourceTable = body.Elements<Table>().ElementAt(0);

            /// Добавляем недостающее количество таблиц после первой таблицы
            for (int i = 1; i < items.Count(); i++)
            {
                // вставляем новую таблицу
                Table clonedTable = (Table)sourceTable.CloneNode(true);
                body.InsertAfter(clonedTable, sourceTable);

                // вставляем разрыв страницы перед новой таблицей
                body.InsertBefore(CreatePageBreak(), clonedTable);
            }

            /// Заполняем все таблицы
            foreach (var item in items)
            {
                Table tableMain = body.Elements<Table>().ElementAt(indexTable); // zero based index
                InsertManifestItemData(tableMain, item);
                InsertManifestContainersData(tableMain, item.Records);

                indexTable++;
            }

            /// Таблица Итогов
            Table tableFooter = body.Elements<Table>().Last();
            InsertManifestFooterData(tableFooter, items);

            mainPart.Document?.Save();
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException(ex.Message);
        }
        catch (NullReferenceException ex)
        {
            throw new NullReferenceException(ex.Message);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private void CreateCustomsSingleFile(IEnumerable<ExportOrderFileDto> items)
    {
        var voyage = items.GroupBy(g => g.VesselCallId).FirstOrDefault()!
                        .Select(g => new ExportOrderFileDto()
                        {
                            DateExplanation = g.DateExplanation,
                            CustomsOfficeName = g.CustomsOfficeName,
                            CustomsOfficeNameShort = g.CustomsOfficeNameShort,
                            CustomsDapartment = g.CustomsDapartment,
                            
                            VesselName = g.VesselName,
                            VesselFlag = g.VesselFlag,
                            VoyageNum = g.VoyageNum,
                            
                            PersonFamily = g.PersonFamily,
                            PersonNameSurname = g.PersonNameSurname,
                            PersonBirthYear = g.PersonBirthYear,
                            PersonBirthPlace = g.PersonBirthPlace,
                            PersonCompany = g.PersonCompany,
                            PersonAddress = g.PersonAddress,
                            PersonPass = g.PersonPass,
                            PersonSign = g.PersonSign
                        }).FirstOrDefault()
            ?? throw new ArgumentException("Error in 'CreateCustomsSingleFile'. Voyage wasn't groupped by Id.");

        try
        {
            using WordprocessingDocument doc = WordprocessingDocument.Open(FilePath, true);

            var mainPart = doc.MainDocumentPart;
            var body = mainPart?.Document?.Body;
            //var sections = body?.Descendants<SectionProperties>();

            //if (mainPart is null || body is null || sections is null)
            //    return;

            if (mainPart is null || body is null)
                return;

            /// Bookmarks
            Dictionary<string, string> BookmarkReplacements = FuncGetReplacementsCustoms(voyage);

            foreach (var replacement in BookmarkReplacements)
            {
                /// Ищем закладку по имени
                var bookmarkStart = body.Descendants<BookmarkStart>()
                    .FirstOrDefault(b => b.Name == replacement.Key);

                if (bookmarkStart != null)
                    ReplaceBookmarkWithText(bookmarkStart, replacement.Value);
            }

            /// TABLES
            Table tableMain = body.Elements<Table>().ElementAt(0); // zero based index
            Table tableExportOrders = body.Elements<Table>().ElementAt(1);
            Table tableFooter = body.Elements<Table>().ElementAt(2);

            InsertCustomsVoyageData(tableMain, voyage);
            InsertCustomsRecordsData(tableExportOrders, items);
            InsertCustomsFooterData(tableFooter, voyage.DateExplanation, voyage.PersonSign);

            mainPart.Document?.Save();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Dispose();
            return;
        }
    }

    #endregion

    #region WORD PROCESSING

    private readonly Func<ExportOrderFileDto, Dictionary<string, string>> FuncGetReplacementsExportOrder = (item) =>
    {
        return new Dictionary<string, string>()
        {
            { "NUM", item.Num ?? string.Empty },
            { "Dated", item.Dated?.ToString("dd.MM.yyyy") ?? string.Empty },
            { "BLNum", item.BLNum ?? string.Empty },
            { "ContractNo", item.CarrierContract ?? string.Empty },
            { "ContractDated", item.CarrierContractDate ?? string.Empty},
            { "MyCompanyNameRu", item.MyCompanyName ?? string.Empty },
            { "MyCompanyEmail", item.MyCompanyEmail ?? string.Empty },
            { "Person", string.Concat(item.PersonSign ?? string.Empty, " (т. ", item.PersonPhone ?? string.Empty, ")") }
        };
    };

    private readonly Func<ExportOrderFileDto, Dictionary<string, string>> FuncGetReplacementsCustoms = (item) =>
    {
        return new Dictionary<string, string>()
        {
            { "Vessel", item.VesselName ?? string.Empty },
            { "VesselFlagVoyage", string.Concat("флаг ", item.VesselFlag, ", рейс ", item.VoyageNum) ?? string.Empty },
        };
    };

    private static void ReplaceBookmarkWithText(BookmarkStart start, string text)
    {
        var run = new Run(new Text(text));

        var current = start.NextSibling();

        try
        {
            while (current != null && !(current is BookmarkEnd && ((BookmarkEnd)current).Id == start.Id))
            {
                /// Получаем свойства форматирования текста (RunProperties) перед удалением Закладки
                if (current is Run currentRun)
                {
                    var runProperties = currentRun.RunProperties;

                    if (runProperties != null)
                        run.RunProperties = (RunProperties)runProperties.Clone();
                    else
                        /// Создаем базовое форматирование
                        run.RunProperties = new RunProperties(
                            new RunFonts() { Ascii = "Arial" },
                            new FontSize() { Val = "20" }
                        );
                }

                /// Remove Bookmark
                var next = current.NextSibling();
                current.Remove();
                current = next;
            }

            /// Insert new text
            start.Parent?.InsertAfter(run, start);
        }
        catch (Exception ex) { Console.WriteLine($"Error Bookmark - {text}: {ex.Message}"); return; }
    }

    private static void InsertOrderVoyageData(Table table, ExportOrderFileDto item)
    {
        string fontName = "Arial";
        string fontSize = "20";

        TableRow tableRow = table.Elements<TableRow>().ElementAt(0);
        var cells = tableRow.Elements<TableCell>().ToArray();

        UpdateCell(cells[1], $"{item.VesselName} ({item.VesselFlag})", fontName, fontSize, true);
        UpdateCell(cells[2], $"Рейс: {item.VoyageNum}", fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(1);
        cells = tableRow.Elements<TableCell>().ToArray();

        UpdateCell(cells[1], item.POL, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(2);
        cells = tableRow.Elements<TableCell>().ToArray();

        UpdateCell(cells[1], item.LoadingDate ?? string.Empty, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(3);
        cells = tableRow.Elements<TableCell>().ToArray();

        UpdateCell(cells[1], item.PortOfDischargeEnCountryRus ?? string.Empty, fontName, fontSize);
    }

    private static void InsertOrderShippersData(Table table, string[] items)
    {
        string fontName = "Arial";
        string fontSize = "20";

        /// Получаем первую строку данных для копирования стиля в новые строки (вторая строка в талице файла-шаблона)
        TableRow sourceRow = table.Elements<TableRow>().ElementAt(1);

        /// Новые строки
        for (int i = 1; i < items.Length; i++)
        {
            table.Append((TableRow)sourceRow.Clone());
        }

        /// Данные
        int rowIndex = 1;
        foreach (string item in items)
        {
            TableRow tableRow = table.Elements<TableRow>().ElementAt(rowIndex);
            var cells = tableRow.Elements<TableCell>().ToArray();

            UpdateCell(cells[0], item ?? string.Empty, fontName, fontSize);

            rowIndex++;
        }
    }

    private static void InsertOrderDeclarationsData(Table table, IEnumerable<ExportOrderRecordFileDto> itemRecords)
    {
        string fontName = "Arial";
        string fontSize = "20";

        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        numberFormat.NumberGroupSeparator = " "; // пробел как разделитель тысяч

        var documents = itemRecords.GroupBy(r => r.DocumentName).Select(g => new
        {
            g.Key,
            CntrTareWt = g.Sum(x => x.CntrTareWt),
            PackageQty = g.Sum(x => x.PackageQty),
            NetWt = g.Sum(x => x.NetWt),
            GrossWt = g.Sum(x => x.GrossWt),
        }).ToArray();

        /// Получаем первую строку данных для копирования стиля в новые строки (вторая строка в талице файла-шаблона)
        TableRow sourceRow = table.Elements<TableRow>().ElementAt(1);

        /// Новые строки (+1 строка Итогов)
        for (int i = 0; i < documents.Length; i++)  //for (int i = 1; i < documents.Length + 1; i++)
        {
            table.Append((TableRow)sourceRow.Clone());
        }

        /// Данные по строкам начиная со второй
        int rowIndex = 1; // zero based index
        
        foreach (var item in documents)
        {
            TableRow row = table.Elements<TableRow>().ElementAt(rowIndex);
            var cells = row.Elements<TableCell>().ToArray();

            UpdateCell(cells[0], rowIndex.ToString().Trim(), fontName, fontSize);
            UpdateCell(cells[1], item.Key ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[2], item.CntrTareWt?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[3], item.PackageQty?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[4], item.NetWt?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[5], item.GrossWt?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);

            rowIndex++;
        }

        /// Данные итогов (последняя строка)
        TableRow footerRow = table.Elements<TableRow>().Last();
        var cellsFooter = footerRow.Elements<TableCell>().ToArray();
                
        UpdateCell(cellsFooter[1], "Итого:", fontName, fontSize, true);
        UpdateCell(cellsFooter[2], documents.Sum(r => r.CntrTareWt)?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);
        UpdateCell(cellsFooter[3], documents.Sum(r => r.PackageQty)?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);
        UpdateCell(cellsFooter[4], documents.Sum(r => r.NetWt)?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize, true);
        UpdateCell(cellsFooter[5], documents.Sum(r => r.GrossWt)?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize, true);
                
        /// Форматирование Первой строки (заголовок)
        TableRow headerRow = table.Elements<TableRow>().First();

        SetHeaderRowBottomBorder(headerRow);             
    }

    private static void InsertOrderContainersData(Table table, IEnumerable<ExportOrderRecordFileDto> itemRecords)
    {
        string fontName = "Courier New";
        string fontSize = "18";

        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        numberFormat.NumberGroupSeparator = " "; // пробел как разделитель тысяч

        /// Получаем первую строку данных для копирования стиля в новые строки (вторая строка в талице файла-шаблона)
        TableRow sourceRow = table.Elements<TableRow>().ElementAt(1);

        /// Новые строки
        for (int i = 1; i < itemRecords.Count(); i++)
        {
            table.Append((TableRow)sourceRow.Clone());
        }

        /// Данные по строкам начиная со второй
        int rowIndex = 1;

        foreach (var record in itemRecords)
        {
            TableRow tableRow = table.Elements<TableRow>().ElementAt(rowIndex);
            var cells = tableRow.Elements<TableCell>().ToArray();

            UpdateCell(cells[0], record.Seq.HasValue ? record.Seq.Value.ToString().Trim() : string.Empty, fontName, fontSize, true);
            UpdateCell(cells[1], record.DocumentName ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[2], record.ContainerNum ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[3], record.CntrTareWt?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[4], record.CntrType ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[5], record.Seal ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[6], record.PackageQty?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[7], record.NetWt?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[8], record.GrossWt?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize, true);

            rowIndex++;
        }

        /// Форматирование Первой строки (заголовок)
        TableRow headerRow = table.Elements<TableRow>().First();

        SetHeaderRowBottomBorder(headerRow);
    }

    private static void InsertBillMainData(Table table, ExportOrderFileDto item)
    {
        string fontName = "Aptos";
        string fontSize = "18";

        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        numberFormat.NumberGroupSeparator = " "; // пробел как разделитель тысяч
       
        /// Определяем высоту строк, которые могут быть увеличины
        double changeableRowsHeight = СhangeableRowHeight(table, 7)
            + СhangeableRowHeight(table, 9)
            + СhangeableRowHeight(table, 11)
            + СhangeableRowHeight(table, 13);

        /// Заполняем таблицу данными
        TableRow tableRow = table.Elements<TableRow>().ElementAt(1);
        var cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[2], item.BLNum, fontName, fontSize, true);

        tableRow = table.Elements<TableRow>().ElementAt(3);
        cells = tableRow.Elements<TableCell>().ToArray();        
        UpdateCellWithCollection(cells[0], item.ShippersEn, fontName, fontSize);
        UpdateCell(cells[1], item.BLNum ?? string.Empty, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(5);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[1], item.POLAgent ?? string.Empty, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(7);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[1], item.PODAgent ?? string.Empty, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(9);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCellWithCollection(cells[0], item.ConsigneesEn, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(11);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[0], item.VesselName ?? string.Empty, fontName, fontSize);
        UpdateCell(cells[1], item.VoyageNum ?? string.Empty, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(13);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[0], item.POLEnCountryEn ?? string.Empty, fontName, fontSize);
        UpdateCell(cells[1], item.PortOfDischargeEn ?? string.Empty, fontName, fontSize);        

        /// DESCRIPTION of goods
        tableRow = table.Elements<TableRow>().ElementAt(16);
        cells = tableRow.Elements<TableCell>().ToArray();        
        var rowDescriptionHeight = tableRow.TableRowProperties?.Elements<TableRowHeight>().FirstOrDefault();        
        double patternRowHeight = rowDescriptionHeight?.Val ?? 0;   // Стандартная (предустановленная) высота строки Description

        if (!string.IsNullOrWhiteSpace(item.CommodityShortEn))
            UpdateCell(cells[1], item.CommodityShortEn, fontName, fontSize);
        else
            UpdateCellWithCollection(cells[1], item.CommoditiesEn, fontName, fontSize);

        var cntrTypesGroup = item.Records.GroupBy(r => r.CntrType)
            .Select(g => new
            {
                g.Key,
                CntrsCount = g.Count(),
                CntrTareWts = g.Sum(x => x.CntrTareWt),
                GrossWts = g.Sum(x => x.GrossWt),
                Mesures = g.FirstOrDefault()!.Measurement
            }).ToArray();        

        /// Заполняем строки для группы: тип контейнера + вес тары + брутто вес
        for (int i = 0; i < cntrTypesGroup.Length; i++)
        {
            tableRow = table.Elements<TableRow>().ElementAt(16 + i);
            cells = tableRow.Elements<TableCell>().ToArray();

            UpdateCell(cells[0], string.Concat(cntrTypesGroup[i].CntrsCount.ToString("00"), "x", cntrTypesGroup[i].Key) ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[2], cntrTypesGroup[i].CntrTareWts?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[3], cntrTypesGroup[i].GrossWts?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[4], cntrTypesGroup[i].Mesures ?? string.Empty, fontName, fontSize);
        }

        /// Раздел Freight
        tableRow = table.Elements<TableRow>().ElementAt(40);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[0], item.Records.Count.ToString() ?? string.Empty, fontName, fontSize, true);
        UpdateCell(cells[1], item.POLEnCountryEn ?? string.Empty, fontName, fontSize);

        /// Продбираем высоту строк после внесения данных
        double extendedRowsHeight = СhangeableRowHeight(table, 7)
            + СhangeableRowHeight(table, 9)
            + СhangeableRowHeight(table, 11)
            + СhangeableRowHeight(table, 13);

        if (extendedRowsHeight > changeableRowsHeight)
        {
            // количество строк для удаления
            int extendedRowsCount = (int)Math.Ceiling(extendedRowsHeight / patternRowHeight);

            for (int i = 33; i > (33 - extendedRowsCount); i--)
            {
                table.Elements<TableRow>().ElementAt(i).Remove();
            }
        }
    }

    private static void InsertBillNcaMainData(Table table, ExportOrderFileDto item)
    {
        string fontName = "Arial Narrow";
        string fontSize = "16";

        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        numberFormat.NumberGroupSeparator = " "; // пробел как разделитель тысяч

        try
        {
            /// Определяем высоту строк, которые могут быть увеличины
            int[] rowIndexes = { 1, 4, 5, 7, 9, 10, 11 };
            double changeableRowsHeight = 0;

            foreach (int i in rowIndexes)
            {
                changeableRowsHeight += СhangeableRowHeight(table, i);
            }

            // так же определяем высоту строк с контейнерами на первом листе: c 13 по 42 = 30 строк
            for (int i = 13; i < 43; i++)
            {
                changeableRowsHeight += СhangeableRowHeight(table, i);
            }

            /// Заполняем таблицу данными
            TableRow tableRow = table.Elements<TableRow>().ElementAt(1);
            var cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCellWithCollection(cells[1], item.ShippersEn, fontName, fontSize);
            UpdateCell(cells[4], item.BLNum, fontName, fontSize, true);

            tableRow = table.Elements<TableRow>().ElementAt(4);
            cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCellWithCollection(cells[1], item.ConsigneesEn, fontName, fontSize);

            tableRow = table.Elements<TableRow>().ElementAt(5);
            cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCellWithCollection(cells[1], item.ConsigneesEn, fontName, fontSize);

            tableRow = table.Elements<TableRow>().ElementAt(9);
            cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCell(cells[0], item.VesselName ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[1], item.VoyageNum ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[2], item.POLEnCountryEn, fontName, fontSize);

            tableRow = table.Elements<TableRow>().ElementAt(11);
            cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCell(cells[0], item.PortOfDischargeEn ?? string.Empty, fontName, fontSize);

            /// Footer range of the Table
            tableRow = table.Elements<TableRow>().ElementAt(44);
            cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCell(cells[1], string.Concat(
                item.Records.Count.ToString(),
                "                  ",
                item.Records.Sum(r => r.PackageQty)?.ToString("N0", numberFormat)
                ),
                fontName, fontSize, true);

            /// DESCRIPTION of goods
            tableRow = table.Elements<TableRow>().ElementAt(13);    // первая строка раздела DESCRIPTION
            var rowDescriptionHeight = tableRow.TableRowProperties?.Elements<TableRowHeight>().FirstOrDefault();
            double patternRowHeight = rowDescriptionHeight?.Val ?? 0;   // высота первой строки раздела Description 

            int rowIndex = 13;

            if (item.Records.Count < 31)
            {
                foreach (var record in item.Records)
                {
                    tableRow = table.Elements<TableRow>().ElementAt(rowIndex);
                    cells = tableRow.Elements<TableCell>().ToArray();

                    UpdateCell(cells[0], record.ContainerNum ?? string.Empty, fontName, fontSize);
                    UpdateCell(cells[1], record.CntrType ?? string.Empty, fontName, fontSize);
                    UpdateCell(cells[2], record.Seal ?? string.Empty, fontName, fontSize);
                    UpdateCell(cells[3], record.PackageQty?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                    UpdateCell(cells[4],
                        string.IsNullOrWhiteSpace(item.CommodityShortEn)
                        ? record.CommodityEn ?? string.Empty
                        : rowIndex == 13
                        ? item.CommodityShortEn
                        : string.Empty,
                        fontName, fontSize);

                    UpdateCell(cells[5],
                        record.Volume.HasValue && record.Volume.Value > 0
                        ? record.Volume.Value.ToString("N3", numberFormat)
                        : record.GrossWt?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);

                    rowIndex++;
                }
            }
            else
            {
                var cntrTypesGroup = item.Records.GroupBy(r => r.CntrType)
                    .Select(g => new
                    {
                        g.Key,
                        CntrsCount = g.Count(),
                        PackageQtys = g.Sum(x => x.PackageQty),
                        GrossWts = g.Sum(x => x.GrossWt),
                        CommoditiesEn = string.Join("; ", g.Select(x => x.CommodityEn).ToArray())
                    }).ToArray();

                /// Заполняем строки для группы: тип контейнера + вес тары + брутто вес
                for (int i = 0; i < cntrTypesGroup.Length; i++)
                {
                    tableRow = table.Elements<TableRow>().ElementAt(rowIndex);
                    cells = tableRow.Elements<TableCell>().ToArray();

                    UpdateCell(cells[0], string.Concat(cntrTypesGroup[i].CntrsCount.ToString("00"), "x", cntrTypesGroup[i].Key) ?? string.Empty, fontName, fontSize);
                    UpdateCell(cells[3], cntrTypesGroup[i].PackageQtys?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                    UpdateCell(cells[4],
                        string.IsNullOrWhiteSpace(item.CommodityShortEn)
                        ? cntrTypesGroup[i].CommoditiesEn ?? string.Empty
                        : i == 0
                        ? item.CommodityShortEn
                        : string.Empty
                        , fontName, fontSize);
                    UpdateCell(cells[5], cntrTypesGroup[i].GrossWts?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
                }
            }

            /// Подсчитываем высоту строк после внесения данных
            double extendedRowsHeight = 0;

            foreach (int i in rowIndexes)
            {
                extendedRowsHeight += СhangeableRowHeight(table, i);
            }

            /// так же Подсчитываем высоту строк с контейнерами на первом листе: c 13 по 42 = 30 строк
            for (int i = 13; i < 43; i++)
            {
                extendedRowsHeight += СhangeableRowHeight(table, i);
            }

            /// Удаление лишних строк
            if (extendedRowsHeight > changeableRowsHeight)
            {
                // количество строк для удаления
                int extendedRowsCount = (int)Math.Ceiling(extendedRowsHeight / patternRowHeight);

                for (int i = 42; i > (42 - extendedRowsCount); i--)
                {
                    table.Elements<TableRow>().ElementAt(i).Remove();
                }
            }
        }
        catch(ArgumentException ex)
        {
            throw new ArgumentException(ex.Message);
        }
    }

    private static void InsertBillSafetransMainData(Table table, ExportOrderFileDto item)
    {
        string fontName = "Calibri";
        string fontSize = "16";

        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        numberFormat.NumberGroupSeparator = " "; // пробел как разделитель разрядов

        int rowIndex = 15;  // Первая строка раздела DESCRIPTION of goods

        try
        {
            /// Получаем первую строку для глубокого копирования в новые строки
            TableRow sourceRow = table.Elements<TableRow>().ElementAt(rowIndex);  // zero-based index

            /// Добавляем недостающее количество строк после первой строки раздела DESCRIPTION of goods
            for (int i = 1; i < item.Records.Count; i++)
            {
                TableRow clonedRow = (TableRow)sourceRow.CloneNode(true);
                table.InsertAfter(clonedRow, sourceRow);    //table.Append((TableRow)sourceRow.Clone());
            }

            /// Добавление границы для строки заголовка DESCRIPTION of goods
            TableRow headerRow = table.Elements<TableRow>().ElementAt(rowIndex - 1);
            SetHeaderRowBottomBorder(headerRow);

            /// Заполняем таблицу данными
            TableRow tableRow = table.Elements<TableRow>().ElementAt(2);
            var cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCellWithCollection(cells[0], item.ShippersEn, fontName, fontSize);
            UpdateCell(cells[1], item.BLNum, fontName, fontSize);

            tableRow = table.Elements<TableRow>().ElementAt(4);
            cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCellWithCollection(cells[0], item.ConsigneesEn, fontName, fontSize);

            tableRow = table.Elements<TableRow>().ElementAt(6);
            cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCellWithCollection(cells[0], item.ConsigneesEn, fontName, fontSize);

            tableRow = table.Elements<TableRow>().ElementAt(8);
            cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCell(cells[0], item.VesselName ?? string.Empty, fontName, fontSize);

            tableRow = table.Elements<TableRow>().ElementAt(10);
            cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCell(cells[0], item.VoyageNum ?? string.Empty, fontName, fontSize);

            tableRow = table.Elements<TableRow>().ElementAt(12);
            cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCell(cells[0], item.PortOfDischargeEn ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[1], item.POLEnCountryEn ?? string.Empty, fontName, fontSize);

            /// DESCRIPTION of goods            

            foreach (var record in item.Records)
            {
                tableRow = table.Elements<TableRow>().ElementAt(rowIndex);
                cells = tableRow.Elements<TableCell>().ToArray();

                UpdateCell(cells[0], record.ContainerNum ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[1], record.CntrType ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[2], record.Seal ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[3], record.PackageQty?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                
                if (!string.IsNullOrWhiteSpace(item.CommodityShortEn))
                {
                    if (rowIndex == 15)
                        UpdateCell(cells[4], item.CommodityShortEn ?? string.Empty, fontName, fontSize);
                }
                else
                    UpdateCell(cells[4], record.CommodityEn ?? string.Empty, fontName, fontSize);

                UpdateCell(cells[5], record.GrossWt?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[6], record.CntrTareWt?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);

                rowIndex++;
            }

            /// Строка итогов раздела DESCRIPTION of goods
            TableRow footerRow = table.Elements<TableRow>().LastOrDefault()!;
            cells = footerRow.Elements<TableCell>().ToArray();

            //UpdateCell(cells[2], item.Records.Sum(r => r.PackageQty)?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[2], item.Records.Sum(r => r.PackageQty) != 0 ? item.Records.Sum(r => r.PackageQty)!.Value.ToString("N0", numberFormat) : string.Empty, fontName, fontSize, true);
            UpdateCell(cells[4], item.Records.Sum(r => r.GrossWt)?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[5], item.Records.Sum(r => r.CntrTareWt)?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException(ex.Message);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private static void InsertBillSafetransFooterTable(MainDocumentPart mainPart, ExportOrderFileDto item)
    {
        var sections = mainPart.Document?.Descendants<SectionProperties>().ToArray();

        if (sections is null) return;

        for (int i = 0; i < sections.Length; i++)
        {
            var section = sections[i];
            var footerReferences = section.Descendants<FooterReference>();

            foreach (var footerRef in footerReferences)
            {
                if (footerRef is null || footerRef.Id == null) continue;

                if (footerRef.Type != null &&
                    (footerRef.Type.Value == HeaderFooterValues.Default || //footerRef.Type.Value == HeaderFooterValues.Even ||
                     footerRef.Type.Value == HeaderFooterValues.First))
                {
                    try
                    {
                        var footerPart = mainPart.GetPartById(footerRef?.Id!) as FooterPart;
                        if (footerPart?.Footer != null)
                        {
                            var footerTable = footerPart.Footer.Descendants<Table>().FirstOrDefault();
                            if (footerTable is null) continue;

                            /// Заполняем таблицу данными
                            string fontName = "Calibri";
                            string fontSize = "16";

                            var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                            numberFormat.NumberGroupSeparator = " "; // пробел как разделитель разрядов
                            
                            TableRow tableRow = footerTable.Elements<TableRow>().ElementAt(0);
                            var cells = tableRow.Elements<TableCell>().ToArray();
                            UpdateCell(cells[1], item.Records.Count.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);

                            //tableRow = footerTable.Elements<TableRow>().ElementAt(1);
                            //cells = tableRow.Elements<TableCell>().ToArray();
                            //UpdateCell(cells[1], item.BLDate ?? string.Empty, fontName, fontSize, true);
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ArgumentException(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при доступе к колонтитулу: {ex.Message}");
                    }
                }
            }
        }
    }

    private static void InsertBillLotkaMainData(MainDocumentPart mainPart, Table table, ExportOrderFileDto item)
    {
        try
        {
            /// Вставляем изображение в ячейку Логотипа
            TableRow tableRow = table.Elements<TableRow>().ElementAt(0);
            TableCell cell = tableRow.Elements<TableCell>().ElementAt(0);

            //// Очищаем содержимое ячейки
            //cell.RemoveAllChildren();

            // Добавляем изображение
            InsertImageIntoCell(table, mainPart);

            // Заполняем первый лист данными
            InsertBillMainData(table, item);
            
            //// Заполняем таблицу с контейнерами
            //var tableCntrs = mainPart?.Document.Body?.Elements<Table>().ElementAt(1);

            //if (tableCntrs !=  null)
            //    InsertBillContainersData(tableCntrs, item.Records);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException(ex.Message);
        }
    }

    private static void InsertBillContainersData(Table table, IEnumerable<ExportOrderRecordFileDto> itemRecords)
    {
        string fontName = "Aptos";
        string fontSize = "16";

        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        numberFormat.NumberGroupSeparator = " "; // пробел как разделитель тысяч

        /// Получаем первую строку данных для копирования стиля в новые строки (вторая строка в талице файла-шаблона)
        TableRow sourceRow = table.Elements<TableRow>().ElementAt(1);

        /// Новые строки (+1 строка Итогов)
        for (int i = 1; i < itemRecords.Count() + 1; i++)
        {
            table.Append((TableRow)sourceRow.Clone());
        }

        /// Данные по строкам начиная со второй
        int rowIndex = 1;

        foreach (var record in itemRecords)
        {
            TableRow tableRow = table.Elements<TableRow>().ElementAt(rowIndex);
            var cells = tableRow.Elements<TableCell>().ToArray();

            UpdateCell(cells[0], record.ContainerNum ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[1], record.Seal ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[2], record.PackageQty?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[3], record.PackageNames ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[4], record.CntrTareWt?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[5], record.GrossWt?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
            //UpdateCell(cells[6], record.Measurement ?? string.Empty, fontName, fontSize);

            rowIndex++;
        }

        /// Данные итогов (последняя строка)
        TableRow footerRow = table.Elements<TableRow>().Last();
        var cellsFooter = footerRow.Elements<TableCell>().ToArray();

        UpdateCell(cellsFooter[0], "TOTAL:", fontName, fontSize, true);
        //UpdateCell(cellsFooter[2], itemRecords.Sum(r => r.PackageQty)?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);
        UpdateCell(cellsFooter[2], itemRecords.Sum(r => r.PackageQty) != 0 ? itemRecords.Sum(r => r.PackageQty)!.Value.ToString("N0", numberFormat) : string.Empty, fontName, fontSize, true);
        UpdateCell(cellsFooter[4], itemRecords.Sum(r => r.CntrTareWt)?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);
        UpdateCell(cellsFooter[5], itemRecords.Sum(r => r.GrossWt)?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize, true);

        /// Форматирование строки Итогов
        SetHeaderRowBottomBorder(footerRow);

        /// Форматирование Первой строки (заголовок)
        TableRow headerRow = table.Elements<TableRow>().First();
        SetHeaderRowBottomBorder(headerRow);        
    } 

    private static void InsertCustomsVoyageData(Table table, ExportOrderFileDto item)
    {
        string fontName = "Times New Roman";
        string fontSize = "24";

        /// Заполняем таблицу данными
        TableRow tableRow = table.Elements<TableRow>().ElementAt(1);
        var cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[0], item.CustomsOfficeName ?? string.Empty, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(4);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[0], CustomsLetterDate(item.DateExplanation) ?? string.Empty, fontName, fontSize);
        UpdateCell(cells[1], item.CustomsDapartment ?? string.Empty, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(5);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[1], item.CustomsOfficeNameShort ?? string.Empty, fontName, fontSize);

        fontName = "Arial";
        fontSize = "20";

        tableRow = table.Elements<TableRow>().ElementAt(10);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[1], item.PersonFamily ?? string.Empty, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(11);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[1], item.PersonNameSurname ?? string.Empty, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(12);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[1], string.Concat(item.PersonBirthYear, " ", item.PersonBirthPlace) ?? string.Empty, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(13);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[1], item.PersonCompany ?? string.Empty, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(14);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[1], item.PersonAddress ?? string.Empty, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(16);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[0], item.PersonPass ?? string.Empty, fontName, fontSize);
    }

    private static void InsertCustomsRecordsData(Table table, IEnumerable<ExportOrderFileDto> exportOrders)
    {
        string fontName = "Arial";
        string fontSize = "16";

        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        numberFormat.NumberGroupSeparator = " "; // пробел как разделитель тысяч

        /// Получаем первую строку данных для копирования стиля в новые строки (вторая строка в талице файла-шаблона)
        TableRow sourceRow = table.Elements<TableRow>().ElementAt(1);

        /// Новые строки (+1 строка Итогов)
        for (int i = 0; i < exportOrders.Count(); i++)
        {
            table.Append((TableRow)sourceRow.Clone());
        }

        /// Данные по строкам начиная со второй
        int rowIndex = 1;

        foreach (var item in exportOrders)
        {
            TableRow tableRow = table.Elements<TableRow>().ElementAt(rowIndex);
            var cells = tableRow.Elements<TableCell>().ToArray();

            UpdateCell(cells[0], item.Num ?? string.Empty, fontName, fontSize);
            //UpdateCell(cells[1], item.Records?.Sum(r => r.PackageQty)?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[1], item.Records?.Count.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[2], item.Records?.Sum(r => r.GrossWt)?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[3], item.Records?.Sum(r => r.GrossAndTare)?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[4], item.CommodityShort ?? string.Join(", ", item.Commodities) ?? string.Empty, fontName, fontSize);
            //if (!string.IsNullOrWhiteSpace(item.CommodityShort))
            //    UpdateCell(cells[1], item.CommodityShort, fontName, fontSize);
            //else
            //    UpdateCellWithCollection(cells[1], item.Commodities, fontName, fontSize);
            UpdateCell(cells[5], item.BLNum ?? string.Empty, fontName, fontSize);
                        
            rowIndex++;
        }

        /// Данные итогов (последняя строка)
        TableRow footerRow = table.Elements<TableRow>().Last();
        var cellsFooter = footerRow.Elements<TableCell>().ToArray();

        UpdateCell(cellsFooter[0], "Итого:", fontName, fontSize, true);
        //UpdateCell(cellsFooter[1], exportOrders.SelectMany(eo => eo.Records).Sum(r => r.PackageQty)?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);
        UpdateCell(cellsFooter[1], exportOrders.SelectMany(eo => eo.Records).Count().ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);
        UpdateCell(cellsFooter[2], exportOrders.SelectMany(eo => eo.Records).Sum(r => r.GrossWt)?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize, true);
        UpdateCell(cellsFooter[3], exportOrders.SelectMany(eo => eo.Records).Sum(r => r.GrossAndTare)?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize, true);
    }

    private static void InsertCustomsFooterData(Table table, DateTime? docDate, string? personSign)
    {
        string fontName = "Times New Roman";
        string fontSize = "24";

        /// Заполняем таблицу данными
        TableRow tableRow = table.Elements<TableRow>().ElementAt(0);
        var cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[0], CustomsLetterDate(docDate) ?? string.Empty, fontName, fontSize);
        UpdateCell(cells[1], personSign ?? string.Empty, fontName, fontSize);

        tableRow = table.Elements<TableRow>().ElementAt(2);
        cells = tableRow.Elements<TableCell>().ToArray();
        UpdateCell(cells[0], CustomsLetterDate(docDate) ?? string.Empty, fontName, fontSize);
    }

    private static void InsertManifestItemData(Table table, ExportOrderFileDto item)
    {
        string fontName = "Consolas";
        string fontSize = "16";

        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        numberFormat.NumberGroupSeparator = " "; // пробел как разделитель тысяч

        try
        {
            /// Заполняем таблицу данными
            TableRow tableRow = table.Elements<TableRow>().ElementAt(1);
            var cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCell(cells[0], item.BLNum ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[1], item.VesselName ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[2], item.VesselFlagEn ?? string.Empty, fontName, fontSize);
            //UpdateCell(cells[3], item.BLDate ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[4], item.VoyageNum ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[5], item.POLEn ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[6], item.PortOfDischargeEn ?? string.Empty, fontName, fontSize);

            tableRow = table.Elements<TableRow>().ElementAt(3);
            cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCell(cells[2], string.Join("; ", item.CommoditiesEn.GroupBy(s => s).Select(g => g.Key)) ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[3], item.Records.Sum(s => s.GrossWt)?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[4], item.Records.Sum(s => s.CntrTareWt)?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[5], item.Records.Sum(s => s.GrossAndTare)?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize, true);

            tableRow = table.Elements<TableRow>().ElementAt(4);
            cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCellWithCollectionManifest(cells[0], item.ShippersEn, item.ConsigneesEn, fontName, fontSize);
        }
        catch(ArgumentException ex)
        {
            throw new ArgumentException(ex.Message);
        }        
    }

    private static void InsertManifestContainersData(Table table, IEnumerable<ExportOrderRecordFileDto> itemRecords)
    {
        string fontName = "Consolas";
        string fontSize = "16";

        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        numberFormat.NumberGroupSeparator = " "; // пробел как разделитель тысяч

        int rowIndex = 5;

        try
        {
            /// Получаем первую строку данных для копирования стиля в новые строки (вторая строка в талице файла-шаблона)
            TableRow sourceRow = table.Elements<TableRow>().ElementAt(rowIndex);

            /// Добавляем недостающее количество строк после первой строки раздела Контейнеры
            for (int i = 1; i < itemRecords.Count(); i++)
            {
                TableRow clonedRow = (TableRow)sourceRow.CloneNode(true);
                table.InsertAfter(clonedRow, sourceRow);
            }

            /// Данные по строкам начиная с первой для списка контейнеров = 5я строка таблицы.
            int Seq = 0; // счетчик записей
            foreach (var record in itemRecords)
            {
                TableRow tableRow = table.Elements<TableRow>().ElementAt(rowIndex);
                var cells = tableRow.Elements<TableCell>().ToArray();

                ++Seq;

                UpdateCell(cells[1], Seq.ToString().Trim(), fontName, fontSize);
                UpdateCell(cells[2], record.ContainerNum ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[3], record.CntrType ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[4], record.Seal ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[5], record.PackageQty?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[6], record.PackageNames ?? string.Empty, fontName, fontSize);
                //UpdateCell(cells[7], "FCL/FCL", fontName, fontSize, true);
                UpdateCell(cells[8], record.GrossWt?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[9], record.CntrTareWt?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[10], record.GrossAndTare?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);

                rowIndex++;
            }

            /// Форматирование Первой строки Контейнеров (заголовок)
            TableRow headerRow = table.Elements<TableRow>().ElementAt(4);
            SetHeaderRowBottomBorder(headerRow);

            /// Форматирование последней строки контейнеров
            TableRow bottomRow = table.Elements<TableRow>().Last();
            SetSummaryRowTopBorder(bottomRow);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException(ex.Message);
        }        
    }

    private static void InsertManifestFooterData(Table table, IEnumerable<ExportOrderFileDto> items)
    {
        string fontName = "Consolas";
        string fontSize = "16";

        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        numberFormat.NumberGroupSeparator = " "; // пробел как разделитель тысяч

        try
        {
            /// Общие данные
            var voyageData = items.GroupBy(s => s.VesselCallId).FirstOrDefault()!
                .Select(g => new
                {
                    g.VesselName,
                    g.VesselFlagEn,
                    g.BLDate,
                    g.VoyageNum,
                    g.POLEn,
                    g.PortOfDischargeEn,
                    CntrsCount = g.Records.Count,
                    GrossWts = g.Records.Sum(r => r.GrossWt),
                    CntrTare = g.Records.Sum(r => r.CntrTareWt),
                    GrossAndTare = g.Records.Sum(r => r.GrossAndTare)
                }).FirstOrDefault();

            if (voyageData is null) return;

            TableRow tableRow = table.Elements<TableRow>().ElementAt(1);
            var cells = tableRow.Elements<TableCell>().ToArray();
            UpdateCell(cells[1], voyageData.VesselName ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[2], voyageData.VesselFlagEn ?? string.Empty, fontName, fontSize);
            //UpdateCell(cells[3], voyageData.BLDate ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[4], voyageData.VoyageNum ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[5], voyageData.POLEn ?? string.Empty, fontName, fontSize);
            UpdateCell(cells[6], voyageData.PortOfDischargeEn ?? string.Empty, fontName, fontSize);

            /// Таблица итогов - Full
            var group20Full = items
                .SelectMany(s => s.Records)
                .Where(r => !string.IsNullOrWhiteSpace(r.CntrType))
                .Where(r => r.CntrType?.Substring(0, 2) == "20")
                .Where(r => r.GrossWt > 0)
                .GroupBy(r => r.CntrType?.Substring(0, 2))
                .Select(g => new
                {
                    CntrsCount = g.Count(),
                    GrossWts = g.Sum(x => x.GrossWt),
                    CntrTareWts = g.Sum(x => x.CntrTareWt),
                    GrossAndTares = g.Sum(x => x.GrossAndTare)
                })
                .FirstOrDefault();

            var group40Full = items
                .SelectMany(s => s.Records)
                .Where(r => !string.IsNullOrWhiteSpace(r.CntrType))
                .Where(r => r.CntrType?.Substring(0, 2) == "40")
                .Where(r => r.GrossWt > 0)
                .GroupBy(r => r.CntrType?.Substring(0, 2))
                .Select(g => new
                {
                    CntrsCount = g.Count(),
                    GrossWts = g.Sum(x => x.GrossWt),
                    CntrTareWts = g.Sum(x => x.CntrTareWt),
                    GrossAndTares = g.Sum(x => x.GrossAndTare)
                })
                .FirstOrDefault();

            var group45Full = items
                .SelectMany(s => s.Records)
                .Where(r => !string.IsNullOrWhiteSpace(r.CntrType))
                .Where(r => r.CntrType?.Substring(0, 2) == "45")
                .Where(r => r.GrossWt > 0)
                .GroupBy(r => r.CntrType?.Substring(0, 2))
                .Select(g => new
                {
                    CntrsCount = g.Count(),
                    GrossWts = g.Sum(x => x.GrossWt),
                    CntrTareWts = g.Sum(x => x.CntrTareWt),
                    GrossAndTares = g.Sum(x => x.GrossAndTare)
                })
                .FirstOrDefault();
                        
            if (group20Full != null)
            {
                tableRow = table.Elements<TableRow>().ElementAt(6);
                cells = tableRow.Elements<TableCell>().ToArray();

                UpdateCell(cells[3], group20Full.CntrsCount.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[4], group20Full.GrossWts?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[5], group20Full.CntrTareWts?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[6], group20Full.GrossAndTares?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
            }

            if (group40Full != null)
            {
                tableRow = table.Elements<TableRow>().ElementAt(7);
                cells = tableRow.Elements<TableCell>().ToArray();

                UpdateCell(cells[3], group40Full.CntrsCount.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[4], group40Full.GrossWts?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[5], group40Full.CntrTareWts?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[6], group40Full.GrossAndTares?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
            }

            if (group45Full != null)
            {
                tableRow = table.Elements<TableRow>().ElementAt(8);
                cells = tableRow.Elements<TableCell>().ToArray();

                UpdateCell(cells[3], group45Full.CntrsCount.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[4], group45Full.GrossWts?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[5], group45Full.CntrTareWts?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[6], group45Full.GrossAndTares?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
            }

            /// Таблица итогов - Empty
            var group20Empty = items
                .SelectMany(s => s.Records)
                .Where(r => !string.IsNullOrWhiteSpace(r.CntrType))
                .Where(r => r.CntrType?.Substring(0, 2) == "20")
                .Where(r => r.GrossWt is null || r.GrossWt == 0)
                .GroupBy(r => r.CntrType?.Substring(0, 2))
                .Select(g => new
                {
                    CntrsCount = g.Count(),
                    GrossWts = g.Sum(x => x.GrossWt),
                    CntrTareWts = g.Sum(x => x.CntrTareWt),
                    GrossAndTares = g.Sum(x => x.GrossAndTare)
                })
                .FirstOrDefault();

            var group40Empty = items
                .SelectMany(s => s.Records)
                .Where(r => !string.IsNullOrWhiteSpace(r.CntrType))
                .Where(r => r.CntrType?.Substring(0, 2) == "40")
                .Where(r => r.GrossWt is null || r.GrossWt == 0)
                .GroupBy(r => r.CntrType?.Substring(0, 2))
                .Select(g => new
                {
                    CntrsCount = g.Count(),
                    GrossWts = g.Sum(x => x.GrossWt),
                    CntrTareWts = g.Sum(x => x.CntrTareWt),
                    GrossAndTares = g.Sum(x => x.GrossAndTare)
                })
                .FirstOrDefault();

            var group45Empty = items
                .SelectMany(s => s.Records)
                .Where(r => !string.IsNullOrWhiteSpace(r.CntrType))
                .Where(r => r.CntrType?.Substring(0, 2) == "45")
                .Where(r => r.GrossWt is null || r.GrossWt == 0)                
                .GroupBy(r => r.CntrType?.Substring(0, 2))
                .Select(g => new
                {
                    CntrsCount = g.Count(),
                    GrossWts = g.Sum(x => x.GrossWt),
                    CntrTareWts = g.Sum(x => x.CntrTareWt),
                    GrossAndTares = g.Sum(x => x.GrossAndTare)
                })
                .FirstOrDefault();

            if (group20Empty != null)
            {
                tableRow = table.Elements<TableRow>().ElementAt(9);
                cells = tableRow.Elements<TableCell>().ToArray();

                UpdateCell(cells[3], group20Empty.CntrsCount.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[4], group20Empty.GrossWts?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[5], group20Empty.CntrTareWts?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[6], group20Empty.GrossAndTares?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
            }

            if (group40Empty != null)
            {
                tableRow = table.Elements<TableRow>().ElementAt(10);
                cells = tableRow.Elements<TableCell>().ToArray();

                UpdateCell(cells[3], group40Empty.CntrsCount.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[4], group40Empty.GrossWts?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[5], group40Empty.CntrTareWts?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[6], group40Empty.GrossAndTares?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
            }

            if (group45Empty != null)
            {
                tableRow = table.Elements<TableRow>().ElementAt(11);
                cells = tableRow.Elements<TableCell>().ToArray();

                UpdateCell(cells[3], group45Empty.CntrsCount.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[4], group45Empty.GrossWts?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[5], group45Empty.CntrTareWts?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize);
                UpdateCell(cells[6], group45Empty.GrossAndTares?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize);
            }

            /// Total
            tableRow = table.Elements<TableRow>().ElementAt(12);
            cells = tableRow.Elements<TableCell>().ToArray();

            UpdateCell(cells[3], items.SelectMany(s => s.Records).Count().ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[4], items.SelectMany(s => s.Records).Sum(r => r.GrossWt)?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[5], items.SelectMany(s => s.Records).Sum(r => r.CntrTareWt)?.ToString("N0", numberFormat) ?? string.Empty, fontName, fontSize, true);
            UpdateCell(cells[6], items.SelectMany(s => s.Records).Sum(r => r.GrossAndTare)?.ToString("N3", numberFormat) ?? string.Empty, fontName, fontSize, true);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException(ex.Message);
        }
        catch (NullReferenceException ex)
        {
            throw new NullReferenceException(ex.Message);
        }
    }

    /// MANAGE CELLS AND ROWS

    private static void UpdateCell(TableCell cell, string content, string fontName, string fontSize, bool isBold = false)
    {
        Text text = new(content) { Space = SpaceProcessingModeValues.Preserve };

        Run run = new(text)
        {
            // Создаем базовое форматирование
            RunProperties = new RunProperties(
                new RunFonts()
                {
                    Ascii = fontName,           // Для латиницы
                    HighAnsi = fontName,        // Для символов с кодами 80-FF (включая русские)
                    ComplexScript = fontName    // Для сложных скриптов (включая кириллицу)
                },
                new FontSize() { Val = fontSize })
        };

        if (isBold)
            run.RunProperties.AddChild(new Bold());

        var paragraph = cell.Elements<Paragraph>().FirstOrDefault();
        if (paragraph == null)
        {
            paragraph = new Paragraph() { ParagraphProperties = new() { Indentation = new() { Left = "57" } } };    // 57 twips = 1mm
            cell.Append(paragraph);
        }

        paragraph.Append(run);
    }

    private static void UpdateCellWithCollection(TableCell cell, string[] textCollection, string fontName, string fontSize)
    {
        // Задаем отступ слева в абзаце ячейки = 1мм (~57 twips)
        double marginInMm = 1.0; 
        int dxaMarginValue = (int)Math.Round(marginInMm * (1440.0 / 25.4)); //Open XML использует twips (1/1440 дюйма) для задания отступов.
        
        // Очищаем содержимое ячейки
        cell.RemoveAllChildren<Paragraph>();
        
        // Добавляем все элементы как новые параграфы
        for (int i = 0; i < textCollection.Length; i++)
        {
            Paragraph paragraph = new() { ParagraphProperties = new() { Indentation = new() { Left = dxaMarginValue.ToString() } } };
            Run run = new()
            {
                RunProperties = new(
                    new RunFonts() { Ascii = fontName },
                    new FontSize() { Val = fontSize })
            };
            Text text = new(textCollection[i]);
            run.Append(text);
            paragraph.Append(run);
            cell.Append(paragraph);
        }
    }

    private static void UpdateCellWithCollectionManifest(TableCell cell, string[] textShippers, string[] textConsignies, string fontName, string fontSize)
    {
        // Задаем отступ слева в абзаце ячейки = 1мм (~57 twips)
        double marginInMm = 1.0;
        int dxaMarginValue = (int)Math.Round(marginInMm * (1440.0 / 25.4)); //Open XML использует twips (1/1440 дюйма) для задания отступов.

        // Очищаем содержимое ячейки
        cell.RemoveAllChildren<Paragraph>();

        // Добавляем все элементы коллекции Shipper как новые параграфы
        for (int i = 0; i < textShippers.Length; i++)
        {
            Paragraph paragraphShipper = new() { ParagraphProperties = new() { Indentation = new() { Left = dxaMarginValue.ToString() } } };
            Run runShipper = new() { RunProperties = new()
                {
                    RunFonts = new RunFonts() { Ascii = fontName },
                    FontSize = new FontSize() { Val = fontSize },
                }
            };

            Text text = new(string.Concat("S: ", textShippers[i]));
            runShipper.Append(text);
            paragraphShipper.Append(runShipper);
            cell.Append(paragraphShipper);
        }

        // Добавляем один пустой параграф для разделения коллекций Shipper и Consignee
        Paragraph paragraph = new() { ParagraphProperties = new() { Indentation = new() { Left = dxaMarginValue.ToString() } } };
        Run run = new(new Text("---")) 
        {
            RunProperties = new(
                new RunFonts() { Ascii = fontName },
                new FontSize() { Val = fontSize })                        
        };

        paragraph.Append(run);
        cell.Append(paragraph);

        // Добавляем все элементы коллекции Consignee как новые параграфы
        for (int i = 0; i < textConsignies.Length; i++)
        {
            Paragraph paragraphConsignee = new() { ParagraphProperties = new() { Indentation = new() { Left = dxaMarginValue.ToString() } } };
            Run runConsignee = new()
            {
                RunProperties = new()
                {
                    RunFonts = new RunFonts() { Ascii = fontName },
                    FontSize = new FontSize() { Val = fontSize },
                }
            };

            Text text = new(string.Concat("C: ", textConsignies[i]));
            runConsignee.Append(text);
            paragraphConsignee.Append(runConsignee);
            cell.Append(paragraphConsignee);
        }
    }

    private static void SetHeaderRowBottomBorder(TableRow row)
    {
        TopBorder topBorder = new();
        BottomBorder bottomBorder = new();

        // Проверяем границы в первой ячейке строки
        var firstCell = row.Elements<TableCell>().First();

        var cellProps = firstCell?.Elements<TableCellProperties>().FirstOrDefault();
        var cellBorders = cellProps?.Elements<TableCellBorders>().FirstOrDefault();
        var existingTopBorder = cellBorders?.TopBorder;

        if (existingTopBorder != null)
        {
            // Создаем нижнюю границу на основе верхней
            bottomBorder.Val = existingTopBorder.Val;
            bottomBorder.Size = existingTopBorder.Size;
            bottomBorder.Color = existingTopBorder.Color;
            bottomBorder.Space = existingTopBorder.Space;
            bottomBorder.ThemeColor = existingTopBorder.ThemeColor;
            bottomBorder.ThemeTint = existingTopBorder.ThemeTint;
            bottomBorder.ThemeShade = existingTopBorder.ThemeShade;                        
        }
        else 
        {
            // Создаем новую верхнюю границу
            topBorder.Val = new EnumValue<BorderValues>(BorderValues.Single);
            topBorder.Size = 8;
            topBorder.Color = "000000";

            // Создаем новую нижнюю границу
            bottomBorder.Val = new EnumValue<BorderValues>(BorderValues.Single);
            bottomBorder.Size = 8;
            bottomBorder.Color = "000000";
        }

        // Применяем ко всем ячейкам строки
        foreach (TableCell cell in row.Elements<TableCell>())
        {
            var currentCellProps = cell.Elements<TableCellProperties>().FirstOrDefault();
            if (currentCellProps == null)
            {
                currentCellProps = new TableCellProperties();
                cell.InsertAt(currentCellProps, 0);
            }

            var currentCellBorders = currentCellProps.Elements<TableCellBorders>().FirstOrDefault();
            if (currentCellBorders == null)
            {
                currentCellBorders = new TableCellBorders();
                currentCellProps.Append(currentCellBorders);
            }

            currentCellBorders.BottomBorder = bottomBorder.Clone() as BottomBorder;

            if (existingTopBorder == null)
                currentCellBorders.TopBorder = topBorder.Clone() as TopBorder;
        }
    }

    private static void SetSummaryRowTopBorder(TableRow row)
    {
        TopBorder topBorder = new()
        {
            Val = new EnumValue<BorderValues>(BorderValues.Single),
            Size = 8,
            Color = "000000"
        };
        
        // Применяем ко всем ячейкам строки
        foreach (TableCell cell in row.Elements<TableCell>())
        {
            var currentCellProps = cell.Elements<TableCellProperties>().FirstOrDefault();
            if (currentCellProps == null)
            {
                currentCellProps = new TableCellProperties();
                cell.InsertAt(currentCellProps, 0);
            }

            var currentCellBorders = currentCellProps.Elements<TableCellBorders>().FirstOrDefault();
            if (currentCellBorders == null)
            {
                currentCellBorders = new TableCellBorders();
                currentCellProps.Append(currentCellBorders);
            }

            currentCellBorders.TopBorder = topBorder.Clone() as TopBorder;
        }
    }

    private static double СhangeableRowHeight(Table table, int rowNumber)
    {
        var row = table.Elements<TableRow>().ElementAt(rowNumber);
        var rowHeight = row.TableRowProperties?.Elements<TableRowHeight>().FirstOrDefault();
        return rowHeight?.Val ?? 0;
    }

    /// IMAGE
    private static void InsertImageIntoCell(Table table, MainDocumentPart mainPart)
    {
        try
        {
            string imagePath = Path.Combine(DirResources, "Images", "Lotka.JPG");

            // Проверяем существование файла
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Изображение не найдено: {imagePath}");
            }

            //// Рассчитываем размеры конечного изображения пикселях
            double dpi = 96.0;
            double widthPx = 0;
            double heightPx = 0;

            // Получаем 3 строки, которые занимает объедиенная ячейка Логотипа
            var imageRows = table.Elements<TableRow>().Take(3).ToArray();
            TableCell cell = imageRows[0].Elements<TableCell>().ElementAt(0);   // first cell in the first row

            // Ширина
            if (cell.TableCellProperties == null ||
                    cell.TableCellProperties.TableCellWidth == null ||
                    cell.TableCellProperties.TableCellWidth.Width == null ||
                    cell.TableCellProperties.TableCellWidth.Width.Value == null)
                return;

            double widthInTwips = double.Parse(cell.TableCellProperties.TableCellWidth.Width.Value);

            widthPx = (widthInTwips / 1440.0) * dpi;    // TWIP → дюймы → пиксели

            // Высота            
            foreach (var row in imageRows)
            {
                var rowProperties = row.GetFirstChild<TableRowProperties>();

                if (rowProperties != null)
                {
                    var rowHeight = rowProperties.GetFirstChild<TableRowHeight>();
                    if (rowHeight != null && rowHeight.Val != null)
                    {
                        double heightInTwips = rowHeight.Val.Value;
                        heightPx += (heightInTwips / 1440.0) * dpi;  // TWIP → дюймы → пиксели
                    }
                }
            }

            // Конвертируем пиксели в английские метрические единицы (EMU)
            // 1 пиксель = 9525 EMU (при 96 DPI)
            long imageWidth = (long)(widthPx * 9525);
            long imageHeight = (long)(heightPx * 9525);
            //---------------------------------------------------------------------------

            /// Читаем изображение
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            //string imageFileName = Path.GetFileName(imagePath);

            // Генерируем уникальный ID для изображения
            string imageId = $"image_{Guid.NewGuid().ToString().Replace("-", "")}";

            // Добавляем изображение в документ
            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg); // или другой тип
            using (MemoryStream stream = new(imageBytes))
            {
                imagePart.FeedData(stream);
            }
            //---------------------------------------------------------------------------
            //// Рассчитываем размеры изображения (в английских метрических единицах EMU)
            //long imageWidth = 1000000; // Пример: 1,5 см
            //long imageHeight = 1000000; // Пример: 1,5 см

            //// Получаем реальные размеры изображения
            //using (System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath))
            //{
            //    // Конвертируем пиксели в английские метрические единицы (EMU)
            //    // 1 пиксель = 9525 EMU (при 96 DPI)
            //    imageWidth = (long)(img.Width * 9525);
            //    imageHeight = (long)(img.Height * 9525);
            //}
            //---------------------------------------------------------------------------            

            // Создаем элемент Drawing для вставки изображения
            Drawing drawing = CreateImageDrawing(mainPart.GetIdOfPart(imagePart), imageId, imageWidth, imageHeight);

            // Добавляем Drawing в существующий параграф ячейки
            var paragraph = cell.Descendants<Paragraph>().FirstOrDefault();

            if (paragraph != null)
            {
                var run = paragraph.Descendants<Run>().FirstOrDefault()
                    ?? new Run();

                run.Append(drawing);
                paragraph.Append(run);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw new Exception("Error with image insert into first cell.");            
        }
    }

    private static Drawing CreateImageDrawing(string relationshipId, string imageId, long width, long height)
    {
        // Создаем Drawing с изображением
        Drawing drawing = new();

        // Inline для встраивания изображения в строку текста
        DFDWP.Inline inline =
            new DFDWP.Inline(
                new DFDWP.Extent()
                {
                    Cx = width,
                    Cy = height
                },
                new DFDWP.DocProperties()
                {
                    Id = 1U,
                    Name = imageId
                },
                new DFDWP.NonVisualGraphicFrameDrawingProperties(
                    new DFD.GraphicFrameLocks()
                    {
                        NoChangeAspect = true
                    }
                ),
                new DFD.Graphic(
                    new DFD.GraphicData(
                        new DFD.Pictures.Picture(
                            new DFD.Pictures.NonVisualPictureProperties(
                                new DFD.Pictures.NonVisualDrawingProperties()
                                {
                                    Id = 0U,
                                    Name = $"Picture {imageId}"
                                },
                                new DFD.Pictures.NonVisualPictureDrawingProperties()
                            ),
                            new DFD.Pictures.BlipFill(
                                new DFD.Blip(
                                    new DFD.BlipExtensionList(
                                        new DFD.BlipExtension()
                                        {
                                            Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                        }
                                    )
                                )
                                {
                                    Embed = relationshipId,
                                    CompressionState = DFD.BlipCompressionValues.Print
                                },
                                new DFD.Stretch(
                                    new DFD.FillRectangle()
                                )
                            ),
                            new DFD.ShapeProperties(
                                new DFD.Transform2D(
                                    new DFD.Offset()
                                    {
                                        X = 0L,
                                        Y = 0L
                                    },
                                    new DFD.Extents()
                                    {
                                        Cx = width,
                                        Cy = height
                                    }
                                ),
                                new DFD.PresetGeometry(
                                    new DFD.AdjustValueList()
                                )
                                {
                                    Preset = DFD.ShapeTypeValues.Rectangle
                                }
                            )
                        )
                    )
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                )
            )
            {
                DistanceFromTop = 0U,
                DistanceFromBottom = 0U,
                DistanceFromLeft = 0U,
                DistanceFromRight = 0U
            };

        drawing.Append(inline);
        return drawing;
    }

    /// Page Break - Метод для создания разрыва страницы
    private static Paragraph CreatePageBreak()
    {
        return new Paragraph(
            new Run(
                new Break() { Type = BreakValues.Page }
            )
        );
    }

    #endregion

    private static string CustomsLetterDate(DateTime? docDate) => string.Concat(
        "\"____\" ",
        docDate?.ToString("MMMM", CultureInfo.GetCultureInfo("ru-RU")),
        " ",
        docDate?.ToString("yyyy"), "г")
        ?? string.Empty;

    private void CreateTemporaryFile(string templateFileName)
    {
        string templateFilePath = Path.Combine(DirResources, templateFileName);

        if (!File.Exists(templateFilePath))
            throw new FileNotFoundException($"Файл Шаблона '{templateFileName}' не найден в папке ресурсов: {DirResources}");

        FilePath = Path.Combine(DirTemporary, FileName);

        if (File.Exists(templateFilePath))
            File.Copy(templateFilePath, FilePath);

        if (!File.Exists(FilePath))
            throw new IOException($"Failed to create temporary file at: {FilePath}");
    }

    private void ConvertWordToPdf(string _wordPath)
    {
        SpireDoc.Document document = new();
        document.LoadFromFile(_wordPath);

        FileName = FileName.Replace(".docx", ".pdf");
        FilePath = FilePath.Replace(".docx", ".pdf");

        document.SaveToFile(FilePath, SpireDoc.FileFormat.PDF);
    }

    //private static void UpdateCell(TableCell cell, string text, string fontName, string fontSize, bool isBold = false)
    //{
    //    Run run = new(new Text(text) { Space = SpaceProcessingModeValues.Preserve })
    //    {
    //        /// Создаем базовое форматирование
    //        RunProperties = new RunProperties(
    //            new RunFonts() { Ascii = fontName },
    //            new FontSize() { Val = fontSize })
    //    };

    //    if (isBold)
    //        run.RunProperties.AddChild(new Bold());

    //    Paragraph paragraph = cell.Elements<Paragraph>().FirstOrDefault()
    //        ?? new Paragraph() { ParagraphProperties = new() { Indentation = new Indentation() { Left = "57" } } };     // 57 twips = 1mm

    //    /// Записываем данные
    //    paragraph.Append(run);
    //}
}