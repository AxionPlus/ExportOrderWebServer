using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExportOrderWebServer.Service;

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
                var result = await exl.ReadUploadingFile();

                return result;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                Console.WriteLine(msg);
                return new List<UploadExcelDTO>();
            }
        }
    }

   
    // NOT USED, DELETE
    //[HttpPost]
    //[Route("UploadExportOrder")]
    //public async Task<UploadResult> UploadExportOrder([FromForm] IEnumerable<IFormFile> files)
    //{
    //    string filePath = string.Empty;        

    //    var cntrTypes = await _cntrTypeProvider.GetCntrTypes();

    //    foreach (var file in files)
    //        if (file != null)
    //        {
    //            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
    //            //filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);
    //            filePath = Path.Combine(_webHostEnvironment.WebRootPath, fileName);
                
    //            using (var stream = new FileStream(filePath, FileMode.Create))
    //            {
    //                file.CopyTo(stream);
    //            }
    //        }

    //    using (var exl = new _ExcelUploadService(filePath, cntrTypes, _documentProvider))
    //    {
    //        try
    //        {
    //            var result = await exl._ReadUploadingFile();

    //            var ur = new UploadResult()
    //            {
    //                _ExportOrderRecords = result.Item1.ToList(),
    //                _Documents = result.Item2.ToList(),
    //            };

    //            return ur;
    //        }
    //        catch (Exception ex)
    //        {
    //            var msg = ex.Message;
    //            return new UploadResult() { Summary = new List<string>() { msg } };
    //        }
    //    }
    //}
}
