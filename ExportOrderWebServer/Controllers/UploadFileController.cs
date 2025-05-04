using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExportOrderWebServer.Controllers;

[AllowAnonymous]
[Route("file/[controller]")]
//[ApiController]
public class UploadFileController : ControllerBase
{
    private readonly static string DirTemporary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TempFiles");

    public UploadFileController()
    {
        if (!Directory.Exists(DirTemporary))
            Directory.CreateDirectory(DirTemporary);
    }


    [HttpPost, Route("UploadFromExcel")]    // file/UploadFileController/UploadFromExcel
    public async Task<List<UploadExcelDTO>?> UploadFromExcel([FromForm] IFormFile file)
    {
        if (file == null) return null;

        string filePath = Path.Combine(DirTemporary, Path.GetRandomFileName() + Path.GetExtension(file.FileName));

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        using (IExcelFileUploadService _excelUploadService = new ExcelFileUploadService())
        {
            var uploadResult = await _excelUploadService.ReadExcelFileExportOrder(filePath);

            return uploadResult;
        }
    }
}