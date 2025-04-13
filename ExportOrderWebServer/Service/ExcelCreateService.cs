using System.Diagnostics;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExportOrderWebServer.Service;
public class ExcelCreateService : IDisposable
{
    private readonly string TempFilePath;
    private readonly uint ExcelAppPid;
    private readonly IEnumerable<VoyageManifestDTO>? Items;
    private readonly ExportOrderDTO? Item;

    private readonly Excel.Application ExcelApp;
    private readonly Excel.Workbooks Workbooks;
    private Excel.Workbook? Workbook;
    private Excel.Sheets? WorkSheets;
    private Excel.Worksheet? WorkSheet;
    private Excel.Range? Range;

    public ExcelCreateService(string tempFilePath, IEnumerable<VoyageManifestDTO>? items, ExportOrderDTO? item)
    {
        ExcelApp = new Excel.Application();
        Workbooks = ExcelApp.Workbooks;

        TempFilePath = tempFilePath;
        Items = items;
        Item = item;

        var tid = GetWindowThreadProcessId(ExcelApp.Hwnd, out ExcelAppPid);        
    }

    /// FILL BILL
    public async Task<byte[]> CreateExcelFile_FillBill()
    {
        //if (string.IsNullOrEmpty(TempFilePath)) return Array.Empty<byte>();

        try
        {
            //InitializeExcel();

            /// Create Teplate file (returt Empty bytes in case error)
            if (Resource.TemplateFillBill == Array.Empty<byte>()) return Array.Empty<byte>();

            CreateTempFile(Resource.TemplateFillBill);

            var carrierGroup = Items!.GroupBy(it => it.CarrierNameEn).ToList();

            int indexCarrier = 0;
            int overallRows = 0;

            for (int i = 1; i < carrierGroup.Count; i++)
                WorkSheet!.Copy(After: WorkSheets!.Item[i]);

            foreach (var carrier in carrierGroup)
            {
                // HEAD of table

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

                //WorkSheet = WorkSheets!.Item[indexCarrier];
                WorkSheet = Workbook!.Sheets[indexCarrier];
                WorkSheet.Name = item!.CarrierNameEn;

                WorkSheet.Cells[1, 2].Value = item!.VesselName;
                WorkSheet.Cells[2, 2].Value = item!.VesselFlag;
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

                WorkSheet.Cells[2, 13].Value = item.CustomsOfficeCode;

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

    /// EXPORT REQUEST - ROLIS
    public async Task<byte[]> CreateExcelFile_Rolis()
    {
        //if (string.IsNullOrEmpty(TempFilePath)) return Array.Empty<byte>();

        /// Create Teplate file (returt Empty bytes in case error)
        CreateTempFile(Resource.TemplateExpRequestRolis);

        var documentsGroup = Item!.ExportOrderRecordsDTO.GroupBy(eor => eor.DocumentName).ToArray();

        WorkSheet!.Cells[2, 2].Value = Item!.Shippers;
        WorkSheet!.Cells[3, 2].Value = Item!.Consignees;
        WorkSheet!.Cells[4, 2].Value = Item!.Consignees;
        WorkSheet!.Cells[5, 2].Value = Item!.Num;
        //WorkSheet!.Cells[6, 2].Value = string.IsNullOrEmpty(Item!.PersonPass) ? string.Empty : Item!.PersonPass.Substring(0, 12);
        WorkSheet!.Cells[6, 4].Value = string.IsNullOrEmpty(Item!.PersonPhone) ? string.Empty : Item!.PersonPhone;
        //WorkSheet!.Cells[3, 7].Value = documentsGroup.Select(g => g.Key).Count() == 1 ? documentsGroup.Select(g => g.Key).FirstOrDefault() : string.Empty;

        /// TABLE                
        int columns = 18;
        int rows = Item.ExportOrderRecordsDTO.Count();

        int startRow = 9;

        var startCell = WorkSheet.Cells[startRow, 1];
        var endCell = WorkSheet.Cells[rows + startRow - 1, columns];
        Range = WorkSheet.Range[startCell, endCell];

        if (rows > 1) Range.FillDown();

        var dataBulk = new object[rows, columns];
        var records = Item.ExportOrderRecordsDTO;

        var result = Parallel.For(0, rows, (row, state) =>
        {
            dataBulk[row, 0] = records.ElementAt(row).Cntr;
            //dataBulk[row, 1] = string.Empty;
            //dataBulk[row, 2] = records.ElementAt(row).CntrTypeISO;
            dataBulk[row, 3] = records.ElementAt(row).Seal;
            dataBulk[row, 4] = records.ElementAt(row).Commodity.ToUpper().Equals("ПОРОЖНИЙ КОНТЕЙНЕР") ?
                                "Порожний контейнер / Empty Container" : records.ElementAt(row).Commodity;
            dataBulk[row, 5] = records.ElementAt(row).PackageQty!;
            dataBulk[row, 6] = records.ElementAt(row).IMO!;
            dataBulk[row, 7] = records.ElementAt(row).UNNO!;
            dataBulk[row, 8] = records.ElementAt(row).NetWt!;
            dataBulk[row, 9] = records.ElementAt(row).GrossWt!;
            dataBulk[row, 10] = records.ElementAt(row).CntrTareWt!;
            dataBulk[row, 11] = records.ElementAt(row).GrossWt! == 0 ? records.ElementAt(row).CntrTareWt! : records.ElementAt(row).GrossAndTare!;
            //dataBulk[row, 12] = string.Empty;
            //dataBulk[row, 13] = 0;
            dataBulk[row, 14] = records.ElementAt(row).HSCode;
            dataBulk[row, 15] = records.ElementAt(row).DocumentName;
            dataBulk[row, 16] = records.ElementAt(row).DocumentType;
            dataBulk[row, 17] = records.ElementAt(row).SeqContent; ;
            //dataBulk[row, 17] = row + 1;
        }); 

        Range.Value = dataBulk;

        if (Item.CommodityShort != null)
        {
            WorkSheet.Cells[rows + 1, 1].Font.Bold = true;
            WorkSheet.Cells[rows + 1, 1] = "Дополнительные сведения";
            WorkSheet.Cells[rows + 1, 2] = Item.CommodityShort;
        }

        SaveTempFile();

        byte[] fileBytes = File.ReadAllBytes(TempFilePath);

        await Task.Run(async () => { await Task.Delay(SetDelay(rows)); });

        return fileBytes;
    }

    #region SUPPORT METHODS

    private void CreateTempFile(byte[] ResourceFileBuffer)
    {
        try
        {   
            //File.WriteAllBytes(TempFilePath, Resource.TemplateFillBill);
            File.WriteAllBytes(TempFilePath, ResourceFileBuffer);
            Resource.ResourceManager.ReleaseAllResources();
            
            Workbook = Workbooks.Open(TempFilePath);
            WorkSheets = Workbook!.Worksheets;
            WorkSheet = WorkSheets[1];
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            Console.WriteLine(msg);
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
