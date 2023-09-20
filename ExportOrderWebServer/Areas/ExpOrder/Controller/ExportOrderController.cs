using AspNetCore.Reporting;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Net;
using System.Numerics;

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
    [Route("ViewReport")]
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
            var dsCommodities = dsRecords?.GroupBy(rec => rec.CommodityName).Select(g => new
                                                                                            {
                                                                                                Commodity = g.Key,
                                                                                                HScode = g.FirstOrDefault()!.HSCode,
                                                                                                IMO = g.FirstOrDefault()!.IMO,
                                                                                                UNNO = g.FirstOrDefault()!.UNNO,
                                                                                                IsIMO = g.FirstOrDefault()!.IsIMO
                                                                                            }).ToList();

            var dsDocuments = dsRecords?.GroupBy(rec => rec.DocumentName).Select(g => new
                                                                                        {
                                                                                            Seq = 1,
                                                                                            Document = g.Key,
                                                                                            Pakages = g.Sum(q => q.Quantity),
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
}
