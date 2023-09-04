using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using ExportOrderWebServer.Areas.ExpOrder.Provider;
using ExportOrderWebServer.Service;

namespace ExportOrderWebServer.UploadFile;

[AllowAnonymous]
[Route("file/[controller]")]
[ApiController]

public class UploadFileController : ControllerBase
{
    public readonly IExportOrderProvider _exportOrderProvider;

    public UploadFileController(IExportOrderProvider exportOrderProvider)
    {
        _exportOrderProvider = exportOrderProvider;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }



    [HttpPost]
    [Route("CheckExpOrderRecords")]
    public async Task<UploadedResult> CheckImpManifest([FromForm] IEnumerable<IFormFile> files)
    {
        var filesDir = new List<string>();
        var items = new List<ExportOrderRecord>();

        var UploadedResults = new UploadedResult()
        {
            CntrCount = items.Count(),
            CntrContentCount = items.SelectMany(s => s.Contents).ToList().DistinctBy(x => x.DocumentRecord?.Id).Count()
            //Errors = err,
        };

        return UploadedResults;
    }

    [HttpPost]
    [Route("UploadExpOrderRecords")]
    public async Task<UploadedResult> UploadExpOrderRecords([FromForm] IEnumerable<IFormFile> files)
    {
        var filesDir = new List<string>();
        var items = new List<ExportOrderRecord>();

        foreach (var file in files)
            if (file != null)
            {
                //string dirName = Path.Combine(Environment.SpecialFolder.Resources.ToString(), "Temp");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                //Get url To Save
                string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);
                filesDir.Add(SavePath);
                using (var stream = new FileStream(SavePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }

        foreach (var fileDir in filesDir)
        {
            using (var exl = new ExcelService(fileDir))
            {
                var uploadingRecords = exl.ReadUploadingFile();
                items.AddRange(uploadingRecords);
            }
        }

        items = items.DistinctBy(s => s.Id).ToList();

        var appresponse = await _exportOrderProvider.AddUploadedFileItemsAsync(items);

        if (!appresponse.HasError)
        {
            return new UploadedResult()
            {
                CntrCount = items.Count(),
                CntrContentCount = items.SelectMany(s => s.Contents).ToList().DistinctBy(x => x.DocumentRecord?.Id).Count()
            };
        }
        else
        {
            return new UploadedResult()
            {
                Errors = appresponse.Error.ToArray(),
            };
        }
    }
}

public class UploadedResult
{
    public int CntrCount { get; set; }
    public int CntrContentCount { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();
    public bool HasErrors => Errors.Count() > 0;
    public IEnumerable<string> ErrorsCntrList { get; set; } = new List<string>();
    public bool HasErrorsCntrList => ErrorsCntrList.Count() > 0;
}