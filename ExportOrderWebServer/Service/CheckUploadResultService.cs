namespace ExportOrderWebServer.Service;

public class CheckUploadResultService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;
    private readonly CheckCntrNumService _checkCntrNumService;

    public CheckUploadResultService(IDbContextFactory<ApplicationDbContext> dbContext, CheckCntrNumService checkCntrNumService)
    {
        _dbContext = dbContext;
        _checkCntrNumService = checkCntrNumService;
    }

    public async Task<List<string>> CheckingResult(List<UploadExcelDTO> result, long voyageId)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var errList = new List<string>();
                        
            var Documents = await db.Documents.Include(d => d.Records).AsNoTracking().ToListAsync();

            var CntrTypes = await db.ContainerTypeSize.AsNoTracking().ToListAsync();

            foreach (var record in result)
            {
                string errMessagePrefix = $"{record.DocumentName};{record.CntrNum};";

                // WEIGHTS
                if (record.CntrTareWt == 0)
                    errList.Add($"{errMessagePrefix} CntrTare is 0.");

                if (record.GrossWt > 29000)
                    errList.Add($"{errMessagePrefix} Gross weight exceeded.");

                if (record.NetWt! > 29000)
                    errList.Add($"{errMessagePrefix} Net weight exceeded.");

                if (record.GrossWt < record.NetWt)
                    errList.Add($"{errMessagePrefix} Net weight exceeds Gross.");

                // DOCUMENT
                if (string.IsNullOrEmpty(record.DocumentName))
                    errList.Add($"{errMessagePrefix} Number of Declaration is missed.");

                var existedDocument = Documents.Where(d => d.Name == record.DocumentName).FirstOrDefault();
                if (existedDocument is not null)
                {
                    // Container Content
                    if (existedDocument.Records.Any(r => r.CommodityEngName.ToUpper().Contains("EMPTY")))
                    {
                        if (record.PackageQty > 1) errList.Add($"{errMessagePrefix} Package Quantity exceeded for Empty container.");
                        if (record.PackageName != null) errList.Add($"{errMessagePrefix} Package Name indicated for Empty container.");
                        if (record.NetWt > 0) errList.Add($"{errMessagePrefix} Netto weight exceeds 0 for Empty container.");
                        if (record.GrossWt > 0) errList.Add($"{errMessagePrefix} Gross weight exceeds 0 for Empty container.");
                    }
                    else
                    {
                        //if (record.PackageQty is null || record.PackageQty < 1) errList.Add($"{errMessagePrefix} - Package Quantity missed");
                        //if (string.IsNullOrEmpty(record.PackageName)) errList.Add($"{errMessagePrefix} - Package Name missed");
                        if (record.NetWt <= 0) errList.Add($"{errMessagePrefix} Netto weight is 0.");
                        if (record.GrossWt <= 0) errList.Add($"{errMessagePrefix} Gross weight is 0.");
                    }

                    // Cargo
                    if (record.SeqCommodity == 0)
                        errList.Add($"{errMessagePrefix} Отсутствует номер товара.");
                    else
                        if (!existedDocument.Records.Any(dr => dr.Seq.Equals(record.SeqCommodity)))
                        errList.Add($"{errMessagePrefix} В декларации нет товара.");
                }
                else
                    errList.Add($"{errMessagePrefix} Декларации нет в базе данных.");

                // CONTAINER

                // --- Type
                if (string.IsNullOrEmpty(record.CntrType))
                    errList.Add($"{errMessagePrefix} Cntr Type is missed.");
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
                        errList.Add($"{errMessagePrefix} Cntr Type '{record.CntrType}' dosn't exist in DataBase.");
                }

                if (record.CntrNum is not null && record.CntrNum.Length == 11)
                {
                    // --- Num DUPLICATION in Voyage
                    bool response = await _checkCntrNumService.IsCntrNumDuplicates(record.CntrNum, voyageId);

                    if (response)
                        errList.Add($"{errMessagePrefix} Контейнер повторяется в этом рейсе.");

                    // --- Num CONTROL DIGIT
                    int controlDigit = await _checkCntrNumService.ControlDigit(record.CntrNum);

                    if (Convert.ToInt32(record.CntrNum.Substring(10, 1)) != controlDigit)
                        errList.Add($"{errMessagePrefix} Контрольная цифра в номере контейнера не верна. Правильно - {controlDigit}.");
                }
                else
                    errList.Add($"{errMessagePrefix} Количество символов в номере контейнера не верно.");                
            }

            return errList;
        }
    }

    //public void Dispose()
    //{
    //    //throw new NotImplementedException();
    //}
}
