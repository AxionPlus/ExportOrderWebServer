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

        string reportFileName = "BLstandard.rdlc";

        if (!string.IsNullOrEmpty(Item.BLtemplate))
        {
            if (Item.BLtemplate == "ametist" || Item.BLtemplate == "certa_lam" || Item.BLtemplate == "safetrans")
                reportFileName = "BL" + Item.BLtemplate + ".rdlc";
        }        

        try
        {
            string mimeType = "";
           // int extension = (int)(DateTime.Now.Ticks >> 10);
            int extension = 1;
            string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", reportFileName);

            LocalReport localReport = new LocalReport(pathReport);

            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //Encoding.GetEncoding("windows-1252");

            #region PARAMETERS
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "BLTemplate", Item.BLtemplate },
            };
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
                x.BLtemplate
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
                                                GrossWt = g.Sum(gr => gr.GrossWt),
                                                Volume = g.Sum(gr => gr.Volume),
                                                PackageNames = string.Join(", ", g.Select(gr => gr.PackageName).Distinct()),
                                                //PackageNames = g.GroupBy(gpk => gpk.PackageName).Count() > 1 ?
                                                //              string.Join(", ", g.Select(gr => gr.PackageName)) :
                                                //              g.Select(gr => gr.PackageName).FirstOrDefault(),
                                                //CntrCommodities = g.GroupBy(gcom => gcom.CommodityNameEn).Count() > 1 ?
                                                //                  string.Join("; ", g.Select(gr => gr.CommodityNameEn)) :
                                                //                  g.Select(gr => gr.CommodityNameEn).FirstOrDefault()
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
            
            ReportResult result = localReport.Execute(RenderType.Pdf, extension, parameters, mimeType);
       
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
    [Route("ViewReportManifest")]
    public async Task<IActionResult> ManifestReport(string Voyage)
    {
        var Items = await _exportOrderProvider.GetManifestAsync(Voyage);
        
        if (Items.Count() == 0) return Empty;

        try
        {
            string mimeType = "";
            int extension = (int)(DateTime.Now.Ticks >> 10);
            //int extension = 1;
            string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "Manifest.rdlc");

            LocalReport localReport = new LocalReport(pathReport);
            
            #region PARAMETERS

            // Count
            var Full20 = Items.Where(it => it.GrossWts is not null && it.CntrType.Substring(0, 2) == "20").GroupBy(it => it.Cntr).Count();
            var Full40 = Items.Where(it => it.GrossWts is not null && it.CntrType.Substring(0, 2) == "40").GroupBy(it => it.Cntr).Count();

            var Empty20 = Items.Where(it => it.GrossWts is null && it.CntrType.Substring(0, 2) == "20").GroupBy(it => it.Cntr).Count();
            var Empty40 = Items.Where(it => it.GrossWts is null && it.CntrType.Substring(0, 2) == "40").GroupBy(it => it.Cntr).Count();

            // Sum Weight
            var SumFull20 = Items.Where(it => it.GrossWts is not null && it.CntrType.Substring(0, 2) == "20").Sum(c => c.GrossWts);
            var SumFull40 = Items.Where(it => it.GrossWts is not null && it.CntrType.Substring(0, 2) == "40").Sum(c => c.GrossWts);

            // Sum Tare
            var SumFull20Tare = Items.Where(it => it.GrossWts is not null && it.CntrType.Substring(0, 2) == "20").Sum(c => c.CntrTareWt);
            var SumFull40Tare = Items.Where(it => it.GrossWts is not null && it.CntrType.Substring(0, 2) == "40").Sum(c => c.CntrTareWt);

            var SumEmpty20Tare = Items.Where(it => it.GrossWts is null && it.CntrType.Substring(0, 2) == "20").Sum(c => c.CntrTareWt);
            var SumEmpty40Tare = Items.Where(it => it.GrossWts is null && it.CntrType.Substring(0, 2) == "40").Sum(c => c.CntrTareWt);

            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "Full20Qty", Full20.ToString() },
                { "Full40Qty", Full40.ToString() },
                { "Empty20Qty", Empty20.ToString() },
                { "Empty40Qty", Empty40.ToString() },

                { "Full20TareWt", SumFull20Tare is not null ? SumFull20Tare.ToString()! : "0" },
                { "Full40TareWt", SumFull40Tare is not null ? SumFull40Tare.ToString()! : "0" },
                { "Empty20TareWt", SumEmpty20Tare is not null ? SumEmpty20Tare.ToString()! : "0" },
                { "Empty40TareWt", SumEmpty40Tare is not null ? SumEmpty40Tare.ToString()! : "0" },

                { "Full20GrossWt", SumFull20 is not null ? SumFull20.ToString()! : "0"},
                { "Full40GrossWt", SumFull40 is not null ? SumFull40.ToString()! : "0"},
            };

            #endregion

            localReport.AddDataSource("dsBsL", Items);

            ReportResult result = localReport.Execute(RenderType.Pdf, extension, parameters, mimeType);

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
            string dirName = Path.Combine(_webHostEnvironment.WebRootPath, "Xml");
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
}
