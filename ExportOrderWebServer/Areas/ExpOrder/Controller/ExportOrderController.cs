using AspNetCore.Reporting;
using Microsoft.AspNetCore.Mvc;
using System.Text;
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

            var Items = new List<ExportOrderDTO>() { Item };

            var dsItems = Items.Select(x => new
            {
                x.Num,
                x.Dated,
                Vessel = x.VesselName + "(" + x.VesselFlag + ")",
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
            var dsCommodities = dsRecords?.GroupBy(r => r.CommodityName).Select(g => new
                                                                                         {
                                                                                             g.Key,
                                                                                             HScode = g.FirstOrDefault()!.HSCode,
                                                                                             g.FirstOrDefault()!.IMO,
                                                                                             g.FirstOrDefault()!.UNNO,
                                                                                             g.FirstOrDefault()!.IsIMO,
                                                                                             Commodity = g.Key + " (" + g.FirstOrDefault()!.HSCode +
                                                                                                                  ") " + g.FirstOrDefault()!.IMO + " " +
                                                                                                                  g.FirstOrDefault()!.UNNO,
                                                                                          }).ToList();

            var dsDocuments = dsRecords?.GroupBy(r => r.DocumentName).Select(g => new
                                                                                      {
                                                                                          Seq = 1,
                                                                                          Document = g.Key,
                                                                                          Pakages = g.Sum(q => q.PackageQty),
                                                                                          Net = g.Sum(net => net.NetWt),
                                                                                          Gross = g.Sum(gr => gr.GrossWt)
                                                                                      }).ToList();            
            
            localReport.AddDataSource("dsItem", dsItems);
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

            var ShipperList = Records.Select(x => x.ShipperEn).Distinct().ToList();
            var ConsigneeList = Records.Select(x => x.ConsigneeEn).Distinct().ToList();
            var NotifyList = Records.Select(x => x.ConsigneeEn).Distinct().ToList();

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
                                                  }).ToList();

            var dsCommodities = CommoditiesGroup;

            string Commodities = "";
            foreach (var item in CommoditiesGroup)
                Commodities = sb.Append(item.CommodityAggregated + "\n").ToString();

            // общие данные
            var dsItem = Items.Select(x => new
            { 
                x.Num,
                POLAgent = x.CarrierNameEn,
                x.PODAgent,
                Vessel = x.VesselName,
                x.Voyage,
                x.POLEn,
                x.PODEn,
                Shippers,
                Consignees,
                NotifyParties,
                Commodities,
                x.TotalCntrCount,
                x.TotalGrossWeight,
                x.TotalTareWeight,
                Measures = CommodityRecords.FirstOrDefault()!.Measures
            }).ToList();

            // список контейнеров
            var dsCntrRecords = Records.Select(r => new 
            {
                m = r.BLnum,
                r.Cntr,
                r.CntrType,
                r.Seal,
                r.PackageQty,
                r.PackageName,
                r.CntrTareWt,
                r.GrossWt,
                r.Volume,                
            }).ToList();

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

    [HttpGet]
    [Route("ViewReportManifest")]
    public async Task<IActionResult> ManifestReport(long vslCallId, long carrierId)
    {
        var Items = await _exportOrderProvider.GetManifestItemsAsync(vslCallId, carrierId);

        try
        {
            string mimeType = "";
            int extension = 1;
            string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "Manifest.rdlc");

            LocalReport localReport = new LocalReport(pathReport);

            #region PARAMETERS

            // Count
            var Full20 = Items.Where(it => it.GrossWt is not null && it.CntrType.Substring(0, 2) == "20").GroupBy(it => it.Cntr).Count();
            var Full40 = Items.Where(it => it.GrossWt is not null && it.CntrType.Substring(0, 2) == "40").GroupBy(it => it.Cntr).Count();

            var Empty20 = Items.Where(it => it.GrossWt is null && it.CntrType.Substring(0, 2) == "20").GroupBy(it => it.Cntr).Count();
            var Empty40 = Items.Where(it => it.GrossWt is null && it.CntrType.Substring(0, 2) == "40").GroupBy(it => it.Cntr).Count();

            // Sum Weight
            var SumFull20 = Items.Where(it => it.GrossWt is not null && it.CntrType.Substring(0, 2) == "20").Sum(c => c.GrossWt);
            var SumFull40 = Items.Where(it => it.GrossWt is not null && it.CntrType.Substring(0, 2) == "40").Sum(c => c.GrossWt);

            // Sum Tare
            var SumFull20Tare = Items.Where(it => it.GrossWt is not null && it.CntrType.Substring(0, 2) == "20").Sum(c => c.CntrTareWt);
            var SumFull40Tare = Items.Where(it => it.GrossWt is not null && it.CntrType.Substring(0, 2) == "40").Sum(c => c.CntrTareWt);

            var SumEmpty20Tare = Items.Where(it => it.GrossWt is null && it.CntrType.Substring(0, 2) == "20").Sum(c => c.CntrTareWt);
            var SumEmpty40Tare = Items.Where(it => it.GrossWt is null && it.CntrType.Substring(0, 2) == "40").Sum(c => c.CntrTareWt);

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

            #region DATA SOURCE
            //var dsItems = Items;

            #region SEPARATE DSOURCES
            //var dsItems = Items.Select(x => new
            //{
            //    BLNum = x.Num,
            //    x.VesselName,
            //    x.Voyage,
            //    x.BLDate,
            //    x.VesselFlagEn,
            //    x.POLEn,
            //    x.PODEn,
            //    Commodities = new List<ExportOrderRecordDTO>(),
            //    //Commodities = Items.GroupBy(eo => x.exportOrderRecordsDTO.FirstOrDefault()),
            //    Records = x.exportOrderRecordsDTO
            //}).ToList();



            //List<ExportOrderRecordDTO> Commodities = new List<ExportOrderRecordDTO>();
            //var dsCommodities = new List<ExportOrderRecordDTO>();
            //var dsCom = new { };

            //foreach (var Item in Items)
            //{
            //    List<ExportOrderRecordDTO> Commodities = new List<ExportOrderRecordDTO>();

            //    var CommodityRecords = Item.exportOrderRecordsDTO.Select(r => new
            //    {
            //        r.CommodityEngName,
            //        r.CntrTareWt,
            //        r.GrossWt,                    
            //    }).ToList();

            //    var CommoditiesGroup = CommodityRecords.GroupBy(cr => cr.CommodityEngName)
            //                            .Select(g => new ExportOrderRecordDTO()
            //                            {
            //                                BLnum = Item.Num,
            //                                CommodityEngName = g.Key,
            //                                CntrTareWt = g.Sum(trw => trw.CntrTareWt),
            //                                GrossWt = g.Sum(gw => gw.GrossWt),
            //                            }).ToList();

            //    foreach (var commodity in CommoditiesGroup)
            //    {
            //        Commodities.Add(commodity);
            //        dsItems.FirstOrDefault(s => s.BLNum == Item.Num)!.Commodities.Add(commodity);
            //    }

            //    //dsItems.FirstOrDefault(s => s.BLNum == Item.Num).Commodities.Add(Commodities);
            //    //dsCommodities.Add(Commodities);

            //}

            //var dsCommodities = Commodities;
            #endregion

            //localReport.AddDataSource("dsBL", dsItems);
            localReport.AddDataSource("dsBsL", Items);

            #endregion

            ReportResult result = localReport.Execute(RenderType.Pdf, extension, parameters, mimeType);
            //ReportResult result = localReport.Execute(RenderType.Pdf, extension, parametersInt, mimeType);

            return File(result.MainStream, "application/pdf");
        }
        catch (Exception ex)
        {
            string message = ex.Message;
            return Ok();
        }

    }
}
