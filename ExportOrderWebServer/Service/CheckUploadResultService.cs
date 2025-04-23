using ExportOrderWebServer.Areas.SupplementaryUnit.Provider;

namespace ExportOrderWebServer.Service;

public interface ICheckUploadResultService : IDisposable
{
    Task<IEnumerable<string>> CheckUploadedResult(List<UploadExcelDTO> uploadResult, long eoId, long voyageId);
}

public class CheckUploadResultService : ICheckUploadResultService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;
    private readonly IValidationService _validationService;
    private readonly ISupplementaryUnitProvider _supplementaryUnitProvider;

    public CheckUploadResultService(IDbContextFactory<ApplicationDbContext> dbContext,
                                        IValidationService validationService,
                                        ISupplementaryUnitProvider supplementaryUnitProvider)
    {
        _dbContext = dbContext;
        _validationService = validationService;
        _supplementaryUnitProvider = supplementaryUnitProvider;
    }

    public async Task<IEnumerable<string>> CheckUploadedResult(List<UploadExcelDTO> uploadResult, long eoId, long voyageId)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            List<string> errList = new();
                        
            var Documents = await db.Documents.Include(d => d.Records).AsNoTracking().ToListAsync();

            var CntrTypes = await db.ContainerTypeSize.AsNoTracking().ToListAsync();

            /// CONTAINERS ONLY
            var uploadedCntrNums = uploadResult.Where(s => !string.IsNullOrWhiteSpace(s.CntrNum)).GroupBy(s => s.CntrNum).Select(g => g.Key).ToArray();

            if (uploadedCntrNums is not null && uploadedCntrNums != Array.Empty<string>())
            {
                var cntrNumErros = await _validationService.ValidateCntrNums(uploadedCntrNums!, eoId, voyageId, null);

                foreach (string err in cntrNumErros)
                    errList.Add(err.Insert(0, "---:"));
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
                    string type = record.CntrType.Substring(2);

                    switch (type)
                    {
                        case "DС": // en D + ru С
                            record.CntrType = record.CntrType.Replace("DС", "DC");
                            break;

                        case "НС": // ru Н + ru С
                            record.CntrType = record.CntrType.Replace("НС", "HC");
                            break;
                        case "НC": // ru Н + en С
                            record.CntrType = record.CntrType.Replace("НC", "HC");
                            break;
                        case "HС": // en Н + ru С
                            record.CntrType = record.CntrType.Replace("HС", "HC");
                            break;

                        case "ТК": // ru Т + ru К
                            record.CntrType = record.CntrType.Replace("ТК", "TK");
                            break;
                        case "ТK": // ru Т + en K
                            record.CntrType = record.CntrType.Replace("Т", "T");
                            break;
                        case "TК": // en T + ru К
                            record.CntrType = record.CntrType.Replace("К", "K");
                            break;

                        case "ОТ": // ru О + ru Т
                            record.CntrType = record.CntrType.Replace("ОТ", "OT");
                            break;
                        case "ОT": // ru О + en T
                            record.CntrType = record.CntrType.Replace("ОT", "OT");
                            break;
                        case "OТ": // en O + ru Т
                            record.CntrType = record.CntrType.Replace("OТ", "OT");
                            break;
                    }

                    if (!CntrTypes.Any(ct => ct.Normolize!.Equals(record.CntrType)))
                        errList.Add($"{errMessagePrefix} Cntr Type '{record.CntrType}' dosn't exist in a DataBase.");
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

                var existedDocument = Documents.Where(d => d.Name == record.DocumentName).FirstOrDefault();
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
                                bool isCodeExists = await _supplementaryUnitProvider.IsSupplementaryUnitExists(record.SupplementaryUnitCode.Value);
                                if (!isCodeExists)
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
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}