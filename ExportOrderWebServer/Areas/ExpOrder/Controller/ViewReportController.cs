using AspNetCore.Reporting;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ExportOrderWebServer.Areas.ExpOrder.Controller;

//[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

[Route("view/[controller]")]
[Controller]
public class ViewReportController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IExportOrderProvider _exportOrderProvider;

    public ViewReportController(IWebHostEnvironment webHostEnvironment, IExportOrderProvider exportOrderProvider)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        _webHostEnvironment = webHostEnvironment;
        _exportOrderProvider = exportOrderProvider;
    }

    [HttpGet]
    [Route("ViewReportExpOrder")]
    public async Task<IActionResult> ExportOrderReport(long Id)
    {
        var Item = await _exportOrderProvider.GetExportOrderDTOAsync(Id);

        try
        {
            string mimeType = "";
            int extension = 1;
            string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "ExportOrder.rdlc");

            LocalReport localReport = new LocalReport(pathReport);

            #region PARAMETERS
            //Dictionary<string, string> parameters = new Dictionary<string, string>()
            //{
            //    { "report", "new" },
            //};
            #endregion

            #region DATA SOURCE

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
                x.MyCompanyName,
                x.Person
            });

            var dsRecords = Item.exportOrderRecordsDTO;

            var dsShippers = dsRecords?.Select(r => new { Shippers = r.Shipper }).Distinct().ToList();
            var dsConsignees = dsRecords?.Select(r => new { Consignees = r.ConsigneeEn }).Distinct().ToList();

            var dsCommodities = dsRecords?.GroupBy(r => new { r.SeqContent, r.Commodity })
                              .Select(g => new {
                                  Commodity = g.Key.Commodity + " (" +
                                  g.FirstOrDefault()!.HSCode + ") " +
                                  (g.FirstOrDefault()!.IsIMO ? " IMO: " + g.FirstOrDefault()!.IMO + " UNNO: " + g.FirstOrDefault()!.UNNO : ""),
                              }).ToList();

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
            localReport.AddDataSource("dsShippers", dsShippers);
            localReport.AddDataSource("dsConsignees", dsConsignees);
            localReport.AddDataSource("dsCommodities", dsCommodities);
            localReport.AddDataSource("dsDocuments", dsDocuments);

            #endregion

            ReportResult result = localReport.Execute(RenderType.Pdf, extension, null, mimeType);

            return File(result.MainStream, "application/pdf");
        }
        catch (Exception ex)
        {
            string message = ex.Message;
            return Ok();
        }
    }

    [HttpGet]
    [Route("ViewReportBL")]
    public async Task<IActionResult> BLReport(long Id)
    {
        var Item = await _exportOrderProvider.GetBLDTOAsync(Id);

        string fileName = "BL" + Item.BLtemplate + ".rdlc";

        try
        {
            string mimeType = "";
            int extension = (int)(DateTime.Now.Ticks >> 10);    //int extension = 1;
            string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", fileName);

            LocalReport localReport = new LocalReport(pathReport);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding.GetEncoding("windows-1252");

            #region DATA SOURCE

            var Items = new List<ExportOrderDTO>() { Item };
            var Records = Item.exportOrderRecordsDTO.ToList();

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

            #endregion

            ReportResult result = localReport.Execute(RenderType.Pdf, extension, null, mimeType);

            return File(result.MainStream, "application/pdf");

            //if (result.TotalPages != 0)
            //    return File(result.MainStream, "application/pdf");
            //else
            //    return Empty;

        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            Console.WriteLine(msg);
            return Ok();
        }
    }

    [HttpGet]
    [Route("ViewReportManifest")]
    public async Task<IActionResult> ManifestReport(long vslcallid, bool isIMO)
    {
        var Items = await _exportOrderProvider.GetVoyageManifestDTOAsync(vslcallid, isIMO);

        if (Items.Count() == 0) return Empty;

        try
        {
            string mimeType = "";
            int extension = (int)(DateTime.Now.Ticks >> 10);    //int extension = 1;            
            string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "Manifest.rdlc");

            if (isIMO)
            {
                pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "ManifestIMO.rdlc");
                Items = Items.Where(s => s.IsIMO == true);
            }                

            LocalReport localReport = new LocalReport(pathReport);

            #region DATA SOURCES

            int full20Count = Items.Where(it => it.GrossWeights > 0)
                                   .Where(it => it.CntrType!.Substring(0, 2) == "20")
                                   .GroupBy(it => it.Cntr).Count();
            int full40Count = Items.Where(it => it.GrossWeights > 0)
                                   .Where(it => it.CntrType!.Substring(0, 2) == "40")
                                   .GroupBy(it => it.Cntr).Count();
            int full45Count = Items.Where(it => it.GrossWeights > 0)
                                   .Where(it => it.CntrType!.Substring(0, 2) == "45")
                                   .GroupBy(it => it.Cntr).Count();

            int empty20Count = Items.Where(it => it.GrossWeights is null || it.GrossWeights == 0)
                                    .Where(it => it.CntrType!.Substring(0, 2) == "20")
                                    .GroupBy(it => it.Cntr).Count();
            int empty40Count = Items.Where(it => it.GrossWeights is null || it.GrossWeights == 0)
                                    .Where(it => it.CntrType!.Substring(0, 2) == "40")
                                    .GroupBy(it => it.Cntr).Count();
            int empty45Count = Items.Where(it => it.GrossWeights is null || it.GrossWeights == 0)
                                    .Where(it => it.CntrType!.Substring(0, 2) == "45")
                                    .GroupBy(it => it.Cntr).Count();

            // Sum weights of Full
            double full20weight = Items.Where(it => it.GrossWeights is not null && it.CntrType!.Substring(0, 2) == "20").Sum(c => (double)c.GrossWeights!);
            double full40weight = Items.Where(it => it.GrossWeights is not null && it.CntrType!.Substring(0, 2) == "40").Sum(c => (double)c.GrossWeights!);
            double full45weight = Items.Where(it => it.GrossWeights is not null && it.CntrType!.Substring(0, 2) == "45").Sum(c => (double)c.GrossWeights!);

            // Sum Tare

            double full20Tare = Items.Where(it => it.GrossWeights > 0).Where(it => it.CntrType!.Substring(0, 2) == "20").Sum(c => (double)c.CntrTareWt!);
            double full40Tare = Items.Where(it => it.GrossWeights > 0).Where(it => it.CntrType!.Substring(0, 2) == "40").Sum(c => (double)c.CntrTareWt!);
            double full45Tare = Items.Where(it => it.GrossWeights > 0).Where(it => it.CntrType!.Substring(0, 2) == "45").Sum(c => (double)c.CntrTareWt!);

            double empty20Tare = Items.Where(it => it.GrossWeights is null || it.GrossWeights == 0)
                                   .Where(it => it.CntrType!.Substring(0, 2) == "20")
                                   .Sum(c => (double)c.CntrTareWt!);
            double empty40Tare = Items.Where(it => it.GrossWeights is null || it.GrossWeights == 0)
                                   .Where(it => it.CntrType!.Substring(0, 2) == "40")
                                   .Sum(c => (double)c.CntrTareWt!);
            double empty45Tare = Items.Where(it => it.GrossWeights is null || it.GrossWeights == 0)
                                   .Where(it => it.CntrType!.Substring(0, 2) == "45")
                                   .Sum(c => (double)c.CntrTareWt!);

            // Total
            var total20 = full20weight + full20Tare;
            var total40 = full40weight + full40Tare;
            var total45 = full45weight + full45Tare;

            var totalCntrs = full20Count + full40Count + full45Count + empty20Count + empty40Count + empty45Count;
            var totalWeight = full20weight + full40weight + full45weight;
            var totalTare = Items.Sum(it => it.CntrTareWt);
            var total = totalWeight + totalTare;

            var dsSummary = new List<ManifestSummaryDTO>()
            {
                new ManifestSummaryDTO()
                {
                    Full20Count = full20Count,
                    Full40Count = full40Count,
                    Full45Count = full45Count,
                    Empty20Count = empty20Count,
                    Empty40Count = empty40Count,
                    Empty45Count = empty45Count,

                    Full20Weight = full20weight,
                    Full40Weight = full40weight,
                    Full45Weight = full45weight,

                    Full20Tare = full20Tare,
                    Full40Tare = full40Tare,
                    Full45Tare = full45Tare,
                    Empty20Tare = empty20Tare,
                    Empty40Tare = empty40Tare,
                    Empty45Tare = empty45Tare,

                    Total20 = total20,
                    Total40 = total40,
                    Total45 = total45,
                    TotalCntrs = totalCntrs,
                    TotalWeight = totalWeight,
                    TotalTare = totalTare,
                    Total = total,
                }
            };

            #endregion

            localReport.AddDataSource("dsManifest", Items);
            localReport.AddDataSource("dsSummary", dsSummary);

            ReportResult result = localReport.Execute(RenderType.Pdf, extension, null, mimeType);
            //parameters
            //await Task.Delay(500);

            return File(result.MainStream, "application/pdf");
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            Console.WriteLine(msg);
            return Ok();
        }
    }

    [HttpGet]
    [Route("ViewCustomsExplanation")]
    public async Task<IActionResult> CustomsReport(long vslcallid)
    {
        var Items = await _exportOrderProvider.GetVoyageManifestDTOAsync(vslcallid, false);

        if (Items.Count() == 0) return Empty;

        try
        {
            string mimeType = "";
            int extension = (int)(DateTime.Now.Ticks >> 10);    //int extension = 1;
            string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "CustomsLetter.rdlc");

            LocalReport localReport = new LocalReport(pathReport);

            #region DATA SOURCES

            // Customs
            var dsCustomsDept = Items.GroupBy(g => g.VesselCallId).FirstOrDefault()!
                                .Select(g => new
                                {
                                    g.CustomsOfficeName,
                                    g.CustomsOfficeShortName,
                                    g.CustomsDapartment,
                                    g.DateExplanation,
                                    VesselVoyage = g.VesselName + ", флаг " + g.VesselFlag + ", рейс: " + g.Voyage,
                                    g.PersonSign
                                }).ToList();

            // Person
            var dsPerson = Items.GroupBy(g => g.CustomsOfficeCode).FirstOrDefault()!
                                .Select(g => new
                                {
                                    g.PersonFamily,
                                    g.PersonSurName,
                                    g.PersonBirthYear,
                                    g.PersonBirthPlace,
                                    g.PersonCompany,
                                    g.PersonAddress,
                                    g.PersonPass,
                                }).ToList();

            // General Data (records)
            var dsRecords = Items.GroupBy(g => g.BLNum)
                               .Select(g => new
                               {
                                   g.FirstOrDefault()!.ExpOrderNum,
                                   BLNum = g.Key,
                                   CntrCount = g.Select(m => m.Cntr).Count(),
                                   GrossWt = g.Select(m => m.GrossWeights).Sum(),
                                   CntrTotalWt = g.Sum(m => m.CntrTotalWeight),
                                   g.FirstOrDefault()!.Commodities,
                                   //Commodities = string.Join("; ", g.Select(m => m.Commodities).Distinct()),
                               }).ToList();

            #endregion

            localReport.AddDataSource("dsLetter", dsCustomsDept);
            localReport.AddDataSource("dsPeron", dsPerson);
            localReport.AddDataSource("dsRecords", dsRecords);

            ReportResult result = localReport.Execute(RenderType.Pdf, extension, null, mimeType);
            //parameters
            //await Task.Delay(500);

            return File(result.MainStream, "application/pdf");
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            Console.WriteLine(msg);
            return Ok();
        }
    }

    [HttpGet]
    [Route("SaveXMLfile")]
    public async Task<IActionResult> SaveXMLfile(long Id)
    {
        try
        {
            var Item = await _exportOrderProvider.GetExportOrderDTOAsync(Id);

            await Task.Delay(100);

            if (Item is null) return Empty;

            string FileName = $"{Item.Num}_Customs";
            //string tempFilePath = Path.Combine(Environment.SpecialFolder.Resources.ToString(), "TempFiles", $"{FileName}_{Path.GetRandomFileName()}.xml");
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

    [HttpGet]
    [Route("SaveFillBillExcelFile")]
    public async Task<IActionResult> SaveFillBill(long vslcallid)
    {
        try
        {
            var Items = await _exportOrderProvider.GetVoyageManifestDTOAsync(vslcallid, false);

            await Task.Delay(100);

            if (Items.Count() == 0) return Empty;

            string FileName = $"FillBill_{Items.FirstOrDefault()!.Voyage}";
            //string dirName = Path.Combine(Environment.SpecialFolder.Resources.ToString(), "TempFiles");
            string tempFilePath = Path.Combine(_webHostEnvironment.WebRootPath, $"{Path.GetRandomFileName()}.xlsx");

            //DirectoryInfo dirInfo = new DirectoryInfo(dirName);
            //if (!dirInfo.Exists) { dirInfo.Create(); }
            //dirName = dirInfo.FullName;
            //string tempFilePath = Path.Combine(dirName, $"{Path.GetRandomFileName()}.xlsx");            

            using (var xls = new ExcelCreateService(tempFilePath, Items))
            {
                var buffer = await xls.CreateExcelFile();

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

    [HttpGet]
    [Route("DownloadExcelTemplate")]
    public async Task<IActionResult> DownloadTemplate()
    {
        try
        {
            string FileName = $"ИмпортСписка ДТ (проформа)";

            byte[] fileBytes = Resource.TemplateUploadCntrs;

            await Task.Delay(100);

            if (fileBytes != Array.Empty<byte>())
                return File(fileBytes, "application/xlsx", $"{FileName}.xlsx");

        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            return Empty;
        }

        return Ok();
    }
}
