using AspNetCore.Reporting;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Net;
using System.Numerics;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data;

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

            var dsItem = new List<ExportOrderDTO>() { Item };
            var dsRecords = Item.exportOrderRecordsDTO;

            var dsShippers = dsRecords?.Select(r => new { ShipperName = r.Shipper }).Distinct().ToList();
            var dsConsignees = dsRecords?.Select(r => new { ConsigneeName = r.Consignee }).Distinct().ToList();
            var dsCommodities = dsRecords?.GroupBy(r => r.CommodityName).Select(g => new
                                                                                         {
                                                                                             Commodity = g.Key,
                                                                                             HScode = g.FirstOrDefault()!.HSCode,
                                                                                             IMO = g.FirstOrDefault()!.IMO,
                                                                                             UNNO = g.FirstOrDefault()!.UNNO,
                                                                                             IsIMO = g.FirstOrDefault()!.IsIMO
                                                                                         }).ToList();

            var dsDocuments = dsRecords?.GroupBy(r => r.DocumentName).Select(g => new
                                                                                      {
                                                                                          Seq = 1,
                                                                                          Document = g.Key,
                                                                                          Pakages = g.Sum(q => q.PackageQty),
                                                                                          Net = g.Sum(net => net.NetWt),
                                                                                          Gross = g.Sum(gr => gr.GrossWt)
                                                                                      }).ToList();

            //var dsCommodities = dsRecords?.Select(s => new {
            //                                                 Commodity = s.CommodityName,
            //                                                 HScode = s.HSCode,
            //                                                 IMO = s.IMO,
            //                                                 UNNO = s.UNNO,
            //                                                 IsIMO = s.IsIMO }).Distinct().ToList();

            //var dsDocuments = dsRecords?.Select(s => new {
            //                                                 Seq = 1,
            //                                                 Document = s.DocumentName,
            //                                                 Pakages = dsRecords.Sum(q=>q.Quantity),
            //                                                 Net = dsRecords.Sum(net => net.NetWt),
            //                                                 Gross = dsRecords.Sum(gr => gr.GrossWt)
            //                                             }).ToList();
            
            
            localReport.AddDataSource("dsItem", dsItem);
            localReport.AddDataSource("dsRecords", dsRecords);
            localReport.AddDataSource("dsShippers", dsShippers);
            localReport.AddDataSource("dsConsignees", dsConsignees);
            localReport.AddDataSource("dsCommmodities", dsCommodities);
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

        try
        {
            string mimeType = "";
            int extension = 1;
            string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "BLStandart.rdlc");

            LocalReport localReport = new LocalReport(pathReport);

            #region PARAMETERS
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "report", "new" },
            };
            #endregion

            #region DATA SOURCE

            var Items = new List<ExportOrderDTO>() { Item };
            var Records = Item.exportOrderRecordsDTO;

            var ShipperList = Records.Select(x => x.Shipper).Distinct().ToList();
            var ConsigneeList = Records.Select(x => x.Consignee).Distinct().ToList();
            var NotifyList = Records.Select(x => x.Consignee).Distinct().ToList();

            StringBuilder sb = new StringBuilder();

            string Shippers = "";
            foreach (var item in ShipperList)
                Shippers = sb.Append(item + "\n").ToString();

            Shippers = Shippers.Trim(); sb.Clear();

            string Consignees = "";
            foreach (var item in ConsigneeList)
                Consignees = sb.Append(item + "\n").ToString();

            Consignees = Consignees.Trim(); sb.Clear();

            string NotifyParties = "";
            foreach (var item in ConsigneeList)
                NotifyParties = sb.Append(item + "\n").ToString();

            NotifyParties = NotifyParties.Trim(); sb.Clear();


            // список товаров
            var CommodityRecords = Records.Select(r => new
            {
                r.CommodityEngName,
                r.HSCode,
                r.IsIMO,
                r.IMO,
                r.UNNO,
                r.CntrType,
                Measures = r.Volume > 0 ? "CUB. M" : "KG"
            }).ToList();

            //List<string> CommAndTypes = new List<string>();

            var CommoditiesGroup = CommodityRecords.GroupBy(c => new { c.CommodityEngName, c.CntrType })
                                                  .Select(g => new
                                                  {
                                                      Commodity = g.Key.CommodityEngName,
                                                      g.Key.CntrType,
                                                      g.FirstOrDefault()!.HSCode,
                                                      g.FirstOrDefault()!.IsIMO,
                                                      g.FirstOrDefault()!.IMO,
                                                      g.FirstOrDefault()!.UNNO,
                                                      g.FirstOrDefault()!.Measures,
                                                      CntrTypesCount = g.ToList().Count,

                                                      CommodityAggregated = string.Join(" ", g.ToList().Count,
                                                                                             "*",
                                                                                             g.FirstOrDefault()!.CntrType,
                                                                                             g.FirstOrDefault()!.CommodityEngName,
                                                                                             g.FirstOrDefault()!.IMO,
                                                                                             g.FirstOrDefault()!.UNNO).Trim(),
                                                      //CommAndTypes = g.ToList()
                                                  })
                                                  .ToList();

            var dsCommodities = CommoditiesGroup;

            string Commodities = "";
            foreach (var item in CommoditiesGroup)
                Commodities = sb.Append(item.CommodityAggregated + "\n").ToString();

            // общие данные
            var dsItem = Items.Select(x => new
            { 
                x.Num,
                POLAgent = x.CarrierName,
                x.PODAgent,
                Vessel = x.VesselName,
                x.Voyage,
                x.POL,
                x.POD,
                Shippers,
                Consignees,
                NotifyParties,
                Commodities,
            }).ToList();

            // список контейнеров
            var dsCntrRecords = Records.Select(r => new 
            {
                r.Cntr,
                r.CntrType,
                r.Seal,
                r.PackageQty,
                r.PackageName,
                r.CntrTareWt,
                r.GrossWt,
                r.Volume,                
            }).ToList();

            //List<string> dsCommodities = new List<string>();

            //foreach (var commodity in CommoditiesGroup)
            //{
            //    var GroupString = new
            //    {
            //        commodity.Commodity,
            //        commodity.CommAndTypes.FirstOrDefault().HSCode,
            //        commodity.CommAndTypes.FirstOrDefault().IsIMO,
            //        commodity.CommAndTypes.FirstOrDefault().IMO,
            //        commodity.CommAndTypes.FirstOrDefault().UNNO,
            //        commodity.CommAndTypes.FirstOrDefault().CntrType,
            //        CntrTypesCount = commodity.CommAndTypes.Count(),
            //        commodity.CommAndTypes.FirstOrDefault()!.Measures
            //    };

            //    dsCommodities.Add(GroupString.ToString()!);
            //}

            #region Attempts
            //var CommsGroup = Commodities.GroupBy(com => com.CommodityEngName); //.ToList();
            //var Comms = Records.GroupBy(r => r.CommodityEngName);

            //List<ExportOrderRecordDTO> ListOfComm = new List<ExportOrderRecordDTO>();
            //List<string[]> ListComms = new List<string[]>();
            //foreach (var com in CommsGroup)
            //    ListComms.Add(com);

            //var dsCommods = ListComms.GroupBy(co => co.CntrType).ToList();
            //var ContrType = Records.GroupBy(r => r.CntrType).Select(type => new { cntrType = type.Key});

            // список товаров
            //var CommoditiesGroup = Records.GroupBy(rgroup => rgroup.CommodityEngName).Select(g => new
            //{
            //    Commodity = g.Key,
            //    HScode = g.FirstOrDefault()!.HSCode,
            //    IMO = g.FirstOrDefault()!.IMO,
            //    UNNO = g.FirstOrDefault()!.UNNO,
            //    IsIMO = g.FirstOrDefault()!.IsIMO,

            //    CntrType = g.FirstOrDefault()!.CntrType,

            //    //CntrCount = g.Count(),
            //    //CntrType = g.Select(type => new { cntrType = type.CntrType }),
            //    _CntrTypes = g.Select(type => type.CntrType).Distinct(),
            //    //__CntrType = g.GroupBy(rgroup => rgroup.CntrType),
            //    //___CntrType = g.Select(rgroup => rgroup).Select(x => x.CntrType).Distinct(),
            //    //____CntrType = g.Select(x => x.CntrType).Distinct(),
            //}).ToList();


            //foreach (var commodity in CommoditiesGroup) //CommsGroup
            //    foreach (var type in commodity._CntrTypes)
            //    {
            //        var commString = new
            //        {
            //            Commodity = commodity.Commodity,
            //            HScode = commodity.HScode,
            //            IMO = commodity.IMO,
            //            UNNO = commodity.UNNO,
            //            IsIMO = commodity.IsIMO,
            //            CntrType = type
            //        };

            //        CommWithTypes.Add(commString.ToString());
            //    }

            //List<string> CommWithTypes = new List<string>();

            //foreach (var commodity in CommsGroup)
            //{
            //    foreach (var type in commodity)
            //    {
            //        var commNtype = new
            //        {
            //            CommodityEngName = type.CommodityEngName,
            //            HSCode = type.HSCode,
            //            IMO = type.IMO,
            //            UNNO = type.UNNO,
            //            IsIMO = type.IsIMO,
            //            CntrType = type.CntrType,
            //            Count = commodity.Where(t => t.CommodityEngName == commodity.Key).Select(t => t.CntrType).Count(),
            //            cnt = type.CntrType.Where(c => type.CommodityEngName == commodity.Key).Count()
            //        };

            //        CommWithTypes.Add(commNtype.ToString());
            //    }
            //}

            //var dsCommodities = CommWithTypes.GroupBy(type => type).ToList();


            //var CommG = Records.GroupBy(r => r.CommodityEngName, c => c.CntrType);
            //var CommG_ = Commodities.GroupBy(r => r.CommodityEngName, c => c.CntrType);


            //var CommGroup = Records.GroupBy(r => r.CommodityEngName, c => c.CntrType).Select(g => new
            //{
            //    Commodity = g.Key,
            //    //HScode = g.FirstOrDefault(),
            //    CntrType = g.Select(x => x.GroupBy(cType => cType).Distinct())
            //}).ToList();

            //var _CommGroup = Records.GroupBy(r => r.CommodityEngName, c => c.CntrType).Select(g => new
            //{
            //    Commodity = g.Key,
            //    //HScode = g.FirstOrDefault(),
            //    CntrType = g.Select(c => g.Key)
            //}).ToList();

            #endregion

            localReport.AddDataSource("dsBL", dsItem);
            localReport.AddDataSource("dsCntrRecords", dsCntrRecords);
            localReport.AddDataSource("dsCommodities", dsCommodities);            

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
}
