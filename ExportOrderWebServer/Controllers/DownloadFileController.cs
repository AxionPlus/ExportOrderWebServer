using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExportOrderWebServer.Controllers;

[AllowAnonymous]
[Route("file/[controller]")]
//[ApiController]

public class DownloadFileController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IExportOrderProvider _exportOrderProvider;
    private readonly IExcelFileCreateService _excelFileCreateService;
    private readonly IXmlFileService _xmlFileService;

    public DownloadFileController(IWebHostEnvironment webHostEnvironment,
                                    IExportOrderProvider exportOrderProvider,
                                    IExcelFileCreateService excelFileCreateService,
                                    IXmlFileService xmlFileService)
    {
        _webHostEnvironment = webHostEnvironment;
        _exportOrderProvider = exportOrderProvider;
        _excelFileCreateService = excelFileCreateService;        
        _xmlFileService = xmlFileService;

        //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [HttpGet, Route("SaveFileExcelRolis")]  // file/DownloadFileController/SaveFileExcelRolis
    public async Task<IActionResult> SaveFileRolis(long Id)
    {
        var item = await _exportOrderProvider.GetExportOrderDTOAsync(Id);
        if (item is null)
            return BadRequest("Данные не получены.");

        string fileName = $"Rolis_{item.Voyage}_{item.Num}";

        var buffer = await _excelFileCreateService.CreateExcelFile_Rolis(item);

        if (buffer == Array.Empty<byte>())
            return BadRequest("Файл не записан.");
        else
            return File(buffer, "application/xlsx", $"{fileName}.xlsx");
    }

    [HttpGet, Route("SaveFileExcelFillBill")]   // file/DownloadFileController/SaveFileExcelFillBill
    public async Task<IActionResult> SaveFileFillBill(long vslcallid)
    {
        var items = await _exportOrderProvider.GetVoyageManifestDTOAsync(vslcallid, false);

        if (items is null || !items.Any()) return BadRequest("Данные не получены.");

        string fileName = $"FillBill_{items.FirstOrDefault()!.Voyage}";

        var buffer = await _excelFileCreateService.CreateExcelFile_FillBill(items);

        if (buffer == Array.Empty<byte>())
            return BadRequest("Файл не записан.");
        else
            return File(buffer, "application/xlsx", $"{fileName}.xlsx");
    }

    [HttpGet, Route("SaveFileXML")]     // file/DownloadFileController/SaveFileXML
    public async Task<IActionResult> SaveFileXML(long Id)
    {
        var item = await _exportOrderProvider.GetExportOrderDTOAsync(Id);

        if (item is null) return BadRequest("Данные не получены.");

        string fileName = $"{item.Num}_Customs";
        
        var buffer = await _xmlFileService.CreateXMLfile(item);

        if (buffer == Array.Empty<byte>())
            return BadRequest("Файл не записан.");
        else
            return File(buffer, "application/xml", $"{fileName}.xml");
    }

    [HttpGet, Route("DownloadExcelTemplate")]
    public async Task<IActionResult> DownloadTemplate()
    {
        string fileName = $"ИмпортСписка ДТ (проформа)";

        byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(Path.Combine(_webHostEnvironment.ContentRootPath, "Resources", "TemplateUploadCntrs.xlsx"));

        if (fileBytes == Array.Empty<byte>())
            return BadRequest("Файл не найден.");
        else
            return File(fileBytes, "application/xlsx", $"{fileName}.xlsx");
    }
}
