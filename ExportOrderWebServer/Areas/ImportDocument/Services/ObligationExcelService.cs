using ClosedXML.Excel;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;
using Org.BouncyCastle.Asn1.X509;
using System.Reflection;
using System.Reflection.Metadata;

namespace ExportOrderWebServer.Areas.ImportDocument.Services;

public interface IObligationExcelService
{
    Task<byte[]> GenerateObligationAsync(VesselCallDto vesselCall);
}

public class ObligationExcelService : IObligationExcelService
{
    private readonly string _templatePath;

    public ObligationExcelService()
    {
        _templatePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Areas\\ImportDocument\\Resources\\Obligation_Template.xlsx";
#if DEBUG
        _templatePath =
            "C:\\Users\\Sergey Vasilenko\\source\\repos\\AxionPlus\\ExportOrderWebServer\\ExportOrderWebServer\\Areas\\ImportDocument\\Resources\\Obligation_Template.xlsx";
#endif
    }


    public async Task<byte[]> GenerateObligationAsync(VesselCallDto vesselCall)
    {
        if (!File.Exists(_templatePath))
        {
            throw new FileNotFoundException($"Шаблон не найден: {_templatePath}");
        }

        using var workbook = new XLWorkbook(_templatePath);

        var worksheet = workbook.Worksheet(1); // Первый лист

        var customsPost = vesselCall.Terminal.CustomsPost switch
        {
            "10317090" => "Начальнику\r\nНовороссийского западного\r\nтаможенного поста",
            "10317110" => "Начальнику\r\nНовороссийского юго-восточного\r\nтаможенного поста",
            _ => string.Empty
        };
        var vesselName = vesselCall.Vessel.Name;
        var vesselFlag = vesselCall.Vessel.FlagRu;
        var vesselVoyage = vesselCall.VoyageNo;

        var bodyText =
            $"ООО «ХАБ ШИППИНГ», действуя как агент от имени и по поручению перевозчика ALPHA SHIPPING (SHANGHAI) LTD., просит Вас разрешить временный ввоз на таможенную территорию Евразийского экономического союза до {DateTime.Today.AddYears(2):dd.MM.yyyy} для контейнеров, прибывающих на т/х {vesselName} (флаг - {vesselFlag}) рейс {vesselVoyage} по коносаментам, указанным в приложении. Обязуемся соблюдать сроки временного ввоза, а также обязуемся соблюдать следующие требования и ограничения, указанные в Конвенции О временном ввозе, заключенной в Стамбуле 26.06.1990 г.";

        worksheet.Cell(1, 8).Value = customsPost;
        worksheet.Cell(7, 1).Value = bodyText;


        // Получаем все контейнеры из BillOfLadings
        var allContainers = vesselCall.BillOfLadings?
            .SelectMany(bl => bl.ContainerRecords.Where(s => !s.IsSoc), (bl, record) => new { ContainerNo = record.ContainerNo, IsoCode = $"{record.ContainerTypeId.Substring(2, 2)}{record.ContainerTypeId.Substring(0, 2)}", TareWt = record.TareWt, BillOfLading = bl.Num }).DistinctBy(s=>s.ContainerNo).ToList().OrderBy(c => c.ContainerNo);

        // Разделяем контейнеры на две колонки
        var halfCount = (int)Math.Ceiling(allContainers.Count() / 2.0);

        var leftColumnContainers = allContainers.Take(halfCount).ToList();
        var rightColumnContainers = allContainers.Skip(halfCount).ToList();

        int index = 1;

        // Начальные строки для таблицы (согласно шаблону - строка 12)
        int currentRow = 12;


        foreach (var containerRecord in leftColumnContainers)
        {
            worksheet.Cell(currentRow, 1).Value = index;
            worksheet.Cell(currentRow, 2).Value = containerRecord.ContainerNo;
            worksheet.Cell(currentRow, 3).Value = containerRecord.IsoCode;
            worksheet.Cell(currentRow, 4).Value = containerRecord.TareWt;
            worksheet.Cell(currentRow, 5).Value = containerRecord.BillOfLading;

            worksheet.Range(currentRow, 1, currentRow, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Range(currentRow, 1, currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(currentRow, 1, currentRow, 5).Style.Font.FontSize = 10;
            worksheet.Range(currentRow, 1, currentRow, 5).Style.Font.FontName = "Courier New";

            currentRow++;
            index++;
        }

        currentRow = 12;
        foreach (var containerRecord in rightColumnContainers)
        {

            worksheet.Cell(currentRow, 6).Value = index;
            worksheet.Cell(currentRow, 7).Value = containerRecord.ContainerNo;
            worksheet.Cell(currentRow, 8).Value = containerRecord.IsoCode;
            worksheet.Cell(currentRow, 9).Value = containerRecord.TareWt;
            worksheet.Cell(currentRow, 10).Value = containerRecord.BillOfLading;

            worksheet.Cell(currentRow, 6).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            worksheet.Range(currentRow, 6, currentRow, 10).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Range(currentRow, 6, currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(currentRow, 6, currentRow, 10).Style.Font.FontSize = 10;
            worksheet.Range(currentRow, 6, currentRow, 10).Style.Font.FontName = "Courier New";
            currentRow++;
            index++;
        }

        currentRow += 2;

        worksheet.Cell(currentRow, 1).Value = "C уважением,";
        currentRow++;
        worksheet.Cell(currentRow, 1).Value = "руководитель отдела операций, Новороссийск";

        worksheet.Range(currentRow - 1, 9, currentRow, 10).Merge();
        worksheet.Cell(currentRow - 1, 9).Value = "О.О.Кучеренко";
        worksheet.Cell(currentRow - 1, 9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        worksheet.Cell(currentRow - 1, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        worksheet.Range(currentRow - 1, 1, currentRow, 10).Style.Font.FontSize = 12;
        worksheet.Range(currentRow - 1, 1, currentRow, 10).Style.Font.FontName = "Times New Roman";

        //C уважением, О.О.Кучеренко
        //    руководитель отдела операций, Новороссийск


        // Сохранение в MemoryStream
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

