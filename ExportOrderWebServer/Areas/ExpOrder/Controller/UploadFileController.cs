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
    [Route("CheckExpOrderRecords")]
    public async Task<CheckingResult> CheckExpOrderRecords([FromForm] IEnumerable<IFormFile> files)
    {
        string filePath = string.Empty;

        var cntrTypes = await _cntrTypeProvider.GetCntrTypes();

        foreach (var file in files)
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }

        using (var exl = new _ExcelUploadService(filePath, cntrTypes, _documentProvider))
        {
            var result = await exl._ReadUploadingFile();

            var errList = new List<string>();

            // Good
            if (result.Item1.Select(eor => eor.Contents.Any(co => co.DocumentRecord == null)).FirstOrDefault() == true)
            {
                var docNums = result.Item1.SelectMany(eor => eor.Contents).Where(co => co.DocumentRecord == null).Select(co => co.DocumentRecord.Document.Name).ToHashSet();
                foreach (var doc in docNums)
                {
                    errList.Add($"В декларации: {doc} нет товара, который вы пытаетесь подгрузить.");
                }
            }

            // Document
            var Documents = new List<DocumentEntity>();
            var responseDocuments = await _documentProvider.GetItemsAsync();
            if (!responseDocuments.HasError)
            {
                Documents = ((List<DocumentEntity>)responseDocuments.Object!).ToList();

                foreach (var doc in Documents)
                    if (!result.Item2.Any(d => d.Name == doc.Name))
                        errList.Add($"Document: <b>{doc.Name}<b> dosn't exists in DataBase.");
            }

            // Container Num
            var duplicateCntrNum = result.Item1.GroupBy(eor => eor.CntrNum).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

            if (duplicateCntrNum.Count() > 0)
                foreach (var num in duplicateCntrNum)
                    errList.Add($"Cntr: {num} duplicates.");

            foreach (var record in result.Item1)
            {
                if (record.Contents.Count() > 0)
                    foreach (var content in record.Contents)
                    {
                        if (content.GrossWt > 29000)
                            errList.Add($"Cntr: {record.CntrNum} - Gross weight exceeded.");

                        if (content.NetWt! > 29000)
                            errList.Add($"Cntr: {record.CntrNum} - Net weight is empty.");
                    }
                else
                    errList.Add($"Cntr: {record.CntrNum} - list of Container Content is empty.");
            }

            var CheckingResults = new CheckingResult()
            {
                CntrCount = result.Item1.Count(),
                DocumentCount = result.Item2.Count(),
                Errors = errList,
            };

            return CheckingResults;
        }
    }

    [HttpGet]
    [Route("CheckExpOrderRecords/{voyageId}")]
    public async Task<CheckingResult> CheckExpOrderRecords([FromForm] IEnumerable<IFormFile> files, long voyageId)
    {
        string filePath = string.Empty;

        var cntrTypes = await _cntrTypeProvider.GetCntrTypes();
        var cntrNums = await _vesselCallProvider.GetExportOrderNumsAsync(voyageId);
        var _cntrNums = await _vesselCallProvider.GetItemToCheckAsync(voyageId);

        foreach (var file in files)
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }

        using (var exl = new _ExcelUploadService(filePath, cntrTypes, _documentProvider))
        {
            var result = await exl._ReadUploadingFile();

            var errList = new List<string>();

            // Good
            if (result.Item1.Select(eor => eor.Contents.Any(co => co.DocumentRecord == null)).FirstOrDefault() == true)
            {
                var docNums = result.Item1.SelectMany(eor => eor.Contents).Where(co => co.DocumentRecord == null).Select(co => co.DocumentRecord.Document.Name).ToHashSet();
                foreach (var doc in docNums)
                {
                    errList.Add($"В декларации: {doc} нет товара, который вы пытаетесь подгрузить.");
                }
            }

            // Document
            var Documents = new List<DocumentEntity>();
            var responseDocuments = await _documentProvider.GetItemsAsync();
            if (!responseDocuments.HasError)
            {
                Documents = ((List<DocumentEntity>)responseDocuments.Object!).ToList();

                foreach (var doc in result.Item2)
                    if (!Documents.Any(d => d.Name == doc.Name))
                        errList.Add($"Document: <b>{doc.Name}<b> dosn't exists in DataBase.");
            }

            // Container Num
            var duplicateCntrNum = new List<string>();

            if (_cntrNums is not null)
            {
                //duplicateCntrNum = _cntrNums.Details.SelectMany(vcd => vcd.ExportOrders).GroupBy(eo => eo.Num).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                var _duplicateCntrNum = _cntrNums.Details.SelectMany(vcd => vcd.ExportOrders).GroupBy(eo => eo.Num).Where(g => g.Count() > 1).ToList();

                if (_duplicateCntrNum.Count() > 0)
                    foreach (var num in _duplicateCntrNum)
                        errList.Add($"Cntr: {num.Select(g => g.Num)} duplicates in Voyage: {num.Select(g => g.VesselCallDetail!.VesselCall.VoyageNo)}.");
            }

            foreach (var record in result.Item1)
            {
                if (record.Contents.Count() > 0)
                    foreach (var content in record.Contents)
                    {
                        // Empties:
                        //if (content.PackageQty <= 0)
                        //    errList.Add($"Cntr: {record.CntrNum} - Pakage Quantity is empty.");

                        if (content.GrossWt > 29000)
                            errList.Add($"Cntr: {record.CntrNum} - Gross weight exceeded.");

                        if (content.NetWt! > 29000)
                            errList.Add($"Cntr: {record.CntrNum} - Net weight is empty.");
                    }
                else
                    errList.Add($"Cntr: {record.CntrNum} - list of Container Content is empty.");
            }

            var CheckingResults = new CheckingResult()
            {
                CntrCount = result.Item1.Count(),
                DocumentCount = result.Item2.Count(),
                //CntrContentCount = items.SelectMany(s => s.Contents).ToList().DistinctBy(x => x.DocumentRecord?.Id).Count(),
                Errors = errList,
            };

            return CheckingResults;
        }
    }

    [HttpPost]
    [Route("UploadExportOrder")]
    public async Task<UploadResult> UploadExportOrder([FromForm] IEnumerable<IFormFile> files)
    {
        string filePath = string.Empty;        

        var cntrTypes = await _cntrTypeProvider.GetCntrTypes();

        foreach (var file in files)
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                //filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);
                filePath = Path.Combine(_webHostEnvironment.WebRootPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }

        using (var exl = new _ExcelUploadService(filePath, cntrTypes, _documentProvider))
        {
            try
            {
                var result = await exl._ReadUploadingFile();

                var ur = new UploadResult()
                {
                    _ExportOrderRecords = result.Item1.ToList(),
                    _Documents = result.Item2.ToList(),
                };

                return ur;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return new UploadResult() { Summary = new List<string>() { msg } };
            }
        }
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
}

//public class UploadResult
//{
//    public IEnumerable<ExportOrderRecord>? _ExportOrderRecords { get; set; }
//    public IEnumerable<DocumentEntity>? _Documents { get; set; }
//    public IEnumerable<string>? Summary { get; set; } = new List<string>();
//    public IEnumerable<string>? Errors { get; set; } = new List<string>();
//}

//public class CheckingResult
//{
//    public int CntrCount { get; set; }
//    //public int CntrContentCount { get; set; }
//    public int DocumentCount { get; set; }
//    public IEnumerable<string> Errors { get; set; } = new List<string>();
//    public bool HasErrors => Errors.Count() > 0;
//    //public IEnumerable<string> ErrorsCntrList { get; set; } = new List<string>();
//    //public bool HasErrorsCntrList => ErrorsCntrList.Count() > 0;
//}