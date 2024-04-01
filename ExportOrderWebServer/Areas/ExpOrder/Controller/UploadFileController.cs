using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExportOrderWebServer.Areas.ExpOrder.Controller;

[AllowAnonymous]
[Route("file/[controller]")]
[ApiController]

public class UploadFileController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    public readonly ICntrTypeProvider _cntrTypeProvider;
    public readonly IDocumentProvider _documentProvider;
    public readonly IVesselCallProvider _vesselCallProvider;

    public UploadFileController(IWebHostEnvironment webHostEnvironment, ICntrTypeProvider cntrTypeProvider, IDocumentProvider documentProvider, IVesselCallProvider vesselCallProvider)
    {
        _webHostEnvironment = webHostEnvironment;
        _cntrTypeProvider = cntrTypeProvider;
        _documentProvider = documentProvider;
        _vesselCallProvider = vesselCallProvider;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }


    [HttpPost]
    [Route("UploadFromExcel")]
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
}
