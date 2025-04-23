using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExportOrderWebServer.Controllers;

[AllowAnonymous]
[Route("file/[controller]")]
//[ApiController]
public class UploadFileController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IExcelFileUploadService _excelUploadService;
        
    public UploadFileController(IWebHostEnvironment webHostEnvironment, IExcelFileUploadService excelUploadService)
    {
        _webHostEnvironment = webHostEnvironment;
        _excelUploadService = excelUploadService;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);        
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

        return await _excelUploadService.ReadImportListDeclarations(filePath);
    }

    #region TO DELETE

    //[HttpPost, Route("SaveBLReportFiles")] // file/DownloadFile/SaveBLReportFiles
    //public async Task<IActionResult> SaveBLReportFiles([FromBody] object obj)
    //{
    //    try
    //    {
    //        IEnumerable<long> Ids = Enumerable.Empty<long>();            

    //        var resultObj = JsonConvert.DeserializeObject<ControllerPassObject<IEnumerable<long>>>(obj.ToString()!);
    //        if (resultObj is not null)
    //        {
    //            if (resultObj.GetObject.Count() > 0)
    //            {
    //                Ids = resultObj.GetObject;
    //            }
    //        }

    //        //ring dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", $"{Voyage}_{Date2Str(DateTime.Now)}");
    //        //string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TempFiles", $"{Voyage}_{DateTime.Now:d}");

    //        //if (!Directory.Exists(dirPath))
    //        //    Directory.CreateDirectory(dirPath);

    //        //string dirTemporary = Path.Combine(DirTemporary, Path.GetRandomFileName());
    //        //if (!Directory.Exists(dirTemporary))
    //        //    Directory.CreateDirectory(dirTemporary);

    //        foreach (var id in Ids)
    //        {
    //            var Item = await _exportOrderProvider.GetBLDTOAsync(id);

    //            string fileName = "BL" + Item.BLtemplate + ".rdlc";

    //            string pathFile = Path.Combine(DirTemporary, $"{Item.Voyage}_{Item.Num}.pdf");

    //            #region LOCAL REPORT CREATE

    //            string mimeType = "";
    //            int pageIndex = new Random().Next(1, 101);
    //            string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", fileName);

    //            LocalReport localReport = new LocalReport(pathReport);

    //            var Items = new List<ExportOrderDTO>() { Item };
    //            var Records = Item.ExportOrderRecordsDTO.ToList();

    //            /// список типов контейнеров
    //            var CntrTypesGroup = Records.Where(r => r.CntrType is not null).GroupBy(r => r.CntrType)
    //                                        .Select(g => new { CntrTypes = string.Join(" ", g.ToList().Count, "*", g.Key)})
    //                                        .ToList();

    //            string CntrTypes = string.Join("\n", CntrTypesGroup.Select(ctg => ctg.CntrTypes));

    //            /// общие данные
    //            var dsItem = Items.Select(x => new
    //            {
    //                x.BLNum,
    //                x.BLDate,
    //                x.BLDateOEL,
    //                x.Shippers,
    //                x.Consignees,
    //                NotifyParties = x.Consignees,
    //                CntrTypes,
    //                x.Commodities,
    //                POLAgent = x.CarrierNameEn,
    //                x.PODAgent,
    //                x.VesselName,
    //                x.Voyage,
    //                x.POLEn,
    //                x.PODEn,
    //                x.PODwithCountryEn,
    //                x.TotalCntrCount,
    //                x.TotalPackages,
    //                x.TotalTareWeight,
    //                x.TotalGrossWeight,
    //                x.TotalGrossNTareWeight,
    //                x.Measurement,
    //            }).ToList();

    //            /// список контейнеров дополненный до 20ти записей на первом листе коносамента
    //            if (Item.BLtemplate == "nca")
    //            {
    //                int recCount = Records.Count();

    //                int upperBound = 20;

    //                if (recCount < 20)
    //                {
    //                    foreach (var record in Records)
    //                    {
    //                        int extraRowsSeal = 0;
    //                        int extraRowsCommodity = 0;

    //                        switch (record.Seal.Length)
    //                        {
    //                            case <= 10:
    //                                break;
    //                            case <= 20:
    //                                extraRowsSeal = 1;
    //                                break;
    //                            case <= 30:
    //                                extraRowsSeal = 2;
    //                                break;
    //                            case <= 40:
    //                                extraRowsSeal = 3;
    //                                break;
    //                            case <= 50:
    //                                extraRowsSeal = 4;
    //                                break;
    //                            case <= 60:
    //                                extraRowsSeal = 5;
    //                                break;
    //                            case <= 70:
    //                                extraRowsSeal = 6;
    //                                break;
    //                            case <= 80:
    //                                extraRowsSeal = 7;
    //                                break;
    //                            case <= 90:
    //                                extraRowsSeal = 8;
    //                                break;
    //                            case <= 100:
    //                                extraRowsSeal = 9;
    //                                break;
    //                            case <= 110:
    //                                extraRowsSeal = 10;
    //                                break;
    //                        }

    //                        switch (record.RecordCommoditiesEn!.Length)
    //                        {
    //                            case <= 35:
    //                                break;
    //                            case <= 70:
    //                                extraRowsCommodity = 1;
    //                                break;
    //                            case <= 105:
    //                                extraRowsCommodity = 2;
    //                                break;
    //                            case <= 140:
    //                                extraRowsCommodity = 3;
    //                                break;
    //                            case <= 175:
    //                                extraRowsCommodity = 4;
    //                                break;
    //                            case <= 210:
    //                                extraRowsCommodity = 5;
    //                                break;
    //                            case <= 245:
    //                                extraRowsCommodity = 6;
    //                                break;
    //                            case <= 280:
    //                                extraRowsCommodity = 7;
    //                                break;
    //                            case <= 315:
    //                                extraRowsCommodity = 8;
    //                                break;
    //                            case <= 350:
    //                                extraRowsCommodity = 9;
    //                                break;
    //                            case <= 385:
    //                                extraRowsCommodity = 10;
    //                                break;
    //                        }

    //                        if (extraRowsSeal >= extraRowsCommodity)
    //                            upperBound -= extraRowsSeal;
    //                        else
    //                            upperBound -= extraRowsCommodity;
    //                    }
    //                }

    //                if (upperBound > 0)
    //                    for (int i = recCount + 1; i <= upperBound; i++)
    //                        Records.Add(new() { Seq = (uint)i, CntrTareWt = null, GrossWt = null });
    //            }

    //            localReport.AddDataSource("dsBL", dsItem);
    //            localReport.AddDataSource("dsCntrRecords", Records);

    //            ReportResult result = localReport.Execute(RenderType.Pdf, pageIndex, null, mimeType);

    //            #endregion

    //            /// сохраняем файл в папку DirPath
    //            using var fileStream = new FileStream(pathFile, FileMode.Create);
    //            //await fileStream.WriteAsync(result.MainStream, 0, result.MainStream.Length);
    //            await fileStream.WriteAsync(result.MainStream.AsMemory(0, result.MainStream.Length));

    //            ///// сохраняем файл в папку DirPath
    //            //await System.IO.File.WriteAllBytesAsync(pathFile, result.MainStream);
    //        }

    //        //string zipName = $"{DirTemporary}.zip";

    //        ZipFile.CreateFromDirectory(DirTemporary, ZipName);

    //        //using (FileStream fileStream = new(ZipName, FileMode.Open))
    //        //{
    //        //    var buffer = new byte[fileStream.Length];

    //        //    await fileStream.ReadAsync(buffer);

    //        //    Dispose(DirTemporary, ZipName);

    //        //    if (buffer != Array.Empty<byte>())
    //        //        return File(buffer, "application/zip", $"{ZipName}");
    //        //}

    //        //byte[] fileBytes = System.IO.File.ReadAllBytes(zipName);
    //        //return File(fileBytes, "application/zip", $"{zipName}");

    //        using FileStream _fileStream = new(ZipName, FileMode.Open);

    //        var buffer = new byte[_fileStream.Length];
    //        await _fileStream.ReadAsync(buffer);
    //        //DeleteTempFiles(DirTemporary, ZipName);

    //        if (buffer != Array.Empty<byte>())
    //            return File(buffer, "application/zip", $"{ZipName}");            
    //        else
    //            return Empty;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.Message);
    //        DeleteTempFiles(DirTemporary, ZipName);
    //        return Empty;
    //    };
    //}
    
    #endregion

}