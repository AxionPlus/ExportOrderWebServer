using System.Text.RegularExpressions;

namespace ExportOrderWebServer.Service;

public class UploadedResult
{
    public List<ExportOrderRecord> Records { get; set; } = new();
    public List<DocumentEntity> Documents { get; set; } = new();
}

public interface IUploadResultService : IDisposable
{
    Task<IEnumerable<string>> CheckUploadedResult(List<UploadExcelDTO> uploadResult, long eoId, long voyageId);
    Task<UploadedResult?> GetUploadedResult(List<UploadExcelDTO> uploaded);
}

public class UploadResultService : IUploadResultService
{
    private readonly ICntrTypeProvider _cntrTypeProvider;
    private readonly IDocumentProvider _documentProvider;
    public readonly IExportOrderProvider _exportOrderProvider;
    //private readonly IValidationService _validationService; // DELETE and replace with: using service
    private readonly ISupplementaryUnitProvider _supplementaryUnitProvider;

    public UploadResultService(ICntrTypeProvider cntrTypeProvider,
                                IDocumentProvider documentProvider,
                                IExportOrderProvider exportOrderProvider,
                                //IValidationService validationService,
                                ISupplementaryUnitProvider supplementaryUnitProvider)
    {
        _cntrTypeProvider = cntrTypeProvider;
        _documentProvider = documentProvider;
        _exportOrderProvider = exportOrderProvider;
        //_validationService = validationService;
        _supplementaryUnitProvider = supplementaryUnitProvider;
    }

    public async Task<IEnumerable<string>> CheckUploadedResult(List<UploadExcelDTO> uploadResult, long eoId, long voyageId)
    {
        List<string> errList = new();

        var dbCntrTypes = await _cntrTypeProvider.GetCntrTypes();
        var dbSupplementaryUnitCodes = await _supplementaryUnitProvider.GetUnitCodesAsync();

        string[]? uploadedDocumentNames = uploadResult.Where(s => !string.IsNullOrWhiteSpace(s.DocumentName)).Select(s => s.DocumentName!).Distinct().ToArray();
        var dbDocuments = await _documentProvider.GetItemsAsync(uploadedDocumentNames);

        /// CONTAINERS ONLY
        var uploadedCntrNums = uploadResult.Where(s => !string.IsNullOrWhiteSpace(s.CntrNum)).GroupBy(s => s.CntrNum).Select(g => g.Key).ToArray();

        if (uploadedCntrNums is not null && uploadedCntrNums != Array.Empty<string>())
        {
            //using IValidationService validationService = new ValidationService(_exportOrderProvider);

            //var cntrNumErros = await validationService.ValidateCntrNums(uploadedCntrNums!, eoId, voyageId, null);

            //foreach (string err in cntrNumErros)
            //    errList.Add(err.Insert(0, "---:"));

            using (IValidationService validationService = new ValidationService(_exportOrderProvider))
            {
                var cntrNumErros = await validationService.ValidateCntrNums(uploadedCntrNums!, eoId, voyageId, null);

                foreach (string err in cntrNumErros)
                    errList.Add(err.Insert(0, "---:"));
            }
        }

        /// All RECORDS
        foreach (var record in uploadResult)
        {
            string errMessagePrefix = $"{record.DocumentName}:{record.CntrNum}:";

            /// CONTAINER TYPE
            if (string.IsNullOrEmpty(record.CntrType))
                errList.Add($"{errMessagePrefix} Пропущен тип Контейнера.");
            else
            {
                string type = record.CntrType[2..];

                #region Cyrillic type
                //switch (type)
                //{
                //    case "DС": // en D + ru С
                //        record.CntrType = record.CntrType.Replace("DС", "DC");
                //        break;

                //    case "НС": // ru Н + ru С
                //        record.CntrType = record.CntrType.Replace("НС", "HC");
                //        break;
                //    case "НC": // ru Н + en С
                //        record.CntrType = record.CntrType.Replace("НC", "HC");
                //        break;
                //    case "HС": // en Н + ru С
                //        record.CntrType = record.CntrType.Replace("HС", "HC");
                //        break;

                //    case "ТК": // ru Т + ru К
                //        record.CntrType = record.CntrType.Replace("ТК", "TK");
                //        break;
                //    case "ТK": // ru Т + en K
                //        record.CntrType = record.CntrType.Replace("Т", "T");
                //        break;
                //    case "TК": // en T + ru К
                //        record.CntrType = record.CntrType.Replace("К", "K");
                //        break;

                //    case "ОТ": // ru О + ru Т
                //        record.CntrType = record.CntrType.Replace("ОТ", "OT");
                //        break;
                //    case "ОT": // ru О + en T
                //        record.CntrType = record.CntrType.Replace("ОT", "OT");
                //        break;
                //    case "OТ": // en O + ru Т
                //        record.CntrType = record.CntrType.Replace("OТ", "OT");
                //        break;
                //}
                #endregion

                if (Regex.IsMatch(type, @"\p{IsCyrillic}"))
                    errList.Add($"{errMessagePrefix} Cntr Type is Cyrillic.");

                if (!dbCntrTypes.Any(ct => ct.Normolize!.Equals(record.CntrType)))
                    errList.Add($"{errMessagePrefix} Cntr Type dosn't exist in a System.");
            }

            /// WEIGHTS
            if (record.CntrTareWt == 0)
                errList.Add($"{errMessagePrefix} CntrTare is 0.");

            if (record.GrossWt > 29000)
                errList.Add($"{errMessagePrefix} Gross weight exceeded.");

            if (record.NetWt! > 29000)
                errList.Add($"{errMessagePrefix} Net weight exceeded.");

            if (record.GrossWt < record.NetWt)
                errList.Add($"{errMessagePrefix} Net weight exceeds Gross.");

            /// DOCUMENT
            if (string.IsNullOrEmpty(record.DocumentName))
                errList.Add($"{errMessagePrefix} *Critical - Пропущен номер Декларации.");

            var existedDocument = dbDocuments?.FirstOrDefault(d => d.Name == record.DocumentName);

            if (existedDocument is not null)
            {
                /// Container Content
                if (existedDocument.Records.Any(r => r.CommodityEngName.ToUpper().Contains("EMPTY")))
                {
                    if (record.PackageQty > 1) errList.Add($"{errMessagePrefix} Package Quantity exceeded for Empty container.");
                    if (record.PackageName != null) errList.Add($"{errMessagePrefix} Package Name indicated for Empty container.");
                    if (record.NetWt > 0) errList.Add($"{errMessagePrefix} Netto weight exceeds 0 for Empty container.");
                    if (record.GrossWt > 0) errList.Add($"{errMessagePrefix} Gross weight exceeds 0 for Empty container.");
                }
                else
                {
                    if (record.NetWt <= 0) errList.Add($"{errMessagePrefix} Netto weight is 0.");
                    if (record.GrossWt <= 0) errList.Add($"{errMessagePrefix} Gross weight is 0.");

                    if (!record.SupplementaryUnitCode.HasValue && record.SupplementaryUnitQuantity.HasValue)
                        errList.Add($"{errMessagePrefix} Supplementary Code is missed.");

                    if (record.SupplementaryUnitCode.HasValue)
                        if (!record.SupplementaryUnitQuantity.HasValue)
                            errList.Add($"{errMessagePrefix} Supplementary Quantity is missed.");
                        else
                        {
                            if (dbSupplementaryUnitCodes is not null && dbSupplementaryUnitCodes.Any())
                                if (!dbSupplementaryUnitCodes.Any(s => s == record.SupplementaryUnitCode.Value))
                                    errList.Add($"{errMessagePrefix} Supplementary Unit not found in the System.");
                        }
                }

                /// Cargo
                if (record.SeqCommodity == 0)
                    errList.Add($"{errMessagePrefix} *Critical - Отсутствует номер товара.");
                else
                    if (!existedDocument.Records.Any(dr => dr.Seq.Equals(record.SeqCommodity)))
                    errList.Add($"{errMessagePrefix} *Critical - в ДТ нет товара с таким номером.");
            }
            else
                errList.Add($"{errMessagePrefix} *Critical - Декларации нет в базе данных.");
        }

        return errList;
    }

    public async Task<UploadedResult?> GetUploadedResult(List<UploadExcelDTO> uploadedDTO)
    {
        try
        {
            var CntrTypes = await _cntrTypeProvider.GetCntrTypes();
            var dbSupplementaryUnits = await _supplementaryUnitProvider.GetItemsAsync();

            string[]? uploadedDocumentNames = uploadedDTO.Where(s => !string.IsNullOrWhiteSpace(s.DocumentName))
                                                         .GroupBy(s => s.DocumentName!)
                                                         .Select(g => g.Key).ToArray();

            var dbDocuments = await _documentProvider.GetItemsAsync(uploadedDocumentNames);
            if (dbDocuments is null || !dbDocuments.Any())
                return null;

            var uploadedRecords = uploadedDTO.GroupBy(s => s.CntrNum);
            var uploadedDocuments = uploadedDTO.GroupBy(s => s.DocumentName);

            var records = new List<ExportOrderRecord>();
            var documents = new List<DocumentEntity>();

            /// RECORDS
            foreach (var uploadedRecord in uploadedRecords)
            {
                var newRecord = new ExportOrderRecord()
                {
                    CntrNum = uploadedRecord.Any(s => !string.IsNullOrWhiteSpace(s.CntrNum)) ?
                                uploadedRecord.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.CntrNum))!.CntrNum! : "",
                    CntrType = CntrTypes.FirstOrDefault(x => x.Normolize == uploadedRecord.FirstOrDefault()!.CntrType),
                    CntrTareWt = uploadedRecord.FirstOrDefault()!.CntrTareWt,
                    Seal = uploadedRecord.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.Seal))?.Seal,
                };

                foreach (var uploadedContent in uploadedRecord)
                {
                    /// Get DocumentRecord
                    var dbDocumentRecord = dbDocuments.Where(s => !string.IsNullOrWhiteSpace(s.Name) && s.Name == uploadedContent.DocumentName)
                                                      .SelectMany(d => d.Records).FirstOrDefault(dr => dr.Seq == uploadedContent.SeqCommodity);

                    if (dbDocumentRecord is null)
                        continue;

                    var content = new ContainerContent()
                    {
                        PackageQty = uploadedContent.PackageQty,
                        PackageName = uploadedContent.PackageName,
                        NetWt = uploadedContent.NetWt,
                        GrossWt = uploadedContent.GrossWt,
                        DocumentRecord = dbDocumentRecord
                    };

                    /// GetSupplementaryUnit if any
                    if (dbSupplementaryUnits is not null && uploadedContent.SupplementaryUnitCode.HasValue && uploadedContent.SupplementaryUnitQuantity.HasValue)
                    {
                        var dbSupplementaryUnit = dbSupplementaryUnits.FirstOrDefault(s => s.Code == uploadedContent.SupplementaryUnitCode.Value);
                        if (dbSupplementaryUnit is not null)
                        {
                            content.SupplementaryUnit = dbSupplementaryUnit;
                            content.SupplementaryUnitQuantity = uploadedContent.SupplementaryUnitQuantity;
                        }
                    }

                    newRecord.Contents.Add(content);
                }

                if (newRecord.Contents.Any())
                    records.Add(newRecord);

                newRecord = null;
            }

            /// DOCUMENTS
            foreach (var dbDocument in dbDocuments)
            {
                var uploadedDocument = uploadedDocuments.FirstOrDefault(g => g.Key == dbDocument.Name);

                /// remove dbRecord that dosn't exists in the Checked one
                foreach (var dbDocumentRecord in dbDocument.Records.ToArray())
                    if (uploadedDocument is not null && !uploadedDocument.Any(d => d.SeqCommodity == dbDocumentRecord.Seq))
                        dbDocument.Records.Remove(dbDocumentRecord);

                documents.Add(dbDocument);
            }            

            /// RESULT
            if (!records.Any() || !documents.Any())
                return null;

            return new UploadedResult
            {
                Records = records,
                Documents = documents
            };
        }
        catch (Exception ex)
        { 
            Console.WriteLine(ex.Message); return null;
        }        
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

