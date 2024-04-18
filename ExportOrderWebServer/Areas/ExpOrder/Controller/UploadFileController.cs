using AspNetCore.Reporting;
using ExportOrderWebServer.Areas.ExpOrder.Provider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ExportOrderWebServer.Areas.ExpOrder.Controller;

[AllowAnonymous]
[Route("file/[controller]")]
[ApiController]

public class UploadFileController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IExportOrderProvider _exportOrderProvider;

    public UploadFileController(IWebHostEnvironment webHostEnvironment,
                                IExportOrderProvider exportOrderProvider)
    {
        _webHostEnvironment = webHostEnvironment;
        _exportOrderProvider = exportOrderProvider;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }


    [HttpPost]
    [Route("UploadFromExcel")] // file/UploadFileController/UploadFromExcel
    public async Task<List<UploadExcelDTO>> UploadFromExcel([FromForm] IEnumerable<IFormFile> files)
    {
        string filePath = string.Empty;

        foreach (var file in files)
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                filePath = Path.Combine(_webHostEnvironment.WebRootPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }

        using (var exl = new ExcelUploadService(filePath))
        {
            try
            {
                var uploadResult = await exl.ReadUploadingFile();

                return uploadResult;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                Console.WriteLine(msg);
                return new List<UploadExcelDTO>();
            }
        }
    }


    [HttpPost, Route("SaveExportOrdersReport")] // file/UploadFile/SaveExportOrdersReport
    public async Task<IActionResult> SaveExportOrdersReport([FromBody] object obj)
    {
        try
        {
            IEnumerable<long> Ids = Enumerable.Empty<long>();
            string voyageNo = string.Empty;

            var resultObj = JsonConvert.DeserializeObject<ControllerPassObject<IEnumerable<long>>>(obj.ToString());
            if (resultObj is not null)
            {
                if (resultObj.GetObject.Count() > 0)
                {
                    Ids = resultObj.GetObject;
                    voyageNo = resultObj.Remarks;
                }
            }

            var Items = await _exportOrderProvider.GetExportOrdersAsync(Ids);

            await Task.Delay(100);

            if (Items is null || !Items.Any()) return Empty; //Items.Count == 0

            string mimeType = "";
            int pageIndex = new Random().Next(1, 101);
            string pathReport = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", "ExportOrderMulti.rdlc");
            string fileName = $"{voyageNo}_ExpOrders_{pageIndex}";

            LocalReport localReport = new LocalReport(pathReport);
            localReport.AddDataSource("dsExportOrders", Items);
            ReportResult result = localReport.Execute(RenderType.Pdf, pageIndex, null, mimeType);

            return File(result.MainStream, "application/pdf", $"{fileName}.pdf");
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            Console.WriteLine(msg);
            return Empty;
        };
    }
}
