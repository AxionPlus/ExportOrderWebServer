using ClosedXML.Excel;

namespace ExportOrderWebServer.Service.FileService;

public interface ICreateExcelFileService : IDisposable
{
    Task<byte[]> CreateExcelFileRolis(ExportOrderFileDto item);
    Task<byte[]> CreateExcelFileFillBill(ExportOrderFileDto[] items);
}

public class CreateExcelFileService : ICreateExcelFileService
{
    private readonly static string DirResources = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
    private string DirTemporary { get; set; } = string.Empty;
    private string FilePath { get; set; } = string.Empty;

    public CreateExcelFileService()
    {
        DirTemporary = Path.Combine(DirResources, "TempFiles", Path.GetRandomFileName());

        if (!Directory.Exists(DirTemporary))
            Directory.CreateDirectory(DirTemporary);
    }

    public async Task<byte[]> CreateExcelFileRolis(ExportOrderFileDto item)
    {
        try
        {
            CreateTemporaryFile("NutepRolis.xlsx");

            using var workbook = new XLWorkbook(FilePath);

            /// Получаем первый лист
            var worksheet = workbook.Worksheet(1);

            /// HEADER
            worksheet.Cell(2, 2).Value = string.Join(";\n", item.Shippers);
            worksheet.Cell(3, 2).Value = string.Join(";\n", item.Consignees);
            worksheet.Cell(4, 2).Value = string.Join(";\n", item.Consignees);
            worksheet.Cell(5, 2).Value = item.Num;
            worksheet.Cell(6, 4).Value = item.PersonPhone;

            /// TABLE
            int columns = 18;
            int rows = item.Records.Count;
            int startRow = 9;

            int row = startRow;
            foreach (var record in item.Records)
            {
                /// Форматирование строки по образцу первой строки
                if (row > startRow)
                {
                    for (int col = 1; col <= columns; col++)
                    {
                        var cell = worksheet.Cell(row, col);
                        cell.Style = worksheet.Cell(startRow, col).Style;
                    }
                }

                worksheet.Cell(row, 1).Value = record.ContainerNum;
                worksheet.Cell(row, 4).Value = record.Seal;
                worksheet.Cell(row, 5).Value = record.Commodity?.ToUpper() == "ПОРОЖНИЙ КОНТЕЙНЕР" ? "Порожний контейнер / Empty Container" : record.Commodity;
                worksheet.Cell(row, 6).Value = record.PackageQty;
                worksheet.Cell(row, 7).Value = record.IMO;
                worksheet.Cell(row, 8).Value = record.UNNO;
                worksheet.Cell(row, 9).Value = record.NetWt;
                worksheet.Cell(row, 10).Value = record.GrossWt;
                worksheet.Cell(row, 11).Value = record.CntrTareWt;
                worksheet.Cell(row, 12).Value = record.GrossAndTare;
                worksheet.Cell(row, 13).Value = record.SupplementaryUnitCode;
                worksheet.Cell(row, 14).Value = record.SupplementaryUnitQuantity;
                worksheet.Cell(row, 15).Value = record.HSCode;
                worksheet.Cell(row, 16).Value = record.DocumentName;
                worksheet.Cell(row, 17).Value = record.DocumentType;
                worksheet.Cell(row, 18).Value = record.SeqContent;

                ++row;
            }

            if (!string.IsNullOrWhiteSpace(item.CommodityShort))
            {
                worksheet.Cell(rows + 1, 1).Style.Font.Bold = true;
                worksheet.Cell(rows + 1, 1).Value = "Дополнительные сведения";
                worksheet.Cell(rows + 1, 2).Value = item.CommodityShort;
            }

            workbook.Save();

            byte[] buffer = await File.ReadAllBytesAsync(FilePath);

            return buffer;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
            return Array.Empty<byte>();
            throw new ApplicationException($"Не удалось создать Excel файл.", ex);
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
            return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Array.Empty<byte>();
        }
        //finally { Dispose(); }
    }

    public async Task<byte[]> CreateExcelFileFillBill(ExportOrderFileDto[] items)
    {
        try
        {
            CreateTemporaryFile("FillBill.xlsx");

            using var workbook = new XLWorkbook(FilePath);

            var carrierGroup = items.GroupBy(s => s.CarrierNameEn).ToList();

            /// Создаем новые листы для каждого Carrier, начиная со второго
            for (int i = 1; i < carrierGroup.Count; i++)
                workbook.Worksheet(0).CopyTo($"Лист {i + 1}");

            /// Заполняем листы данными
            int indexCarrier = 0;

            foreach (var carrier in carrierGroup)
            {
                ++indexCarrier;

                var item = carrier.Select(s => new
                {
                    s.CarrierNameEn,
                    s.VesselName,
                    s.VesselFlag,
                    s.CarrierCountryEn,
                    s.CarrierLocation,
                    s.CarrierContract,
                    s.CarrierContractDate,
                    s.CaptainFamily,
                    s.CaptainName,
                    s.BLDate,
                    s.POLEnCountryEn,
                    s.CustomsOfficeCode,
                }).FirstOrDefault();

                if (item is null) continue;

                /// Получаем текущий лист
                var worksheet = workbook.Worksheet(indexCarrier);

                worksheet.Name = item.CarrierNameEn ?? $"Лист {indexCarrier}";

                /// HEADER
                worksheet.Cell(1, 2).Value = item.VesselName;
                worksheet.Cell(2, 2).Value = item.VesselFlag;
                worksheet.Cell(3, 2).Value = item.CarrierNameEn;
                worksheet.Cell(4, 2).Value = item.CarrierCountryEn;
                worksheet.Cell(5, 2).Value = item.CarrierLocation;

                worksheet.Cell(2, 5).Value = item.CarrierContract;
                worksheet.Cell(3, 5).Value = item.CarrierContractDate;

                worksheet.Cell(1, 11).Value = item.CaptainFamily;
                worksheet.Cell(2, 11).Value = item.CaptainName;
                worksheet.Cell(3, 11).Value = "КАПИТАН";
                worksheet.Cell(4, 11).Value = item.BLDate;
                worksheet.Cell(5, 11).Value = item.POLEnCountryEn;

                worksheet.Cell(2, 13).Value = item.CustomsOfficeCode;

                /// TABLE                
                int columns = 18;
                int startRow = 7;
                int row = startRow;

                foreach (var exportOrder in carrier)
                {
                    foreach (var record in exportOrder.Records)
                    {
                        /// Форматирование строки по образцу первой строки
                        if (row > startRow)
                        {
                            for (int col = 1; col <= columns; col++)
                            {
                                var cell = worksheet.Cell(row, col);
                                cell.Style = worksheet.Cell(startRow, col).Style;
                            }
                        }

                        worksheet.Cell(row, 1).Value = record.Seal;
                        worksheet.Cell(row, 2).Value = exportOrder.PortOfDischargeEn;
                        worksheet.Cell(row, 3).Value = exportOrder.PortOfDischargeUnlocode;
                        worksheet.Cell(row, 4).Value = exportOrder.BLDate;
                        worksheet.Cell(row, 5).Value = exportOrder.BLNum;
                        worksheet.Cell(row, 6).Value = record.ShipperEn;
                        worksheet.Cell(row, 7).Value = record.ShipperCountryEn;
                        worksheet.Cell(row, 8).Value = record.ConsigneeEn;
                        worksheet.Cell(row, 9).Value = record.ConsigneeCountryEn;

                        worksheet.Cell(row, 10).Value = record.ContainerNum;
                        worksheet.Cell(row, 11).Value = record.Commodity;
                        worksheet.Cell(row, 12).Value = record.GrossWt == 0 ? record.CntrTareWt : record.GrossWt;
                        worksheet.Cell(row, 13).Value = record.PackageQty ?? 0;
                        worksheet.Cell(row, 14).Value = record.CntrType?.Substring(2, 2);
                        worksheet.Cell(row, 15).Value = record.CntrType?[..2];
                        worksheet.Cell(row, 16).Value = record.GrossWt == 0 ? 0 : record.CntrTareWt;
                        worksheet.Cell(row, 17).Value = record.IMO;
                        worksheet.Cell(row, 18).Value = record.UNNO;

                        ++row;
                    }                    
                }
            }
            
            workbook.Save();

            byte[] buffer = await File.ReadAllBytesAsync(FilePath);

            return buffer;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
            return Array.Empty<byte>();
            throw new ApplicationException($"Не удалось создать Excel файл.", ex);
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
            return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Array.Empty<byte>();
        }
    }

    private void CreateTemporaryFile(string templateFileName)
    {
        string templateFilePath = Path.Combine(DirResources, templateFileName);

        if (!File.Exists(templateFilePath))
            throw new FileNotFoundException($"Файл Шаблона '{templateFileName}' не найден в папке ресурсов: {DirResources}");

        FilePath = Path.Combine(DirTemporary, $"{Guid.NewGuid()}.xlsx");

        if (File.Exists(templateFilePath))
            File.Copy(templateFilePath, FilePath);

        if (!File.Exists(FilePath))
            throw new IOException($"Failed to create temporary file at: {FilePath}");
    }

    public void Dispose()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);

        if (Directory.Exists(DirTemporary))
            Directory.Delete(DirTemporary, true);
    }
}
