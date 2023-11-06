using AspNetCore.Reporting;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Data;
using System.IO;
using ExportOrderWebServer.Service;

namespace ExportOrderWebServer.Areas.ExpOrder.Controller;

[Route("server/[controller]")]
[Controller]

public class ExportOrderController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IExportOrderProvider _exportOrderProvider;

    public ExportOrderController(IWebHostEnvironment webHostEnvironment, IExportOrderProvider exportOrderProvider)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        _webHostEnvironment = webHostEnvironment;
        _exportOrderProvider = exportOrderProvider;
    }

    [HttpGet]
    [Route("ViewReportExpOrder")]
    public async Task<IActionResult> ExportOrderReport(long Id)
    {
        var Item = await _exportOrderProvider.GetItemDTOAsync(Id);

        try
        {
            string mimeType = "";
            int extension = 1;
            string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "ExportOrder.rdlc");

            LocalReport localReport = new LocalReport(pathReport);

            #region PARAMETERS
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "report", "new" },
            };
            #endregion

            #region DATA SOURCE

            var Items = new List<ExportOrderDTO>() { Item };

            var dsItem = Items.Select(x => new
            {
                x.Num,
                x.Dated,
                Vessel = x.VesselName + " (" + x.VesselFlag + ")",
                x.Voyage,
                x.DateOfLoading,
                x.POL,
                x.POD,
                x.Contract,
                x.ContractDate,
                x.MyCompanyName,
                x.Person
            });

            var dsRecords = Item.exportOrderRecordsDTO;

            var dsShippers = dsRecords?.Select(r => new { Shippers = r.Shipper }).Distinct().ToList();
            var dsConsignees = dsRecords?.Select(r => new { Consignees = r.Consignee }).Distinct().ToList();
            var dsCommodities = dsRecords?.GroupBy(r => r.CommodityName).Select(g => new {
                                                                                             Commodity = g.Key + " (" +
                                                                                                        g.FirstOrDefault()!.HSCode + ") " +
                                                                                                        g.FirstOrDefault()!.IMO + " " + g.FirstOrDefault()!.UNNO,
                                                                                          }).ToList();

            var dsDocuments = dsRecords?.GroupBy(r => r.DocumentName).Select(g => new {
                                                                                          Seq = 1,
                                                                                          Document = g.Key,
                                                                                          Pakages = g.Sum(q => q.PackageQty),
                                                                                          CntrTare = g.Sum(tr => tr.CntrTareWt),
                                                                                          Net = g.Sum(net => net.NetWt),
                                                                                          Gross = g.Sum(gr => gr.GrossWt)
                                                                                      }).ToList();            
            
            localReport.AddDataSource("dsItem", dsItem);
            localReport.AddDataSource("dsRecords", dsRecords);
            localReport.AddDataSource("dsShippers", dsShippers);
            localReport.AddDataSource("dsConsignees", dsConsignees);
            localReport.AddDataSource("dsCommodities", dsCommodities);
            localReport.AddDataSource("dsDocuments", dsDocuments);

            #endregion

            ReportResult result = localReport.Execute(RenderType.Pdf, extension, parameters, mimeType);

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
        var Item = await _exportOrderProvider.GetItemDTOAsync(Id);

        string fileName = "BL" + Item.BLtemplate + ".rdlc";

        try
        {
            string mimeType = "";
            int extension = (int)(DateTime.Now.Ticks >> 10);    //int extension = 1;
            string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", fileName);

            LocalReport localReport = new LocalReport(pathReport);

            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //Encoding.GetEncoding("windows-1252");

            #region PARAMETERS
            //Dictionary<string, string> parameters = new Dictionary<string, string>()
            //{
            //    { "BLTemplate", Item.BLtemplate },
            //};
            #endregion

            #region DATA SOURCE

            var Items = new List<ExportOrderDTO>() { Item };
            var Records = Item.exportOrderRecordsDTO;

            string Shippers = string.Join("\n", Records.Select(x => x.ShipperEn).Distinct().ToList());            
            string Consignees = string.Join("\n", Records.Select(x => x.ConsigneeEn).Distinct().ToList());
            string NotifyParties = string.Join("\n", Records.Select(x => x.ConsigneeEn).Distinct().ToList());

            // список товаров
            var CommoditiesGroup = Records.GroupBy(c => c.CommodityNameEn)
                                           .Select(g => new
                                           {
                                               Commodity = ( g.Key + " " +
                                                             g.FirstOrDefault()!.IMO + " " +
                                                             g.FirstOrDefault()!.UNNO
                                                           ).Trim()
                                           }).ToList();

            string Commodities = string.Join("\n", CommoditiesGroup.Select(cg => cg.Commodity));

            #region GROUP WITH 2 KEYS
            //var CommoditiesGroup = CommodityRecords.GroupBy(c => new { c.CommodityEngName, c.CntrType })
            //                                      .Select(g => new
            //                                      {
            //                                          Commodity = g.Key.CommodityEngName,
            //                                          g.Key.CntrType,

            //                                          g.FirstOrDefault()!.HSCode,
            //                                          g.FirstOrDefault()!.IsIMO,
            //                                          g.FirstOrDefault()!.IMO,
            //                                          g.FirstOrDefault()!.UNNO,
            //                                          g.FirstOrDefault()!.Measures,

            //                                          CntrTypesCount = g.ToList().Count,

            //                                          CommodityAggregated = string.Join(" ", g.ToList().Count,
            //                                                                                 "*",
            //                                                                                 g.FirstOrDefault()!.CntrType,
            //                                                                                 g.FirstOrDefault()!.CommodityEngName,
            //                                                                                 g.FirstOrDefault()!.IMO,
            //                                                                                 g.FirstOrDefault()!.UNNO).Trim(),
            //                                      }).ToList();
            #endregion

            // список типов контейнеров
            var CntrTypesGroup = Records.Where(r => r.CntrType is not null).GroupBy(r => r.CntrType)
                                        .Select(g => new
                                        {
                                            CntrTypes = string.Join(" ", g.ToList().Count, "*", g.Key),
                                        }).ToList();

            string CntrTypes = string.Join("\n", CntrTypesGroup.Select(ctg => ctg.CntrTypes));

            // общие данные
            var dsItem = Items.Select(x => new
            { 
               
                x.Num,
                x.BLDate,
                POLAgent = x.CarrierNameEn,
                x.PODAgent,
                x.VesselName,
                x.Voyage,
                x.POLEn,
                x.PODEn,
                Shippers,
                Consignees,
                NotifyParties,
                CntrTypes,
                Commodities,
                x.TotalCntrCount,
                x.TotalTareWeight,
                x.TotalGrossWeight,
                x.Measurement,
            }).ToList();

            // список контейнеров
            var dsCntrRecords = Records.GroupBy(r => r.Cntr)
                                            .Select(g => new
                                            {
                                                Cntr = g.Key,
                                                CntrType = g.Select(gr => gr.CntrType).FirstOrDefault()!,
                                                Seal = g.Select(gr => gr.Seal).FirstOrDefault()!,
                                                CntrTareWt = g.Select(gr => gr.CntrTareWt).FirstOrDefault()!,
                                                PackageQty = (uint)g.Sum(gr => gr.PackageQty),
                                                PackageNames = string.Join(", ", g.Select(gr => gr.PackageName).Distinct()),
                                                GrossWt = g.Sum(gr => gr.GrossWt),
                                                Volume = g.Sum(gr => gr.Volume),                                                                            
                                                CntrCommodities = string.Join("; ", g.Select(gr => gr.CommodityNameEn).Distinct())
                                            })                            
                                            .ToList();

            #region OLD список контейнеров
            //var dsCntrRecords = Records.Select(r => new
            //{
            //    r.Cntr,
            //    r.CntrType,
            //    r.Seal,
            //    r.PackageQty,
            //    r.PackageName,
            //    r.CntrTareWt,
            //    r.GrossWt,
            //    r.Volume,
            //    r.CommodityNameEn,
            //}).ToList();
            #endregion

            localReport.AddDataSource("dsBL", dsItem);
            localReport.AddDataSource("dsCntrRecords", dsCntrRecords);

            #endregion
            
            ReportResult result = localReport.Execute(RenderType.Pdf, extension, null, mimeType);

            //parameters
            //await Task.Delay(500);

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
    public async Task<IActionResult> ManifestReport(long vslcallid)    //string Voyage
    {
        //var Items = await _exportOrderProvider.GetManifestItemAsync(Voyage);
        var Items = await _exportOrderProvider.GetManifestAsync(vslcallid);

        if (Items.Count() == 0) return Empty;

        try
        {
            string mimeType = "";
            int extension = (int)(DateTime.Now.Ticks >> 10);    //int extension = 1;
            string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "Manifest.rdlc");

            LocalReport localReport = new LocalReport(pathReport);

            #region PARAMETERS
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                //{ "Full20Qty", Full20 > 0 ? Full20.ToString() : "0" },
            };
            #endregion

            #region DATA SOURCES

            // Count
            int full20Count = Items.Where(it => it.GrossWeights is not null || it.GrossWeights > 0)
                                   .Where(it => it.CntrType.Substring(0, 2) == "20")
                                   .GroupBy(it => it.Cntr).Count();
            int full40Count = Items.Where(it => it.GrossWeights is not null || it.GrossWeights > 0)
                                   .Where(it => it.CntrType.Substring(0, 2) == "40")
                                   .GroupBy(it => it.Cntr).Count();
            int empty20Count = Items.Where(it => it.GrossWeights is null || it.GrossWeights == 0)
                                    .Where(it => it.CntrType.Substring(0, 2) == "20")
                                    .GroupBy(it => it.Cntr).Count();
            int empty40Count = Items.Where(it => it.GrossWeights is null || it.GrossWeights == 0)
                                    .Where(it => it.CntrType.Substring(0, 2) == "40")
                                    .GroupBy(it => it.Cntr).Count();

            // Sum Full
            var full20weight = Items.Where(it => it.GrossWeights is not null && it.CntrType.Substring(0, 2) == "20").Sum(c => c.GrossWeights);
            var full40weight = Items.Where(it => it.GrossWeights is not null && it.CntrType.Substring(0, 2) == "40").Sum(c => c.GrossWeights);

            // Sum Tare
            var full20Tare = Items.Where(it => it.GrossWeights is not null || it.GrossWeights > 0)
                                  .Where(it => it.CntrType.Substring(0, 2) == "20")
                                  .Sum(c => c.CntrTareWt);
            var full40Tare = Items.Where(it => it.GrossWeights is not null || it.GrossWeights > 0)
                                  .Where(it => it.CntrType.Substring(0, 2) == "40")
                                  .Sum(c => c.CntrTareWt);
            var empty20Tare = Items.Where(it => it.GrossWeights is null || it.GrossWeights == 0)
                                   .Where(it => it.CntrType.Substring(0, 2) == "20")
                                   .Sum(c => c.CntrTareWt);
            var empty40Tare = Items.Where(it => it.GrossWeights is null || it.GrossWeights == 0)
                                   .Where(it => it.CntrType.Substring(0, 2) == "40")
                                   .Sum(c => c.CntrTareWt);

            // Total
            var total20 = (double)full20weight! + full20Tare;
            var total40 = (double)full40weight! + full40Tare;

            var totalCntrs = full20Count + full40Count + empty20Count + empty40Count;
            var totalWeight = (double)full20weight + (double)full40weight;
            var totalTare = Items.Sum(it => it.CntrTareWt);
            var total = totalWeight + totalTare;

            var dsSummary = new List<ManifestSummaryDTO>()
            {
                new ManifestSummaryDTO()
                {
                    Full20Count = full20Count,
                    Full40Count = full40Count,
                    Empty20Count = empty20Count,
                    Empty40Count = empty40Count,

                    Full20Weight = (double)full20weight,
                    Full40Weight = (double)full40weight,

                    Full20Tare = full20Tare,
                    Full40Tare = full40Tare,
                    Empty20Tare = empty20Tare,
                    Empty40Tare = empty40Tare,

                    Total20 = total20,
                    Total40 = total40,
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
    [Route("SaveXMLfile")]
    public async Task<IActionResult> SaveXMLfile(long Id)
    {
        try
        {
            var Item = await _exportOrderProvider.GetItemDTOAsync(Id);

            await Task.Delay(100);

            if (Item is null) return Empty;

            string FileName = $"{Item.Num}_Customs";
            string dirName = Path.Combine(_webHostEnvironment.WebRootPath, "TempFiles");
            string filePath = Path.Combine(dirName, $"{FileName}.xml");

            if (System.IO.File.Exists(filePath))
                filePath = Path.Combine(dirName, $"{FileName}_{Path.GetRandomFileName()}.xml");

            using (var xml = new XmlService(filePath, Item))
            {   
                var buffer = await xml.CreateXMLfile();

                if (buffer != Array.Empty<byte>())
                    return File(buffer, "application/xml", $"{FileName}.xml");
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
    [Route("SaveExcelFile")]
    public async Task<IActionResult> SaveFillBillFile(long vslcallid)
    {
        try
        {
            var Items = await _exportOrderProvider.GetFillBillAsync(vslcallid);

            await Task.Delay(100);

            if (Items.Count() == 0) return Empty;
            //if (Items is null) return Empty;

            string FileName = $"Fillbill_{Items.FirstOrDefault()!.Voyage}";
            string tempFilePath = Path.Combine(_webHostEnvironment.WebRootPath, $"TempFiles/{Path.GetRandomFileName()}.xlsx");

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
}
