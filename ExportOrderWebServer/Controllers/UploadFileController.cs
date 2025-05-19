using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExportOrderWebServer.Controllers;

[AllowAnonymous]
[Route("file/[controller]")]
//[ApiController]
public class UploadFileController : ControllerBase
{
    private readonly IUploadResultService _UploadResultService;    
    private AppObjectResponse _AppObjectResponse = new();
    private readonly static string DirTemporary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TempFiles");

    public UploadFileController(IUploadResultService uploadResultService)
    {
        _UploadResultService = uploadResultService;

        if (!Directory.Exists(DirTemporary))
            Directory.CreateDirectory(DirTemporary);
    }


    [HttpPost, Route("UploadFromFileExcel")]    // file/UploadFileController/UploadFromExcel
    public async Task<List<UploadExcelDTO>?> UploadFromExcel([FromForm] IFormFile file) //AppObjectResponse?
    {
        if (file == null) return null;

        string filePath = Path.Combine(DirTemporary, string.Concat(Path.GetRandomFileName(), Path.GetExtension(file.FileName)));

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        using (IExcelFileUploadService _excelUploadService = new ExcelFileUploadService())
        {
            var uploadResult = await _excelUploadService.ReadExcelFileExportOrder(filePath);

            // ... CheckService
            // ... write AppObjectResponse.Object1 
            // ... write AppObjectResponse.Object2

            return uploadResult;
        }
    }

    [HttpPost, Route("UploadFromFileXML")]    // file/UploadFileController/UploadFromExcel
    public async Task<AppObjectResponse?> UploadFromXML([FromForm] IEnumerable<IFormFile> files)
    {
        _AppObjectResponse = new();

        string DirTemporaryXml = Path.Combine(DirTemporary, $"UploadedXmlFiles_{new Random().Next(0,100)}");

        if (!Directory.Exists(DirTemporaryXml))
            Directory.CreateDirectory(DirTemporaryXml);

        foreach (var file in files)
            if (file != null)
            {
                string filePath = Path.Combine(DirTemporaryXml, string.Concat(Path.GetRandomFileName(), "_", Path.GetExtension(file.FileName)));
                using var stream = new FileStream(filePath, FileMode.Create);
                file.CopyTo(stream);
            }
            else
                _AppObjectResponse.ErrorAdd("File not found.");

        using (IXmlFileReadService xmlFileReadService = new XmlFileReadService())
        {
            var uploadResult = await xmlFileReadService.ReadXmlFileDocuments(DirTemporaryXml);

            if (uploadResult is not null && uploadResult.Any())
            {   
                _AppObjectResponse.Object = uploadResult;
                _AppObjectResponse.Description = $"Всего ДТ: {uploadResult.Count}";

                /// Check uploaded DATA
                var checkResponse = await _UploadResultService.CheckUploadedDocuments(uploadResult);
                if (checkResponse is not null && checkResponse.Any())
                    _AppObjectResponse.Errors.AddRange(checkResponse);
            }
            else
                _AppObjectResponse.ErrorAdd("Xml file wasn't read.");
        }

        return _AppObjectResponse;
    }
}