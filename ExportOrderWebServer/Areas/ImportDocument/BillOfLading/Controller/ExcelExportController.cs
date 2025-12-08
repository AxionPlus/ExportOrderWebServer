using ClosedXML.Excel;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Services;
using ExportOrderWebServer.Areas.ImportDocument.Services;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



namespace ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Controller;

[ApiController]
[Route("api/excel-export")]
public class ExcelExportController : ControllerBase
{
    private readonly IExcelExportService _excelService;
    private readonly IArrivalNoticeExportService _arrivalNoticeExportService;
    private readonly IBillOfLadingService _billOfLadingService;
    private readonly IVesselCallService _vesselCallService;

    public ExcelExportController(
        IExcelExportService excelService,
        IBillOfLadingService billOfLadingService,
        IVesselCallService vesselCallService,
        IArrivalNoticeExportService arrivalNoticeExportService)
    {
        _excelService = excelService;
        _billOfLadingService = billOfLadingService;
        _vesselCallService = vesselCallService;
        _arrivalNoticeExportService = arrivalNoticeExportService;
    }

    [HttpGet("bill-of-lading/list/{id}")]
    public async Task<IActionResult> ExportBillOfLadingList(Guid id)
    {
        try
        {
            // Получаем данные по ID
            var bolList = new List<BillOfLadingBaseDto>();

            var bol = await _billOfLadingService.GetByVesselCallIdAsync(id);
            bolList = bol.ToList();

            if (!bolList.Any())
                return NotFound("Коносаменты не найдены");

            var excelBytes = await _excelService.ExportBillOfLadingToExcelAsync(bolList);

            var fileName = $"Коносаменты_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при экспорте: {ex.Message}");
        }
    }
    [HttpGet("bill-of-lading/arrivalNotice/{id}")]
    public async Task<IActionResult> ExportArrivalNotice(Guid id)
    {
        try
        {

            var vesselCall = await _vesselCallService.GetByIdForArrivalNoticeAsync(id);

            if (!vesselCall.BillOfLadings.Any())
                return NotFound("Коносаменты не найдены");

            var excelBytes = await _arrivalNoticeExportService.GenerateArrivalNoticeAsync(vesselCall);

            var fileName = $"ArrivalNotice_{vesselCall.Vessel.Name}_{vesselCall.VoyageNo}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при экспорте: {ex.Message}");
        }
    }

    [HttpGet("template")]
    public async Task<IActionResult> DownloadTemplate()
    {
        try
        {
            // Создаем пустой шаблон
            var template = CreateEmptyTemplate();

            var fileName = $"Шаблон_коносамента_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(template,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при создании шаблона: {ex.Message}");
        }
    }

    private byte[] CreateEmptyTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Шаблон заполнения");

        // Заголовок
        worksheet.Cell(1, 1).Value = "Шаблон для заполнения данных коносамента";
        worksheet.Range(1, 1, 1, 5).Merge();

        // Заголовки полей
        var fields = new[]
        {
            "Номер коносамента",
            "Дата коносамента (дд.ММ.гггг)",
            "Код клиента",
            "Грузоотправитель",
            "Грузополучатель",
            "Порт погрузки",
            "Порт выгрузки",
            "Описание груза"
        };

        for (int i = 0; i < fields.Length; i++)
        {
            worksheet.Cell(i + 3, 1).Value = fields[i];
            worksheet.Cell(i + 3, 2).Style.Fill.BackgroundColor = XLColor.LightYellow;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

public class ExportRequest
{
    public List<Guid> Ids { get; set; } = new();
}