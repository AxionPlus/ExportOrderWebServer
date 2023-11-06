
using Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExportOrderWebServer.Service;
public class ExcelCreateService : IDisposable
{
    private readonly string TempFilePath;
    private readonly uint ExcelAppPid;
    private readonly IEnumerable<ManifestDTO> Items;

    private Excel.Application? ExcelApp;
    private Excel.Workbooks? Workbooks;
    private Excel.Workbook? Workbook;
    private Excel.Sheets? WorkSheets;
    private Excel.Worksheet? WorkSheet;
    private Excel.Range? Range;

    public ExcelCreateService(string tempFilePath, IEnumerable<ManifestDTO> items)
    {
        ExcelApp = new Excel.Application();
        Workbooks = ExcelApp.Workbooks;

        TempFilePath = tempFilePath;
        Items = items;

        var tid = GetWindowThreadProcessId(ExcelApp.Hwnd, out ExcelAppPid);
    }


    public async Task<byte[]> CreateExcelFile()
    {
        if (string.IsNullOrEmpty(TempFilePath)) return Array.Empty<byte>();

        try
        {
            InitializeExcel();

            CreateTempFile(Resource.TemplateFillBill);

            var carrierGroup = Items.GroupBy(it => it.CarrierNameEn).ToList();

            int indexCarrier = 0;
            int overallRows = 0;

            for (int i = 1; i < carrierGroup.Count(); i++)
                WorkSheets!.Copy(After: WorkSheets!.Item[i]);

            foreach (var carrier in carrierGroup)
            {
                // HEAD

                ++indexCarrier;

                var item = carrier.Select(g => new
                                   {
                                      g.CarrierNameEn,
                                      g.VesselName,
                                      g.VesselFlagEn,
                                      g.CarrierCountryEn,
                                      g.CarrierLocation,
                                      g.CarrierContract,
                                      g.CarrierContractDate,
                                      g.CaptainFamily,
                                      g.CaptainName,
                                      g.BLDate,
                                      g.POLEn,
                                      g.CustomsOfiiceCode,
                                   })
                                  .FirstOrDefault();

                WorkSheet = WorkSheets!.Item[indexCarrier];
                WorkSheet.Name = item!.CarrierNameEn;

                WorkSheet.Cells[1, 2].Value = item!.VesselName;
                WorkSheet.Cells[2, 2].Value = item!.VesselFlagEn;
                WorkSheet.Cells[3, 2].Value = item!.CarrierNameEn;
                WorkSheet.Cells[4, 2].Value = item!.CarrierCountryEn;
                WorkSheet.Cells[5, 2].Value = item!.CarrierLocation;

                WorkSheet.Cells[2, 5].Value = item!.CarrierContract;
                WorkSheet.Cells[3, 5].Value = item!.CarrierContractDate;

                WorkSheet.Cells[1, 11].Value = item!.CaptainFamily;
                WorkSheet.Cells[2, 11].Value = item!.CaptainName;
                WorkSheet.Cells[3, 11].Value = "КАПИТАН";
                WorkSheet.Cells[4, 11].Value = item.BLDate;
                WorkSheet.Cells[5, 11].Value = item.POLEn;

                WorkSheet.Cells[2, 13].Value = item.CustomsOfiiceCode;

                // TABLE                
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
                    dataBulk[row, 0] = Items.ElementAt(row).Seal;
                    dataBulk[row, 1] = Items.ElementAt(row).PODEn;
                    dataBulk[row, 2] = Items.ElementAt(row).PODunlocode!;
                    dataBulk[row, 3] = Items.ElementAt(row).BLDate;
                    dataBulk[row, 4] = Items.ElementAt(row).BLNum;
                    dataBulk[row, 5] = Items.ElementAt(row).Shippers.Substring(4);
                    dataBulk[row, 6] = Items.ElementAt(row).ShippersCountries;
                    dataBulk[row, 7] = Items.ElementAt(row).Consignees.Substring(4);
                    dataBulk[row, 8] = Items.ElementAt(row).ConsigneesCountries;
                    dataBulk[row, 9] = Items.ElementAt(row).Cntr;
                    dataBulk[row, 10] = Items.ElementAt(row).Commodities;
                    dataBulk[row, 11] = Items.ElementAt(row).GrossWeights! == 0 ? Items.ElementAt(row).CntrTareWt : Items.ElementAt(row).GrossWeights!;
                    dataBulk[row, 12] = Items.ElementAt(row).PackageQtys!;
                    dataBulk[row, 13] = Items.ElementAt(row).CntrType.Substring(2, 2);
                    dataBulk[row, 14] = Items.ElementAt(row).CntrType.Substring(0, 2);
                    dataBulk[row, 15] = Items.ElementAt(row).GrossWeights! == 0 ? 0 : Items.ElementAt(row).CntrTareWt!;
                    dataBulk[row, 16] = Items.ElementAt(row).IMO;
                    dataBulk[row, 17] = Items.ElementAt(row).UNNO;
                });

                Range.Value = dataBulk;
                dataBulk = null;
                
                overallRows = overallRows + rows;   // check, probably use "|"
            }

            SaveTempFile();

            byte[] fileBytes = File.ReadAllBytes(TempFilePath);

            await Task.Run(async () => { await Task.Delay(SetDelay(overallRows)); });

            return fileBytes;
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            Console.WriteLine(msg);
            return Array.Empty<byte>();
        }        
    }

    #region SUPPORT METHODS

    private void InitializeExcel()
    {
        ExcelApp = new Excel.Application();
        Workbooks = ExcelApp.Workbooks;
        //var tid = GetWindowThreadProcessId(ExcelApp.Hwnd, out ExcelAppPid);
    }

    private bool CreateTempFile(byte[]? ResourceFile)
    {
        try
        {
            File.WriteAllBytes(TempFilePath, ResourceFile!);
            Resource.ResourceManager.ReleaseAllResources();

            Workbook = Workbooks!.Open(TempFilePath);
            WorkSheets = Workbook!.Worksheets;

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    private void SaveTempFile()
    {
        Workbook!.Save();
        if (Workbook != null)
            Workbook.Close();

        if (ExcelApp != null)
            ExcelApp.Quit();
    }

    private int SetDelay(int records)
    {
        switch (records)
        {
            case < 1000: return 2000;
            case < 2000: return 4000;
            case < 3000: return 6000;
            case < 4000: return 8000;
            default: return 10000;
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    void IDisposable.Dispose()
    {
        try
        {
            Process[] process = Process.GetProcessesByName("Excel");
            foreach (Process p in process)
                if (!string.IsNullOrEmpty(p.ProcessName))
                    if (ExcelAppPid > 0)
                        if (p.Id == ExcelAppPid)
                            p.Kill();

            if (File.Exists(TempFilePath))
                File.Delete(TempFilePath);
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }

    #endregion
}
