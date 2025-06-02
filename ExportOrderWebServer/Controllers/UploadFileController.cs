using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExportOrderWebServer.Controllers;

[AllowAnonymous]
[Route("file/[controller]")]
//[ApiController]
public class UploadFileController : ControllerBase
{
    private readonly IUploadResultService _UploadResultService;
    private readonly ICommodityProvider _CommodityProvider;
    private AppObjectResponse _AppObjectResponse = new();
    private readonly static string DirTemporary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TempFiles");

    public UploadFileController(IUploadResultService uploadResultService, ICommodityProvider commodityProvider)
    {
        _UploadResultService = uploadResultService;
        _CommodityProvider = commodityProvider;

        if (!Directory.Exists(DirTemporary))
            Directory.CreateDirectory(DirTemporary);        
    }


    [HttpPost, Route("UploadFromFileExcel")]    // file/UploadFileController/UploadFromFileExcel
    public async Task<List<ReadExcelExportOrderRecordDTO>?> UploadFromExcel([FromForm] IFormFile file) //AppObjectResponse?
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

    [HttpPost, Route("UploadFromFileXML")]    // file/UploadFileController/UploadFromFileXML
    public async Task<AppObjectResponse?> UploadFromXML([FromForm] IFormFile file, string DocumentName)
    {
        _AppObjectResponse = new();

        string filePath = Path.Combine(DirTemporary, string.Concat(Path.GetRandomFileName(), "_", Path.GetExtension(file.FileName)));
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream); //stream.Flush();        
        }            

        using (IXmlFileReadService xmlFileReadService = new XmlFileReadService())
        {
            var readResult = await xmlFileReadService.ReadXmlFileDocument(filePath);

            /// CHECK uploaded DATA
            if (readResult is not null && readResult.Any())
            {                
                var errors = await _UploadResultService.CheckUploadedDocumentRecords(readResult, DocumentName);
                if (errors is not null && errors.Any())
                    _AppObjectResponse.Errors.AddRange(errors);

                /// CREATE new Document Records                
                List<DocumentRecord> newDocumentRecords = new();

                List<CommodityCatalog> dbCommodities = new();
                var commodityResponse = await _CommodityProvider.GetItemsAsync();
                if (commodityResponse is not null && commodityResponse.Object is not null)
                    dbCommodities = (List<CommodityCatalog>)commodityResponse.Object;

                foreach (ReadXmlDocumentRecordDTO record in readResult.OrderBy(s => s.Seq))
                {
                    DocumentRecord newDocumentRecord = new()
                    {
                        Seq = record.Seq,
                        CommodityHSCode = record.CommodityHSCode ?? string.Empty,
                        CommodityName = record.CommodityName ?? string.Empty,
                        GrossWt = double.TryParse(record.GrossWt, out double _gw) ? _gw : 0,
                        NetWt = double.TryParse(record.NetWt, out double _nw) ? _nw : 0,
                    };

                    /// Commodity in English only
                    var dbCommodity = dbCommodities.FirstOrDefault(s => s.HSCode == record.CommodityHSCode && s.Name == record.CommodityName);
                    if (dbCommodity != null)
                        newDocumentRecord.CommodityEngName = dbCommodity.NameEn;

                    newDocumentRecords.Add(newDocumentRecord);
                }

                _AppObjectResponse.Object = newDocumentRecords;
                _AppObjectResponse.Description = $"Всего товаров: {newDocumentRecords.Count}";
            }
            else
                _AppObjectResponse.ErrorAdd("Xml file wasn't read.");
        }

        return _AppObjectResponse;
    }
}