using AspNetCore.Reporting;
using ExportOrderEntites.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Text;

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
            string pathReport = Path.Combine(_webHostEnvironment.WebRootPath, "Reports", "ExportOrder.rdlc");
            string fileName = $"{Item?.Num}_{DateOnly.FromDateTime(Item!.Dated!.Value)}_{Path.GetRandomFileName()}.pdf";

            LocalReport localReport = new LocalReport(pathReport);

            #region PARAMETERS
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                //{ "MoneyToText", await MoneyToTextParam(Invoice!) },
            };
            #endregion


            #region DATA SOURCE

            var dsItem = new List<ExportOrderDTO>() { Item };
            var dsShippers = Item._ExportOrderRecordsDTO!.GroupBy(s => s.Shipper);
            var dsConsignees = Item._ExportOrderRecordsDTO!.GroupBy(s => s.Consignee);
            var dsCommodities = Item._ExportOrderRecordsDTO?.GroupBy(r => r.CommodityName);
            var dsDocuments = Item._ExportOrderRecordsDTO?.GroupBy(r => r.DocumentName);

            localReport.AddDataSource("dsItem", Item);
            localReport.AddDataSource("dsRecords", Item?._ExportOrderRecordsDTO!.ToArray());            
            localReport.AddDataSource("dsShippers", dsShippers);
            localReport.AddDataSource("dsConsignees", dsConsignees);
            localReport.AddDataSource("dsCommmodities", dsCommodities);
            localReport.AddDataSource("dsDocuments", dsDocuments);

            //var dsAgreement = new List<AgreementCustomer>() { Invoice!.Agreement! };
            //var dsCustomer = new List<ContractorEntity>() { Invoice.Agreement?.Contractor! };
            //var dsCustomerAddress = new List<ContractorAddressDetail>() { Invoice.Agreement?.Contractor?.AddressDetails! };
            //var dsMyCompany = new List<MyCompanyEntity>() { Invoice.MyCompany };

            //localReport.AddDataSource("dsAgreement", dsAgreement.ToArray());
            //localReport.AddDataSource("dsCustomer", dsCustomer.ToArray());
            //localReport.AddDataSource("dsCustomerAddress", dsCustomerAddress.ToArray());
            //localReport.AddDataSource("dsMyCompany", dsMyCompany.ToArray());

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
