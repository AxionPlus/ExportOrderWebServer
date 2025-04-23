using AspNetCore.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace ExportOrderWebServer.Controllers;

[AllowAnonymous]
[Route("file/[controller]")]
//[ApiController]
public class DownloadFileController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IExportOrderProvider _exportOrderProvider;
    private readonly IExcelFileCreateService _excelFileCreateService;
    private readonly IXmlFileCreateService _xmlFileService;

    private static readonly string DirTemporary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TempFiles", Path.GetRandomFileName());
    private static readonly string ZipName = $"{DirTemporary}.zip";

    public DownloadFileController(IWebHostEnvironment webHostEnvironment,
                                    IExportOrderProvider exportOrderProvider,
                                    IExcelFileCreateService excelFileCreateService,
                                    IXmlFileCreateService xmlFileService)
    {
        _webHostEnvironment = webHostEnvironment;
        _exportOrderProvider = exportOrderProvider;
        _excelFileCreateService = excelFileCreateService;        
        _xmlFileService = xmlFileService;

        if (!Directory.Exists(DirTemporary))
            Directory.CreateDirectory(DirTemporary);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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

    //-----------------------------------------------------------------------    

    [HttpPost, Route("SaveExportOrderReportFiles")] // file/UploadFile/SaveExportOrderReportFiles
    public async Task<IActionResult> SaveExportOrderReportFiles([FromBody] IEnumerable<long> ids)
    {
        try
        {
            if (ids is null || !ids.Any())
                return BadRequest("List of selected records is empty.");

            foreach (var id in ids)
            {
                var Item = await _exportOrderProvider.GetExportOrderDTOAsync(id);

                string pathFile = Path.Combine(DirTemporary, $"Order_{Item.Num}.pdf");

                #region LOCAL REPORT CREATE

                string mimeType = "application/pdf";
                int pageIndex = new Random().Next(1, 101);
                string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "ExportOrder.rdlc");

                LocalReport localReport = new LocalReport(pathReport);

                var Items = new List<ExportOrderDTO>() { Item };

                var dsItem = Items.Select(x => new
                {
                    x.Num,
                    x.BLNum,
                    x.Dated,
                    Vessel = x.VesselName + " (" + x.VesselFlag + ")",
                    x.Voyage,
                    x.DateOfLoading,
                    x.POL,
                    x.PODwithCountryRus,
                    x.Contract,
                    x.ContractDate,
                    x.Shippers,
                    x.Consignees,
                    x.Commodities,
                    x.CommodityShort,
                    x.MyCompanyName,
                    x.Person
                });

                var dsRecords = Item.ExportOrderRecordsDTO;

                int dSeq = 0;
                var dsDocuments = dsRecords?.GroupBy(r => r.DocumentName).Select(g => new {
                    docSeq = ++dSeq,
                    Document = g.Key,
                    Pakages = g.Sum(q => q.PackageQty),
                    CntrTare = g.Sum(tr => tr.CntrTareWt),
                    docNet = g.Sum(net => net.NetWt),
                    docGross = g.Sum(gr => gr.GrossWt)
                }).ToList();

                localReport.AddDataSource("dsItem", dsItem);
                localReport.AddDataSource("dsRecords", dsRecords);
                localReport.AddDataSource("dsDocuments", dsDocuments);

                ReportResult result = localReport.Execute(RenderType.Pdf, pageIndex, null, mimeType);

                #endregion

                /// сохраняем файл в папку DirPath
                using var fileStream = new FileStream(pathFile, FileMode.Create);
                await fileStream.WriteAsync(result.MainStream.AsMemory(0, result.MainStream.Length));
            }

            ZipFile.CreateFromDirectory(DirTemporary, ZipName);

            var buffer = Array.Empty<byte>();

            using (FileStream fileStream = new(ZipName, FileMode.Open))
            {
                buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer);                
            }

            DeleteTempFiles();

            if (buffer != Array.Empty<byte>())
                return File(buffer, "application/zip", $"{ZipName}");
            else
                return Empty;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            DeleteTempFiles();
            return Empty;
        };
    }

    [HttpPost, Route("SaveBLReportFiles")] // file/DownloadFile/SaveBLReportFiles
    public async Task<IActionResult> SaveBLReportFiles([FromBody] IEnumerable<long> ids)
    {
        try
        {
            if (ids is null || !ids.Any())
                return BadRequest("List of selected records is empty.");

            foreach (var id in ids)
            {
                var Item = await _exportOrderProvider.GetBLDTOAsync(id);

                string fileName = "BL" + Item.BLtemplate + ".rdlc";
                string pathFile = Path.Combine(DirTemporary, $"{Item.Voyage}_{Item.Num}.pdf");

                #region LOCAL REPORT CREATE

                string mimeType = "";
                int pageIndex = new Random().Next(1, 101);
                string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", fileName);

                LocalReport localReport = new LocalReport(pathReport);

                var Items = new List<ExportOrderDTO>() { Item };
                var Records = Item.ExportOrderRecordsDTO.ToList();

                /// список типов контейнеров
                var CntrTypesGroup = Records.Where(r => r.CntrType is not null).GroupBy(r => r.CntrType)
                                            .Select(g => new { CntrTypes = string.Join(" ", g.ToList().Count, "*", g.Key) })
                                            .ToList();

                string CntrTypes = string.Join("\n", CntrTypesGroup.Select(ctg => ctg.CntrTypes));

                /// общие данные
                var dsItem = Items.Select(x => new
                {
                    x.BLNum,
                    x.BLDate,
                    x.BLDateOEL,
                    x.Shippers,
                    x.Consignees,
                    NotifyParties = x.Consignees,
                    CntrTypes,
                    x.Commodities,
                    POLAgent = x.CarrierNameEn,
                    x.PODAgent,
                    x.VesselName,
                    x.Voyage,
                    x.POLEn,
                    x.PODEn,
                    x.PODwithCountryEn,
                    x.TotalCntrCount,
                    x.TotalPackages,
                    x.TotalTareWeight,
                    x.TotalGrossWeight,
                    x.TotalGrossNTareWeight,
                    x.Measurement,
                }).ToList();

                /// список контейнеров дополненный до 20ти записей на первом листе коносамента
                if (Item.BLtemplate == "nca")
                {
                    int recCount = Records.Count();

                    int upperBound = 20;

                    if (recCount < 20)
                    {
                        foreach (var record in Records)
                        {
                            int extraRowsSeal = 0;
                            int extraRowsCommodity = 0;

                            switch (record.Seal?.Length)
                            {
                                case <= 10:
                                    break;
                                case <= 20:
                                    extraRowsSeal = 1;
                                    break;
                                case <= 30:
                                    extraRowsSeal = 2;
                                    break;
                                case <= 40:
                                    extraRowsSeal = 3;
                                    break;
                                case <= 50:
                                    extraRowsSeal = 4;
                                    break;
                                case <= 60:
                                    extraRowsSeal = 5;
                                    break;
                                case <= 70:
                                    extraRowsSeal = 6;
                                    break;
                                case <= 80:
                                    extraRowsSeal = 7;
                                    break;
                                case <= 90:
                                    extraRowsSeal = 8;
                                    break;
                                case <= 100:
                                    extraRowsSeal = 9;
                                    break;
                                case <= 110:
                                    extraRowsSeal = 10;
                                    break;
                            }

                            switch (record.RecordCommoditiesEn?.Length)
                            {
                                case <= 35:
                                    break;
                                case <= 70:
                                    extraRowsCommodity = 1;
                                    break;
                                case <= 105:
                                    extraRowsCommodity = 2;
                                    break;
                                case <= 140:
                                    extraRowsCommodity = 3;
                                    break;
                                case <= 175:
                                    extraRowsCommodity = 4;
                                    break;
                                case <= 210:
                                    extraRowsCommodity = 5;
                                    break;
                                case <= 245:
                                    extraRowsCommodity = 6;
                                    break;
                                case <= 280:
                                    extraRowsCommodity = 7;
                                    break;
                                case <= 315:
                                    extraRowsCommodity = 8;
                                    break;
                                case <= 350:
                                    extraRowsCommodity = 9;
                                    break;
                                case <= 385:
                                    extraRowsCommodity = 10;
                                    break;
                            }

                            if (extraRowsSeal >= extraRowsCommodity)
                                upperBound -= extraRowsSeal;
                            else
                                upperBound -= extraRowsCommodity;
                        }
                    }

                    if (upperBound > 0)
                        for (int i = recCount + 1; i <= upperBound; i++)
                            Records.Add(new() { Seq = (uint)i, CntrTareWt = null, GrossWt = null });
                }

                localReport.AddDataSource("dsBL", dsItem);
                localReport.AddDataSource("dsCntrRecords", Records);

                ReportResult result = localReport.Execute(RenderType.Pdf, pageIndex, null, mimeType);

                #endregion

                /// сохраняем файл в папку DirTemporary
                using FileStream fileStream = new(pathFile, FileMode.Create);
                //await fileStream.WriteAsync(result.MainStream, 0, result.MainStream.Length);
                await fileStream.WriteAsync(result.MainStream.AsMemory(0, result.MainStream.Length));
            }

            ZipFile.CreateFromDirectory(DirTemporary, ZipName);

            using FileStream _fileStream = new(ZipName, FileMode.Open);

            var buffer = new byte[_fileStream.Length];
            await _fileStream.ReadAsync(buffer);

            DeleteTempFiles();

            if (buffer != Array.Empty<byte>())
                return File(buffer, "application/zip", $"{ZipName}");
            else
                return Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            DeleteTempFiles();
            return Empty;
        };
    }

    public void DeleteTempFiles()
    {
        if (Directory.Exists(DirTemporary))
            Directory.Delete(DirTemporary, true);

        //if (System.IO.File.Exists(ZipName))       ZipName - deleting upon FileStream completion
        //    System.IO.File.Delete(ZipName);
    }
}
