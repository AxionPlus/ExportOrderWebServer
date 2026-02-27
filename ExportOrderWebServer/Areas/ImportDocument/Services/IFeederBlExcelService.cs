using ClosedXML.Excel;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;
using System.Reflection;

namespace ExportOrderWebServer.Areas.ImportDocument.Services;

public interface IFeederBlExcelService
{
    Task<byte[]> GenerateFeederBlAsync(VesselCallDto vesselCall);

    Task<string> GenerateFeederBlFileAsync(VesselCallDto vesselCall, string filePath);
}
public class FeederBlExcelService : IFeederBlExcelService
{

    private readonly string _templatePath;

    public FeederBlExcelService()
    {
        _templatePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Areas\\ImportDocument\\Resources\\FeederBL_Template.xlsx";

#if  DEBUG
        _templatePath =
            "C:\\Users\\Sergey Vasilenko\\source\\repos\\AxionPlus\\ExportOrderWebServer\\ExportOrderWebServer\\Areas\\ImportDocument\\Resources\\FeederBL_Template.xlsx";
#endif
    }
    public async Task<byte[]> GenerateFeederBlAsync(VesselCallDto vesselCall)
    {
        if (!File.Exists(_templatePath))
        {
            throw new FileNotFoundException($"Шаблон не найден: {_templatePath}");
        }

        var decimalFormat = "# ### ##0.000_-; # ### ##0,000_-;_-* \"-\"??_-;_-@_-";
        var intFormat = "# ### ##0_-;# ### ##0_-;_-* \"-\"??_-;_-@_-";

        var vesselName = vesselCall.Vessel.Name;
        var voyageNumber = vesselCall.VoyageNo;
        var feederBl = vesselCall.FeederBlNo;
        var arrivalDate = vesselCall.ETA;
        var vesselFlag = vesselCall.Vessel.FlagRu;
        var portOfLoading = vesselCall.PortOfLoading.FullEn;
        var portOfLoadingDate = vesselCall.PortOfLoadingDate;


        using var workbook = new XLWorkbook(_templatePath);

        var worksheet = workbook.Worksheet(1); // Первый лист


        worksheet.Cell(4, 8).Value = feederBl;
        worksheet.Cell(5, 2).Value = portOfLoading;
        worksheet.Cell(26, 1).Value = $"{vesselName} / {voyageNumber}";
        worksheet.Cell(26, 4).Value = portOfLoading;
        worksheet.Cell(63, 7).Value = portOfLoadingDate;


        worksheet.Cell(33, 4).Value = vesselCall.QuantityEmpty20;
        worksheet.Cell(34, 4).Value = vesselCall.QuantityEmpty40;
        worksheet.Cell(35, 4).Value = 0; //    X 45` EMPTY
        worksheet.Cell(36, 4).Value = vesselCall.QuantityFull20;
        worksheet.Cell(37, 4).Value = vesselCall.QuantityFull40;
        worksheet.Cell(38, 4).Value = 0; //    X 45` FULL


        worksheet.Cell(39, 9).Value = vesselCall.GrossWtFull;



        worksheet = workbook.Worksheet(2); // Второй лист



        worksheet.Cell(1, 1).Value = $"Attached List to Bill of Lading No {feederBl}";


        var ContainerRecords = vesselCall.BillOfLadings.SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).OrderBy(s => s.Key);

        int currentRow = 5;

        foreach (var containerRecord in ContainerRecords)
        {
            worksheet.Cell(currentRow, 1).Value = containerRecord.Key;
            worksheet.Cell(currentRow + 1, 1).Value = containerRecord.First().SealNo;

            worksheet.Cell(currentRow, 2).Value = $"{containerRecord.First().ContainerTypeId} x 01 // {containerRecord.Sum(s => s.NoOfPackage)} {containerRecord.First().PackageType}";
            worksheet.Cell(currentRow + 1, 2).Value = "GENERAL CARGO";

            worksheet.Cell(currentRow, 3).Value = containerRecord.First().TareWt;
            worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = intFormat;

            worksheet.Cell(currentRow, 4).Value = containerRecord.Sum(s => s.GrossWeight);
            worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = decimalFormat;


            worksheet.Range(currentRow, 1, currentRow + 1, 4).Style.Font.FontSize = 10;
            worksheet.Range(currentRow, 1, currentRow + 1, 4).Style.Font.FontName = "Courier New";
            worksheet.Cell(currentRow + 1, 1).Style.Font.FontSize = 8;

            worksheet.Range(currentRow, 1, currentRow + 1, 2).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Left;
            worksheet.Range(currentRow, 3, currentRow + 1, 4).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            worksheet.Row(currentRow).Height = 12;
            worksheet.Row(currentRow + 1).Height = 12;
            currentRow += 2;
        }

        currentRow += 4;

        worksheet.Cell(currentRow, 2).Value = "TOTAL Container Tare Weight:";
        worksheet.Cell(currentRow, 3).Value = vesselCall.TareWtContainers;
        worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = intFormat;
        worksheet.Range(currentRow, 3, currentRow, 4).Merge();

        currentRow++;
        worksheet.Cell(currentRow, 2).Value = "TOTAL Cargo Weight:";
        worksheet.Cell(currentRow, 3).Value = vesselCall.GrossWtFull;
        worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = decimalFormat;
        worksheet.Range(currentRow, 3, currentRow, 4).Merge();

        currentRow++;
        worksheet.Cell(currentRow, 2).Value = "TOTAL:";
        worksheet.Cell(currentRow, 3).Value = vesselCall.TareWtContainers + vesselCall.GrossWtFull;
        worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = decimalFormat;
        worksheet.Range(currentRow, 3, currentRow, 4).Merge();



        worksheet.Range(currentRow - 2, 2, currentRow, 4).Style.Font.FontSize = 12;
        worksheet.Range(currentRow - 2, 2, currentRow, 4).Style.Font.Bold = true;
        worksheet.Range(currentRow - 2, 2, currentRow, 4).Style.Font.FontName = "Courier New";

        worksheet.Range(currentRow - 2, 2, currentRow, 2).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Right;
        worksheet.Range(currentRow - 2, 2, currentRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        worksheet.Range(currentRow - 2, 3, currentRow, 4).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;
        worksheet.Range(currentRow - 2, 3, currentRow, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


        // Настройка нижнего колонтитула
        var footer = worksheet.PageSetup.Footer;

        // Очистить существующий колонтитул
        footer.Clear();

        // Вариант 1: Номер страницы по центру
        worksheet.PageSetup.Footer.Left.AddText($"{vesselName} / {voyageNumber}");
        worksheet.PageSetup.Footer.Right.AddText("&P - &N");

        // Сохранение в MemoryStream
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public Task<string> GenerateFeederBlFileAsync(VesselCallDto vesselCall, string filePath)
    {
        throw new NotImplementedException();
    }
}

