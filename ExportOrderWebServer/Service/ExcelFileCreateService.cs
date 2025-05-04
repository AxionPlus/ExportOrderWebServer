using System.Diagnostics;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExportOrderWebServer.Service;

public interface IExcelFileCreateService : IDisposable
{
    Task<byte[]> CreateExcelFile_FillBill(IEnumerable<ManifestDTO> items);
    Task<byte[]> CreateExcelFile_Rolis(ExportOrderDTO item);
}

public class ExcelFileCreateService : IExcelFileCreateService
{    
    private readonly uint ExcelAppPid;
    private readonly Excel.Application ExcelApp;
    private readonly Excel.Workbooks Workbooks;
    private Excel.Workbook? Workbook;
    private Excel.Sheets? WorkSheets;
    private Excel.Worksheet? WorkSheet;
    private Excel.Range? Range;
        
    private readonly static string DirResources = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
    private readonly static string DirTemporary = Path.Combine(DirResources, "TempFiles");

    private string TemplateFilePath { get; set; } = string.Empty;
    private string TemporaryFilePath { get; set; } = Path.Combine(DirTemporary, $"{Path.GetRandomFileName()}.xlsx");

    public ExcelFileCreateService()
    {   
        ExcelApp = new Excel.Application();
        Workbooks = ExcelApp.Workbooks;

        if (!Directory.Exists(DirTemporary))
            Directory.CreateDirectory(DirTemporary);

        var tid = GetWindowThreadProcessId(ExcelApp.Hwnd, out ExcelAppPid);
    }


    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    
    public async Task<byte[]> CreateExcelFile_FillBill(IEnumerable<ManifestDTO> items)
    {        
        TemplateFilePath = Path.Combine(DirResources, "TemplateFillBill.xlsx");

        if (!File.Exists(TemplateFilePath)) return Array.Empty<byte>();
        
        CreateTempFile(TemporaryFilePath);

        if (WorkSheet is null) return Array.Empty<byte>();

        try
        {
            var carrierGroup = items.GroupBy(it => it.CarrierNameEn).ToList();

            int indexCarrier = 0;
            int overallRows = 0;

            for (int i = 1; i < carrierGroup.Count; i++)
                WorkSheet!.Copy(After: WorkSheets!.Item[i]);

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
                    s.POLEn,
                    s.CustomsOfficeCode,
                }).FirstOrDefault();

                WorkSheet = Workbook!.Sheets[indexCarrier];
                WorkSheet.Name = item!.CarrierNameEn;

                /// HEADER
                WorkSheet.Cells[1, 2].Value = item.VesselName;
                WorkSheet.Cells[2, 2].Value = item.VesselFlag;
                WorkSheet.Cells[3, 2].Value = item.CarrierNameEn;
                WorkSheet.Cells[4, 2].Value = item.CarrierCountryEn;
                WorkSheet.Cells[5, 2].Value = item.CarrierLocation;

                WorkSheet.Cells[2, 5].Value = item.CarrierContract;
                WorkSheet.Cells[3, 5].Value = item.CarrierContractDate;

                WorkSheet.Cells[1, 11].Value = item.CaptainFamily;
                WorkSheet.Cells[2, 11].Value = item.CaptainName;
                WorkSheet.Cells[3, 11].Value = "КАПИТАН";
                WorkSheet.Cells[4, 11].Value = item.BLDate;
                WorkSheet.Cells[5, 11].Value = item.POLEn;

                WorkSheet.Cells[2, 13].Value = item.CustomsOfficeCode;

                /// TABLE                
                int columns = 18;
                int rows = carrier.Count();

                int startRow = 7;

                var startCell = WorkSheet.Cells[startRow, 1];
                var endCell = WorkSheet.Cells[rows + startRow - 1, columns];
                Range = WorkSheet.Range[startCell, endCell];

                if (rows > 1) Range.FillDown();

                var dataBulk = new object[rows, columns];

                var result = Parallel.For(0, rows, (row, state) =>
                {
                    dataBulk[row, 0] = carrier.ElementAt(row).Seal!;
                    dataBulk[row, 1] = carrier.ElementAt(row).PODandCountryEn!;
                    dataBulk[row, 2] = carrier.ElementAt(row).PODunlocode!;
                    dataBulk[row, 3] = carrier.ElementAt(row).BLDate!;
                    dataBulk[row, 4] = carrier.ElementAt(row).BLNum!;
                    dataBulk[row, 5] = carrier.ElementAt(row).RecordShippers!;
                    dataBulk[row, 6] = carrier.ElementAt(row).RecordShippersCountries!;
                    dataBulk[row, 7] = carrier.ElementAt(row).RecordConsignees!;
                    dataBulk[row, 8] = carrier.ElementAt(row).RecordConsigneesCountries!;
                    dataBulk[row, 9] = carrier.ElementAt(row).Cntr!;
                    dataBulk[row, 10] = carrier.ElementAt(row).RecordCommodities!;
                    dataBulk[row, 11] = carrier.ElementAt(row).GrossWeights! == 0 ? carrier.ElementAt(row).CntrTareWt : carrier.ElementAt(row).GrossWeights!;
                    dataBulk[row, 12] = carrier.ElementAt(row).PackageQtys!;
                    dataBulk[row, 13] = carrier.ElementAt(row).CntrType!.Substring(2, 2);
                    dataBulk[row, 14] = carrier.ElementAt(row).CntrType!.Substring(0, 2);
                    dataBulk[row, 15] = carrier.ElementAt(row).GrossWeights! == 0 ? 0 : carrier.ElementAt(row).CntrTareWt!;
                    dataBulk[row, 16] = carrier.ElementAt(row).IMO!;
                    dataBulk[row, 17] = carrier.ElementAt(row).UNNO!;
                });

                Range.Value = dataBulk;

                overallRows += rows;
            }

            SaveTempFile();

            byte[] fileBytes = File.ReadAllBytes(TemporaryFilePath);

            await Task.Run(async () => { await Task.Delay(SetDelay(overallRows)); });

            return fileBytes;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Dispose();
            return Array.Empty<byte>();
        }        
    }
    
    public async Task<byte[]> CreateExcelFile_Rolis(ExportOrderDTO item)
    {
        TemplateFilePath = Path.Combine(DirResources, "TemplateNutepRolis.xlsx");

        if (!File.Exists(TemplateFilePath)) return Array.Empty<byte>();

        CreateTempFile(TemporaryFilePath);

        if (WorkSheet is null) return Array.Empty<byte>();

        try
        {
            /// HEADER
            WorkSheet!.Cells[2, 2].Value = item.Shippers;
            WorkSheet!.Cells[3, 2].Value = item.Consignees;
            WorkSheet!.Cells[4, 2].Value = item.Consignees;
            WorkSheet!.Cells[5, 2].Value = item.Num;
            WorkSheet!.Cells[6, 4].Value = item.PersonPhone;

            /// TABLE                
            int columns = 18;
            int rows = item.ExportOrderRecordsDTO.Count();

            int startRow = 9;

            var startCell = WorkSheet.Cells[startRow, 1];
            var endCell = WorkSheet.Cells[rows + startRow - 1, columns];
            Range = WorkSheet.Range[startCell, endCell];

            if (rows > 1) Range.FillDown();

            var dataBulk = new object[rows, columns];
            var records = item.ExportOrderRecordsDTO;

            var result = Parallel.For(0, rows, (row, state) =>
            {
                dataBulk[row, 0] = records.ElementAt(row).Cntr;
                dataBulk[row, 3] = records.ElementAt(row).Seal!;
                dataBulk[row, 4] = records.ElementAt(row).Commodity.ToUpper().Equals("ПОРОЖНИЙ КОНТЕЙНЕР") ?
                                    "Порожний контейнер / Empty Container" : records.ElementAt(row).Commodity;
                dataBulk[row, 5] = records.ElementAt(row).PackageQty!;
                dataBulk[row, 6] = records.ElementAt(row).IMO!;
                dataBulk[row, 7] = records.ElementAt(row).UNNO!;
                dataBulk[row, 8] = records.ElementAt(row).NetWt!;
                dataBulk[row, 9] = records.ElementAt(row).GrossWt!;
                dataBulk[row, 10] = records.ElementAt(row).CntrTareWt!;
                dataBulk[row, 11] = records.ElementAt(row).GrossAndTare!;
                dataBulk[row, 12] = records.ElementAt(row).SupplementaryUnitCode!;
                dataBulk[row, 13] = records.ElementAt(row).SupplementaryUnitQuantity!;
                dataBulk[row, 14] = records.ElementAt(row).HSCode;
                dataBulk[row, 15] = records.ElementAt(row).DocumentName;
                dataBulk[row, 16] = records.ElementAt(row).DocumentType;
                dataBulk[row, 17] = records.ElementAt(row).SeqContent; ;
            });

            Range.Value = dataBulk;

            if (!string.IsNullOrWhiteSpace(item.CommodityShort))
            {
                WorkSheet.Cells[rows + 1, 1].Font.Bold = true;
                WorkSheet.Cells[rows + 1, 1] = "Дополнительные сведения";
                WorkSheet.Cells[rows + 1, 2] = item.CommodityShort;
            }

            SaveTempFile();

            byte[] fileBytes = File.ReadAllBytes(TemporaryFilePath);

            await Task.Run(async () => { await Task.Delay(SetDelay(rows)); });

            return fileBytes;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Dispose();
            return Array.Empty<byte>();
        }
    }


    #region SUPPORT METHODS

    private void CreateTempFile(string destinationPath)
    {
        //if (File.Exists(TemplateFilePath))
        File.Copy(TemplateFilePath, destinationPath);

        if (File.Exists(destinationPath))
            Workbook = Workbooks.Open(destinationPath);
        else
            return;

        if (Workbook is null)
            return;
        else
        {
            WorkSheets = Workbook.Worksheets;
            WorkSheet = WorkSheets[1];
        }
    }

    private void SaveTempFile()
    {
        Workbook?.Save();
        Workbook?.Close();
        ExcelApp.Quit();
    }

    private static int SetDelay(int records)
    {
        return records switch
        {
            < 1000 => 2000,
            < 2000 => 4000,
            < 3000 => 6000,
            < 4000 => 8000,
            _ => 10000,
        };
    }

    public void Dispose()
    {
        Process[] process = Process.GetProcessesByName("Excel");
        foreach (Process p in process)
            if (!string.IsNullOrEmpty(p.ProcessName))
                if (ExcelAppPid > 0)
                    if (p.Id == ExcelAppPid)
                        p.Kill();

        //foreach (var file in Directory.GetFiles(DirTemporary))
        //        File.Delete(file);
        if (File.Exists(TemporaryFilePath))
            File.Delete(TemporaryFilePath);
    }

    #endregion
}
