using AspNetCore.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO.Compression;

namespace ExportOrderWebServer.Areas.ExpOrder.Controller;

[AllowAnonymous]
[Route("file/[controller]")]
[ApiController]

public class UploadFileController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IExportOrderProvider _exportOrderProvider;
    private readonly IExcelFileUploadService _excelUploadService;
        
    public UploadFileController(IWebHostEnvironment webHostEnvironment, IExportOrderProvider exportOrderProvider, IExcelFileUploadService excelUploadService)
    {
        _webHostEnvironment = webHostEnvironment;
        _exportOrderProvider = exportOrderProvider;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _excelUploadService = excelUploadService;
    }

    [HttpGet, Route("SaveFileExcelFillBill")]   // file/UploadFileController/SaveFileExcelFillBill
    public async Task<IActionResult> SaveFileFillBill(long vslcallid)
    {
        try
        {
            var Items = await _exportOrderProvider.GetVoyageManifestDTOAsync(vslcallid, false);

            if (!Items.Any()) return Empty;

            string FileName = $"FillBill_{Items.FirstOrDefault()!.Voyage}";
            string tempFilePath = Path.Combine(_webHostEnvironment.WebRootPath, $"{Path.GetRandomFileName()}.xlsx");

            using (var xls = new ExcelCreateService(tempFilePath, Items, null))
            {
                var buffer = await xls.CreateExcelFile_FillBill();

                if (buffer != Array.Empty<byte>())
                    return File(buffer, "application/xlsx", $"{FileName}.xlsx");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Empty;
        }

        return Ok();
    }

    [HttpGet, Route("SaveFileExcelRolis")]  // file/UploadFileController/SaveFileExcelRolis
    public async Task<IActionResult> SaveFileRolis(long expOrderId)
    {
        try
        {
            var Item = await _exportOrderProvider.GetExportOrderDTOAsync(expOrderId);

            if (Item is null) return Empty;

            string FileName = $"Rolis_{Item.Voyage}_{Item.Num}";

            string tempFilePath = Path.Combine(_webHostEnvironment.WebRootPath, $"{Path.GetRandomFileName()}.xlsx");

            using (var xls = new ExcelCreateService(tempFilePath, null, Item))
            {
                var buffer = await xls.CreateExcelFile_Rolis();

                if (buffer != Array.Empty<byte>())
                    return File(buffer, "application/xlsx", $"{FileName}.xlsx");
            }
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            return Empty;
        }

        return Ok();
    }

    [HttpGet, Route("SaveXMLfile")]     // file/UploadFileController/SaveXMLfile
    public async Task<IActionResult> SaveXMLfile(long Id)
    {
        try
        {
            var Item = await _exportOrderProvider.GetExportOrderDTOAsync(Id);

            await Task.Delay(100);

            if (Item is null) return Empty;

            string FileName = $"{Item.Num}_Customs";
            string tempFilePath = Path.Combine(_webHostEnvironment.WebRootPath, $"{Path.GetRandomFileName()}.xml");

            using (var xml = new XmlService(tempFilePath, Item))
            {
                var buffer = await xml.CreateXMLfile();

                if (buffer != Array.Empty<byte>())
                    return File(buffer, "application/xml", $"{FileName}.xml");
            }
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            Console.WriteLine($"Error: {ex.Message}");
            return Empty;
        }

        return Ok();
    }

    [HttpPost, Route("UploadFromExcel")]    // file/UploadFileController/UploadFromExcel
    public async Task<List<UploadExcelDTO>?> UploadFromExcel([FromForm] IEnumerable<IFormFile> files)
    {
        string filePath = string.Empty;

        foreach (var file in files)
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                filePath = Path.Combine(_webHostEnvironment.WebRootPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                file.CopyTo(stream);
            }

        //using var exl = new ExcelUploadService(filePath);

        //return await exl.ReadUploadingFile();

        return await _excelUploadService.ReadImportListDeclarations(filePath);
    }

    [HttpPost, Route("SaveExportOrderReportFiles")] // file/UploadFile/SaveExportOrderReportFiles
    public async Task<IActionResult> SaveExportOrderReportFiles([FromBody] object obj)
    {
        try
        {
            IEnumerable<long> Ids = Enumerable.Empty<long>();
            string Voyage = string.Empty;

            var resultObj = JsonConvert.DeserializeObject<ControllerPassObject<IEnumerable<long>>>(obj.ToString());
            if (resultObj is not null)
            {
                if (resultObj.GetObject.Count() > 0)
                {
                    Ids = resultObj.GetObject;
                    Voyage = resultObj.Remarks;
                }
            }

            string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", $"{Voyage}_{Date2Str(DateTime.Now)}");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            foreach (var id in Ids)
            {
                var Item = await _exportOrderProvider.GetExportOrderDTOAsync(id);

                string pathFile = Path.Combine(dirPath, $"{Item.Num}.pdf");

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
                using (var fileStream = new FileStream(pathFile, FileMode.Create))
                {
                    await fileStream.WriteAsync(result.MainStream, 0, result.MainStream.Length);
                }
            }

            string zipName = $"{dirPath}.zip";

            ZipFile.CreateFromDirectory(dirPath, zipName);

            using (FileStream fileStream = new FileStream(zipName, FileMode.Open))
            {
                var buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer);
                if (buffer != Array.Empty<byte>())
                    return File(buffer, "application/zip", $"{zipName}");
            }

            return Empty;

        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            Console.WriteLine(msg);
            return Empty;
        };
    }

    [HttpPost, Route("SaveBLReportFiles")] // file/UploadFile/SaveBLReportFiles
    public async Task<IActionResult> SaveBLReportFiles([FromBody] object obj)
    {
        try
        {
            IEnumerable<long> Ids = Enumerable.Empty<long>();
            string Voyage = string.Empty;

            var resultObj = JsonConvert.DeserializeObject<ControllerPassObject<IEnumerable<long>>>(obj.ToString());
            if (resultObj is not null)
            {
                if (resultObj.GetObject.Count() > 0)
                {
                    Ids = resultObj.GetObject;
                    Voyage = resultObj.Remarks;
                }
            }

            string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", $"{Voyage}_{Date2Str(DateTime.Now)}");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            foreach (var id in Ids)
            {
                var Item = await _exportOrderProvider.GetBLDTOAsync(id);

                string fileName = "BL" + Item.BLtemplate + ".rdlc";

                string pathFile = Path.Combine(dirPath, $"{Item.Num}.pdf");

                #region LOCAL REPORT CREATE

                string mimeType = "";
                int pageIndex = new Random().Next(1, 101);
                string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", fileName);

                LocalReport localReport = new LocalReport(pathReport);

                var Items = new List<ExportOrderDTO>() { Item };
                var Records = Item.ExportOrderRecordsDTO.ToList();

                // список типов контейнеров
                var CntrTypesGroup = Records.Where(r => r.CntrType is not null).GroupBy(r => r.CntrType)
                                            .Select(g => new
                                            {
                                                CntrTypes = string.Join(" ", g.ToList().Count, "*", g.Key),
                                            })
                                            .ToList();

                string CntrTypes = string.Join("\n", CntrTypesGroup.Select(ctg => ctg.CntrTypes));

                // общие данные
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

                // список контейнеров дополненный до 20ти записей на первом листе коносамента
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

                            switch (record.Seal.Length)
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

                            switch (record.RecordCommoditiesEn!.Length)
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

                /// сохраняем файл в папку DirPath
                using (var fileStream = new FileStream(pathFile, FileMode.Create))
                {
                    await fileStream.WriteAsync(result.MainStream, 0, result.MainStream.Length);
                }

                ///// сохраняем файл в папку DirPath
                //await System.IO.File.WriteAllBytesAsync(pathFile, result.MainStream);
            }

            string zipName = $"{dirPath}.zip";

            ZipFile.CreateFromDirectory(dirPath, zipName);

            using (FileStream fileStream = new FileStream(zipName, FileMode.Open))
            {
                var buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer);
                if (buffer != Array.Empty<byte>())
                    return File(buffer, "application/zip", $"{zipName}");
            }

            //byte[] fileBytes = System.IO.File.ReadAllBytes(zipName);
            //return File(fileBytes, "application/zip", $"{zipName}");

            return Empty;

        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            Console.WriteLine(msg);
            return Empty;
        };
    }

    Func<DateTime, string> Date2Str = (date) =>
    {
        return date.ToString("yyyy") + date.ToString("MM") + date.ToString("dd") + date.ToString("HH") + date.ToString("mm");
    };       
}