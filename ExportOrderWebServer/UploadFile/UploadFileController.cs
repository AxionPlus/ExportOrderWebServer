using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using ExportOrderWebServer.Service;
using static MudBlazor.CategoryTypes;
using ExportOrderEntites.ExportOrder;

namespace ExportOrderWebServer.UploadFile;

[AllowAnonymous]
[Route("file/[controller]")]
[ApiController]

public class UploadFileController : ControllerBase
{
    public readonly IExportOrderProvider _exportOrderProvider;
    public readonly ICntrTypeProvider _cntrTypeProvider;
    public readonly IDocumentProvider _documentProvider;

    public UploadFileController(IExportOrderProvider exportOrderProvider, ICntrTypeProvider cntrTypeProvider, IDocumentProvider documentProvider)
    {
        _exportOrderProvider = exportOrderProvider;
        _cntrTypeProvider = cntrTypeProvider;
        _documentProvider = documentProvider;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }


    #region Check upload
    [HttpPost]
    [Route("CheckExpOrderRecords/{eoId}")]
    //public async Task<UploadedResult> CheckExpOrderRecords([FromForm] IEnumerable<IFormFile> files, long eoId, ExportOrderEntity expOrder)
    //{
    //    string filePath = string.Empty;

    //    var items = new List<ExportOrderRecord>();

    //    var cntrTypes = await _cntrTypeProvider.GetCntrTypes();
    //    var documents = await _documentProvider.GetDocumentsAsync();
    //    //var exportOrder = new ExportOrderEntity();

    //    foreach (var file in files)
    //        if (file != null)
    //        {
    //            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
    //            filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);

    //            using (var stream = new FileStream(filePath, FileMode.Create))
    //            {
    //                file.CopyTo(stream);
    //            }
    //        }

    //    using (var exl = new ExcelService(filePath, cntrTypes, _documentProvider))
    //    {
    //        var uploadingRecords = await exl.ReadUploadingFile();
    //     //   items.AddRange(uploadingRecords);
    //    }

    //    //items = items.DistinctBy(s => s.Id).ToList();

    //    #region CHECK Entity before Upload

    //    var errList = new List<string>();

    //    foreach (var item in items)
    //    {
    //        // Empties:

    //        if (item.CntrTareWt! > 0)
    //            errList.Add($"Cntr: {item.CntrNum} - Tare weight is empty.");

    //        if (string.IsNullOrWhiteSpace(item.Seal))
    //            errList.Add($"Cntr: {item.CntrNum} - Seal is empty.");

    //        // Content records:

    //        if (item.Contents.Count() > 0)
    //            foreach (var content in item.Contents)
    //            {
    //                // Empties:

    //                if (content.Quantity! > 0)
    //                    errList.Add($"Cntr: {item.CntrNum} - Pakage Quantity is empty.");

    //                if (content.GrossWt! > 0)
    //                    errList.Add($"Cntr: {item.CntrNum} - Gross weight is empty.");
    //                else
    //                    if (content.GrossWt > 29000)
    //                    errList.Add($"Cntr: {item.CntrNum} - Gross weight exceeded.");

    //                if (content.NetWt! > 0)
    //                    errList.Add($"Cntr: {item.CntrNum} - Net weight is empty.");
    //                else
    //                    if (content.NetWt > 29000)
    //                    errList.Add($"Cntr: {item.CntrNum} - Netto weight exceeded.");




    //            }
    //        else
    //            errList.Add($"List of Content to upload is empty.");
    //    }

    //    #endregion

    //    var UploadedResults = new UploadedResult()
    //    {
    //        CntrCount = items.Count(),
    //        CntrContentCount = items.SelectMany(s => s.Contents).ToList().DistinctBy(x => x.DocumentRecord?.Id).Count(),
    //        Errors = errList,
    //    };

    //    return UploadedResults;
    //}
    #endregion

    #region Old uploader
    [HttpPost]
    [Route("UploadExpOrderRecords")]
    //public async Task<IEnumerable<ExportOrderRecord>> UploadExpOrderRecords([FromForm] IEnumerable<IFormFile> files)
    //{
    //    string filePath = string.Empty;

    //    var items = new List<ExportOrderRecord>();

    //    var ExportOrderRecords = new List<ExportOrderRecord>();
    //    //var documents = new List<DocumentEntity>();


    //    var cntrTypes = await _cntrTypeProvider.GetCntrTypes();
    //    var documents = await _documentProvider.GetDocumentsAsync();
    //    //var exportOrder = new ExportOrderEntity();

    //    //exportOrder.Id = eoId;


    //    //var eoAppResponse = await _exportOrderProvider.GetItemAsync(eoId);
    //    //if (!eoAppResponse.HasError)
    //    //    exportOrder = (ExportOrderEntity)eoAppResponse.Object!;

    //    foreach (var file in files)
    //        if (file != null)
    //        {
    //            //string dirName = Path.Combine(Environment.SpecialFolder.Resources.ToString(), "Temp");
    //            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
    //            filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);

    //            using (var stream = new FileStream(filePath, FileMode.Create))
    //            {
    //                file.CopyTo(stream);
    //            }
    //        }

    //    using (var exl = new ExcelService(filePath, cntrTypes, documents))
    //    {
    //        var uploadingRecords = exl.ReadUploadingFile();
    //        items.AddRange(uploadingRecords);
    //    }

    //    items = items.DistinctBy(s => s.Id).ToList();

    //    //var appresponse = await _exportOrderProvider.AddUploadedFileItemsAsync(items);

    //    //if (!appresponse.HasError)
    //    //{
    //    //    return new UploadedResult()
    //    //    {
    //    //        CntrCount = items.Count(),
    //    //        CntrContentCount = items.SelectMany(s => s.Contents).ToList().DistinctBy(x => x.DocumentRecord?.Id).Count()
    //    //    };
    //    //}
    //    //else
    //    //{
    //    //    return new UploadedResult()
    //    //    {
    //    //        Errors = appresponse.Error.ToArray(),
    //    //    };
    //    //}

    //    return items;
    //}

    #endregion

    [HttpPost]
    [Route("UploadExportOrder")]
    public async Task<UploadResult> UploadExportOrder([FromForm] IEnumerable<IFormFile> files)
    {
        string filePath = string.Empty;

        var cntrTypes = await _cntrTypeProvider.GetCntrTypes();
                
        foreach (var file in files)
            if (file != null)
            {
                //string dirName = Path.Combine(Environment.SpecialFolder.Resources.ToString(), "Temp");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }

        using (var exl = new ExcelService(filePath, cntrTypes, _documentProvider))
        {
            try
            {            
                var result = await exl.ReadUploadingFile();

                //var ur = new UploadResult()
                //{
                //    _ExportOrderRecords = result.Item1.ToList(),
                //    _Docuemnts = result.Item2.ToList(),
                //};

                return result;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return new UploadResult();
            }
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

public class UploadResult
{
    public IEnumerable<ExportOrderRecord>? _ExportOrderRecords { get; set; }
    public IEnumerable<DocumentEntity>? _Docuemnts { get; set; }

    public IEnumerable<string> Summary { get; set; } = new List<string>();
}
