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
    public readonly ICntrTypeProvider _cntrTypeProvider;

    public UploadFileController(IExportOrderProvider exportOrderProvider , ICntrTypeProvider cntrTypeProvider)
    {
        _exportOrderProvider = exportOrderProvider;
        _cntrTypeProvider = cntrTypeProvider;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
    }



    [HttpPost]
    [Route("CheckExpOrderRecords")]
    public async Task<UploadedResult> CheckImpManifest([FromForm] IEnumerable<IFormFile> files)
    {
        var filesDir = new List<string>();
        var items = new List<ExportOrderRecord>();
        var cntrTpSzList = await _cntrTypeProvider.GetCntrTypes();

        foreach (var file in files)
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);
                filesDir.Add(SavePath);
                using (var stream = new FileStream(SavePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }

        foreach (var fileDir in filesDir)
            using (var exl = new ExcelService(fileDir, cntrTpSzList))
            {
                var uploadingRecords = exl.ReadUploadingFile();
                items.AddRange(uploadingRecords);
            }

        items = items.DistinctBy(s => s.Id).ToList();

        #region CHECK Entity before Upload

        var errList = new List<string>();
        
        foreach (var item in items)
        {
            // Empties:

            if (item.CntrTareWt !> 0)
                errList.Add($"Cntr: {item.CntrNum} - Tare weight is empty.");

            if (string.IsNullOrWhiteSpace(item.Seal))
                errList.Add($"Cntr: {item.CntrNum} - Seal is empty.");

            // Content records:

            if (item.Contents.Count() > 0)
                foreach (var content in item.Contents)
                {
                    // Empties:

                    if (content.Quantity !> 0)
                        errList.Add($"Cntr: {item.CntrNum} - Pakage Quantity is empty.");

                    if (content.GrossWt !> 0)
                        errList.Add($"Cntr: {item.CntrNum} - Gross weight is empty.");

                    if (content.NetWt !> 0)
                        errList.Add($"Cntr: {item.CntrNum} - Net weight is empty.");

                    if (content.Volume !> 0)
                        errList.Add($"Cntr: {item.CntrNum} - Volume is empty.");
                }
            else
                errList.Add($"List of Content to upload is empty.");
        }

        #endregion

        var UploadedResults = new UploadedResult()
        {
            CntrCount = items.Count(),
            CntrContentCount = items.SelectMany(s => s.Contents).ToList().DistinctBy(x => x.DocumentRecord?.Id).Count(),
            Errors = errList,
        };

        return UploadedResults;
    }

    [HttpPost]
    [Route("UploadExpOrderRecords")]
    public async Task<UploadedResult> UploadExpOrderRecords([FromForm] IEnumerable<IFormFile> files)
    {
        var filesDir = new List<string>();
        var items = new List<ExportOrderRecord>();
        var cntrTpSzList = await _cntrTypeProvider.GetCntrTypes();

        foreach (var file in files)
            if (file != null)
            {
                //string dirName = Path.Combine(Environment.SpecialFolder.Resources.ToString(), "Temp");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);
                filesDir.Add(SavePath);
                using (var stream = new FileStream(SavePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }

        foreach (var fileDir in filesDir)
        {
            using (var exl = new ExcelService(fileDir, cntrTpSzList))
            {
                var uploadingRecords = exl.ReadUploadingFile();
                items.AddRange(uploadingRecords);
            }
        }

        //items = items.DistinctBy(s => s.Id).ToList();

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