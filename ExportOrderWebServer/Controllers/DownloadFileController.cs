using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Xml.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Graphics;

namespace ExportOrderWebServer.Controllers;

[AllowAnonymous]
[Route("file/[controller]")]
//[ApiController]
public class DownloadFileController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IExportOrderProvider _exportOrderProvider;
    private readonly static string DirTemporary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TempFiles");

    public DownloadFileController(IWebHostEnvironment webHostEnvironment, IExportOrderProvider exportOrderProvider)
    {
        _webHostEnvironment = webHostEnvironment;
        _exportOrderProvider = exportOrderProvider;
    }

    [HttpGet, Route("SaveFileExcelRolis")]  // file/DownloadFileController/SaveFileExcelRolis
    public async Task<IActionResult> SaveFileRolis(long Id)
    {
        var ids = new List<long>() { Id };

        var items = await _exportOrderProvider.GetItemsExportOrderDTOAsync(ids);

        if (items is null) return BadRequest("Данные не получены.");

        var Item = items.FirstOrDefault();

        if (Item is null) return BadRequest("Данные не получены.");

        string fileName = $"Rolis_{Item.Voyage}_{Item.Num}";

        using IExcelFileCreateService _excelFileCreateService = new ExcelFileCreateService();
        var buffer = await _excelFileCreateService.CreateExcelFile_Rolis(Item);

        if (buffer == Array.Empty<byte>())
            return BadRequest("Файл не записан.");
        else
            return File(buffer, "application/xlsx", $"{fileName}.xlsx");
    }
    
    [HttpGet, Route("SaveFileXML")]     // file/DownloadFileController/SaveFileXML
    public async Task<IActionResult> SaveFileXML(long Id)
    {
        var ids = new List<long>() { Id };
        var items = await _exportOrderProvider.GetItemsExportOrderDTOAsync(ids);   //var item = await _exportOrderProvider.GetExportOrderDTOAsync(Id);

        if (items is null) return BadRequest("Данные не получены.");

        var Item = items.FirstOrDefault();

        if (Item is null) return BadRequest("Данные не получены.");

        string fileName = $"Customs_{Item.Num}";

        using IXmlFileCreateService _xmlFileService = new XmlFileCreateService();
        var buffer = await _xmlFileService.CreateXMLfileExportOrder(Item);

        if (buffer == Array.Empty<byte>())
            return BadRequest("Файл не записан.");
        else
            return File(buffer, "application/xml", $"{fileName}.xml");
    }

    [HttpGet, Route("SaveFileExcelFillBill")]   // file/DownloadFileController/SaveFileExcelFillBill
    public async Task<IActionResult> SaveFileFillBill(long vslcallid)
    {
        var items = await _exportOrderProvider.GetItemsManifestDTOAsync(vslcallid, false);

        if (items is null || !items.Any()) return BadRequest("Данные не получены.");

        string fileName = $"FillBill_{items.FirstOrDefault()!.Voyage}";

        using IExcelFileCreateService _excelFileCreateService = new ExcelFileCreateService();
        var buffer = await _excelFileCreateService.CreateExcelFile_FillBill(items);

        if (buffer == Array.Empty<byte>())
            return BadRequest("Файл не записан.");
        else
            return File(buffer, "application/xlsx", $"{fileName}.xlsx");
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

    [HttpGet, Route("UploadPdfAndSaveToExcel")]   //Upload Pdf then Convert To Excel File
    public async Task<IActionResult> SaveExcelCustomsReport(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return BadRequest("File not found.");

        using (IExcelFileCreateService _excelCreateService = new ExcelFileCreateService())
        {
            var buffer = await _excelCreateService.CreateExcelFromPdf(filePath);

            if (buffer == Array.Empty<byte>())
                return BadRequest("Файл не записан.");
            else
                return File(buffer, "application/xlsx", "CustomsReport.xlsx");
        }
    }
    //-----------------------------------------------------------------------    

    [HttpPost, Route("SaveFilesPdfOrder")]
    public async Task<IActionResult> SaveFilesPdfOrder([FromBody] IEnumerable<long> ids)
    {
        if (ids is null || !ids.Any())
            return BadRequest("List of selected records is empty.");

        var Items = await _exportOrderProvider.GetItemsExportOrderDTOAsync(ids);

        if (Items is null || !Items.Any())
            return BadRequest("Records not found");

        using IPdfFileCreateService _pdfFileCreateService = new PdfFileCreateService();
        var buffer = await _pdfFileCreateService.CreatePdfOrders(Items);

        if (buffer == Array.Empty<byte>())
            return BadRequest("Файл не записан.");
        else
            return File(buffer, "application/zip");
    }

    [HttpPost, Route("SaveFilesPdfBill")]
    public async Task<IActionResult> SaveFilesPdfBill([FromBody] IEnumerable<long> ids)
    {
        if (ids is null || !ids.Any())
            return BadRequest("List of selected records is empty.");

        var Items = await _exportOrderProvider.GetItemsBillDTOAsync(ids);

        if (Items is null || !Items.Any())
            return BadRequest("Records not found");

        using IPdfFileCreateService _pdfFileCreateService = new PdfFileCreateService();
        var buffer = await _pdfFileCreateService.CreatePdfBills(Items);

        if (buffer == Array.Empty<byte>())
            return BadRequest("Файл не записан.");
        else
            return File(buffer, "application/zip"); //, $"{fileName}"
    }

    [HttpPost, Route("GetExcelReport")]
    public async Task<IActionResult> GetExcelReport([FromBody] object obj)
    {
        var array= Newtonsoft.Json.JsonConvert.DeserializeObject<object[,]>(obj.ToString());

        using IExcelFileCreateService _excelFileCreateService = new ExcelFileCreateService();
        var buffer = await _excelFileCreateService.CreateExcelReport( array, $"Release list_{DateTime.Now}");

        if (buffer == Array.Empty<byte>())
            return BadRequest("Файл не записан.");
        else
            return File(buffer, "application/zip");
    }

    //[HttpPost, Route("SaveExcelCustomsReport")]   //ConvertPdfToExcelReport
    //public async Task<IActionResult> SaveExcelCustomsReport([FromBody] string dirPath)
    //{
    //    if (string.IsNullOrEmpty(dirPath)) return BadRequest("Files list is empty.");
        
    //    if (!Directory.Exists(dirPath)) return BadRequest("Directory dosn't exists.");

    //    using (IExcelFileCreateService _excelCreateService = new ExcelFileCreateService())
    //    {
    //        var buffer = await _excelCreateService.CreateExcelCustomsReport(dirPath);

    //        if (buffer == Array.Empty<byte>())
    //            return BadRequest("Файл не записан.");
    //        else
    //            return File(buffer, "application/xlsx", "CustomsReport.xlsx");
    //    }        
    //}
}
