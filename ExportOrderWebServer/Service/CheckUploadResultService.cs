namespace ExportOrderWebServer.Service;

public class CheckUploadResultService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    public CheckUploadResultService(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<string>> CheckingResult(List<UploadExcelDTO> result, long voyageId)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var errList = new List<string>();

            var VesselCall = await db.VesselCalls.Include(vc => vc.Details)
                                                 .ThenInclude(vcd => vcd.ExportOrders).ThenInclude(eo => eo.Records).ThenInclude(eor => eor.Contents)
                                                 .AsNoTracking().FirstOrDefaultAsync(s => s.Id == voyageId);

            var Documents = await db.Documents.Include(d => d.Records).AsNoTracking().ToListAsync();

            var CntrTypes = await db.ContainerTypeSize.AsNoTracking().ToListAsync();

            foreach (var record in result)
            {
                string errMessagePrefix = $"{record.DocumentName};{record.CntrNum};";

                // CNTR TYPE
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

                // Container Num DUPLICATION in Voyage
                if (VesselCall is not null)
                    if (VesselCall.Details.SelectMany(vcd => vcd.ExportOrders).SelectMany(eo => eo.Records).Any(eor => eor.CntrNum.Equals(record.CntrNum)))
                        errList.Add($"{errMessagePrefix} Cntr duplicates in: {VesselCall.Details.SelectMany(vcd => vcd.ExportOrders).FirstOrDefault(eo => eo.Records.Any(eor => eor.CntrNum.Equals(record.CntrNum)))!.Num}.");

                // CONTAINER NUM CONTROL DIGIT

                if (record.CntrNum is not null)
                {
                    string num = record.CntrNum;

                    // Chars in CntrNum
                    char[] chars = new char[26]
                    {
                        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
                    };

                    // digital equivalents:
                    int[] ControlEquivalents = new int[26]
                    {                                
                        10, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 23, 24, 25, 26, 27, 28, 29,30, 31, 32, 34, 35, 36, 37, 38
                    };

                    //int equivalentIndex1 = Array.IndexOf(ControlEquivalents, record.CntrNum.Substring(0, 1));
                    int DigitalEquivalent1 = ControlEquivalents[Array.IndexOf(chars, Convert.ToChar(num.Substring(0, 1)))] * (int)Math.Pow(2, 0);
                    int DigitalEquivalent2 = ControlEquivalents[Array.IndexOf(chars, Convert.ToChar(num.Substring(1, 1)))] * (int)Math.Pow(2, 1);
                    int DigitalEquivalent3 = ControlEquivalents[Array.IndexOf(chars, Convert.ToChar(num.Substring(2, 1)))] * (int)Math.Pow(2, 2);
                    int DigitalEquivalent4 = ControlEquivalents[Array.IndexOf(chars, Convert.ToChar(num.Substring(3, 1)))] * (int)Math.Pow(2, 3);

                    // Digits in CntrNum
                    int digit1 = Convert.ToInt32(num.Substring(4, 1)) * (int)Math.Pow(2, 4);
                    int digit2 = Convert.ToInt32(num.Substring(5, 1)) * (int)Math.Pow(2, 5);
                    int digit3 = Convert.ToInt32(num.Substring(6, 1)) * (int)Math.Pow(2, 6);
                    int digit4 = Convert.ToInt32(num.Substring(7, 1)) * (int)Math.Pow(2, 7);
                    int digit5 = Convert.ToInt32(num.Substring(8, 1)) * (int)Math.Pow(2, 8);
                    int digit6 = Convert.ToInt32(num.Substring(9, 1)) * (int)Math.Pow(2, 9);

                    // sum of ControlEquivalents multiplications (Characters only)
                    int multiplicationOfChars = DigitalEquivalent1 + DigitalEquivalent2 + DigitalEquivalent3 + DigitalEquivalent4;

                    // sum of Serial numbers multiplications (Digits only)
                    int multiplicationOfDigits = digit1 + digit2 + digit3 + digit4 + digit5 + digit6;

                    // Остаток от деления на 11 по модулю
                    int Remainder = (multiplicationOfChars + multiplicationOfDigits) % 11;

                    if (Remainder == 10) Remainder = 0;
                    //int Remainder;
                    //int div = Math.DivRem((multiplicationOfChars + multiplicationOfDigits), 11, out Remainder);

                    if (Convert.ToInt32(record.CntrNum.Substring(10, 1)) != Remainder)
                        errList.Add($"{errMessagePrefix} Контрольная цифра в номере контейнера не верна. Правитльно - {Remainder}.");
                }            
            }
        
            return errList;
        }
    }

    //public void Dispose()
    //{
    //    //throw new NotImplementedException();
    //}
}
