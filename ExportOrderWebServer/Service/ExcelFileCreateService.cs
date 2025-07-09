using ExportOrderEntites.BillofLading;
using ExportOrderEntites.BillofLading.Dto;
using ExportOrderEntites.ImportVesselCall.Dto;
using Microsoft.JSInterop;
using Microsoft.Office.Interop.Excel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using static MudBlazor.CategoryTypes;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExportOrderWebServer.Service;

public interface IExcelFileCreateService : IDisposable
{
    Task<byte[]> CreateExcelFile_FillBill(IEnumerable<ManifestDTO> items);
    Task<byte[]> CreateExcelFile_Rolis(ExportOrderDTO item);
    Task<byte[]> CreateExcelTemplate1C(VesselCallDetailDTO VesselCall, IEnumerable<BillOfLadingDto> billOfLadings);
    Task<byte[]> CreateExcelReport(object[,] Array, string reportName);
    Task<byte[]> CreateExcelTemplateFillBill(ImportVesselCallDto VesselCall);
    Task<byte[]> CreateExcelTemplateArrivalNotice(ImportVesselCallDto VesselCall);
    Task<byte[]> CreateExcelTemplateCargoManifest(ImportVesselCallDto VesselCall);
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
    private readonly IJSRuntime JSRuntime;

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


    public async Task<byte[]> CreateExcelReport(object[,] Array, string reportName)
    {
        TemplateFilePath = Path.Combine(DirResources, "EmptyWorkSheet.xlsx");

        if (!File.Exists(TemplateFilePath)) return new byte[] { };

        CreateTempFile(TemporaryFilePath);


        long rows = Array.GetLength(0);
        int columns = Array.GetLength(1);

        WorkSheet = WorkSheets!.Item[1];

        var startCell = WorkSheet.Cells[1, 1];
        var endCell = WorkSheet.Cells[rows, columns];

        Excel.Range? HeaderRange = WorkSheet.Range[startCell, WorkSheet.Cells[1, columns]];
        HeaderRange.Font.Bold = true;
        HeaderRange.Interior.Color = Excel.XlRgbColor.rgbLightGray;

        Range = WorkSheet.Range[startCell, endCell];

        Range.Value = Array;
        Range.Columns.AutoFit();

        SaveTempFile();

        byte[] fileBytes = File.ReadAllBytes(TemporaryFilePath);
        return fileBytes;
    }

    public async Task<byte[]> CreateExcelTemplate1C(VesselCallDetailDTO VesselCall, IEnumerable<BillOfLadingDto> billOfLadings)
    {
        TemplateFilePath = Path.Combine(DirResources, "Template1C.xlsx");

        if (!File.Exists(TemplateFilePath)) return Array.Empty<byte>();

        CreateTempFile(TemporaryFilePath);

        if (WorkSheet is null) return Array.Empty<byte>();

        var records = billOfLadings.SelectMany(s => s.ContainerRecords, (bl, rec) => new ManifestBillOfLadingDto()
        {
            Num = bl.Num,
            IssueDate = bl.IssueDate,
            ServiceCode = bl.ServiceCode,
            ShipperName = bl.ShipperName,
            ShipperAddress = bl.ShipperAddress,
            ConsigneeName = bl.ConsigneeName,
            ConsigneeAddress = bl.ConsigneeAddress,
            ConsigneeTaxNo = bl.ConsigneeTaxNo,
            NotifyName = bl.NotifyName,
            NotifyAddress = bl.NotifyAddress,
            NotifyEmail = bl.NotifyEmail,
            AdditionalInfo = bl.AdditionalInfo,
            ShipperNameRu = bl.ShipperNameRu,
            ShipperAddressRu = bl.ShipperAddressRu,
            ShipperCountryRu = bl.ShipperCountryRu,
            ConsigneeNameRu = bl.ConsigneeNameRu,
            ConsigneeAddressRu = bl.ConsigneeAddressRu,
            ConsigneeCountryRu = bl.ConsigneeCountryRu,
            CustomsDeliveryMode = bl.CustomsDeliveryMode,
            POR = bl.POR,
            POL = bl.POL,
            TS_PORT = bl.TS_PORT,
            POD = bl.POD,
            F_POD = bl.F_POD,
            ContainerNum = rec.ContainerNum,
            ContainerType = rec.ContainerType,
            TareWeight = rec.TareWeight,
            CargoWeight = rec.CargoWeight,
            SealNo = rec.SealNo,
            SealShr = rec.SealShr,
            SealOth = rec.SealOth,
            ImoClass = rec.ImoClass,
            Unno = rec.Unno,
            TempSet = rec.TempSet,
            PackageQty = rec.PackageQty,
            CommodityCode = rec.CommodityCode,
            GoodsDescription = rec.GoodsDescription,
            GoodsDescriptionRu = rec.GoodsDescriptionRu,

        }).ToList();
        var i = 1;
        records.ForEach(record => record.No = i++);
        /// TABLE                
        int columns = 28;
        int rows = records.Count();

        int startRow = 2;

        var startCell = WorkSheet.Cells[startRow, 1];
        var endCell = WorkSheet.Cells[rows + startRow - 1, columns];
        Range = WorkSheet.Range[startCell, endCell];

        if (rows > 1) Range.FillDown();

        var dataBulk = new object[rows, columns];

        var result = Parallel.For(0, rows, (row, state) =>
        {
            var seal = new List<string>();
            if (!string.IsNullOrWhiteSpace(records.ElementAt(row).SealNo))
                seal.Add(records.ElementAt(row).SealNo);
            if (!string.IsNullOrWhiteSpace(records.ElementAt(row).SealShr))
                seal.Add(records.ElementAt(row).SealShr);
            if (!string.IsNullOrWhiteSpace(records.ElementAt(row).SealOth))
                seal.Add(records.ElementAt(row).SealOth);

            dataBulk[row, 0] = records.ElementAt(row).No;
            dataBulk[row, 2 - 1] = records.ElementAt(row).ContainerNum;
            dataBulk[row, 2 - 1] = records.ElementAt(row).ContainerNum;
            dataBulk[row, 3 - 1] = records.ElementAt(row).ContainerType;
            dataBulk[row, 4 - 1] = records.ElementAt(row).ContainerType;
            dataBulk[row, 5 - 1] = string.Join("; ", seal);
            dataBulk[row, 6 - 1] = records.ElementAt(row).PackageQty;
            dataBulk[row, 7 - 1] = records.ElementAt(row).GoodsDescriptionRu;
            dataBulk[row, 8 - 1] = records.ElementAt(row).GoodsDescription;
            dataBulk[row, 9 - 1] = records.ElementAt(row).CommodityCode;
            dataBulk[row, 10 - 1] = records.ElementAt(row).CargoWeight;
            dataBulk[row, 11 - 1] = records.ElementAt(row).TareWeight;
            dataBulk[row, 12 - 1] = records.ElementAt(row).Num;
            dataBulk[row, 13 - 1] = records.ElementAt(row).IssueDate.HasValue ? records.ElementAt(row).IssueDate.Value.Date : null;
            dataBulk[row, 14 - 1] = records.ElementAt(row).ShipperName;
            dataBulk[row, 15 - 1] = records.ElementAt(row).ShipperNameRu;
            dataBulk[row, 16 - 1] = records.ElementAt(row).ShipperCountryRu;
            dataBulk[row, 17 - 1] = records.ElementAt(row).ShipperAddress;
            dataBulk[row, 18 - 1] = records.ElementAt(row).ConsigneeName;
            dataBulk[row, 19 - 1] = records.ElementAt(row).ConsigneeNameRu;
            dataBulk[row, 20 - 1] = records.ElementAt(row).ConsigneeCountryRu;
            dataBulk[row, 21 - 1] = records.ElementAt(row).ConsigneeAddressRu;
            dataBulk[row, 22 - 1] = records.ElementAt(row).POR;
            dataBulk[row, 23 - 1] = records.ElementAt(row).POL;
            dataBulk[row, 24 - 1] = records.ElementAt(row).CustomsDeliveryMode;
            dataBulk[row, 25 - 1] = records.ElementAt(row).ImoClass + records.ElementAt(row).Unno;
            dataBulk[row, 26 - 1] = records.ElementAt(row).TempSet;

        });

        Range.Value = dataBulk;

        SaveTempFile();

        byte[] fileBytes = File.ReadAllBytes(TemporaryFilePath);

        await Task.Run(async () => { await Task.Delay(SetDelay(rows)); });

        return fileBytes;

    }

    public async Task<byte[]> CreateExcelTemplateFillBill(ImportVesselCallDto VesselCall)
    {
        TemplateFilePath = Path.Combine(DirResources, "FillBillTemplateSoling.xlsx");

        if (!File.Exists(TemplateFilePath)) return Array.Empty<byte>();

        CreateTempFile(TemporaryFilePath);

        if (WorkSheet is null) return Array.Empty<byte>();

        WorkSheet.Cells[2, 1] = VesselCall.VesselName.ToUpper();  //Vessel Name 
        WorkSheet.Cells[2, 5] = VesselCall.VesselVoyage.ToUpper();  //Vessel Voyage 
        WorkSheet.Cells[2, 2] = VesselCall.VesselFlag.ToUpper();  //Vessel Flag 
        WorkSheet.Cells[2, 4] = VesselCall.DeparturePortName.ToUpper();  //POL 

        WorkSheet.Cells[1, 10] = "";  //Captain Surname
        WorkSheet.Cells[2, 10] = "";  //Captain Name

        WorkSheet.Cells[2, 15] = VesselCall.CustomsPostCode.ToUpper();  //Customs Post Code Name


        var records = VesselCall.BillofLadings.SelectMany(s => s.ContainerRecords, (bl, rec) => new ManifestBillOfLadingDto()
        {
            Num = bl.Num,
            IssueDate = bl.IssueDate,
            ServiceCode = bl.ServiceCode,
            ShipperName = bl.ShipperName,
            ShipperAddress = bl.ShipperAddress,
            ConsigneeName = bl.ConsigneeName,
            ConsigneeAddress = bl.ConsigneeAddress,
            ConsigneeTaxNo = bl.ConsigneeTaxNo,
            NotifyName = bl.NotifyName,
            NotifyAddress = bl.NotifyAddress,
            NotifyEmail = bl.NotifyEmail,
            AdditionalInfo = bl.AdditionalInfo,
            ShipperNameRu = bl.ShipperNameRu,
            ShipperAddressRu = bl.ShipperAddressRu,
            ShipperCountryRu = bl.ShipperCountryRu,
            ConsigneeNameRu = bl.ConsigneeNameRu,
            ConsigneeAddressRu = bl.ConsigneeAddressRu,
            ConsigneeCountryRu = bl.ConsigneeCountryRu,
            CustomsDeliveryMode = bl.CustomsDeliveryMode,
            POR = bl.POR,
            POL = bl.POL,
            TS_PORT = bl.TS_PORT,
            POD = bl.POD,
            F_POD = bl.F_POD,
            ContainerNum = rec.ContainerNum,
            ContainerType = rec.ContainerType,
            TareWeight = rec.TareWeight,
            CargoWeight = rec.CargoWeight,
            SealNo = rec.SealNo,
            SealShr = rec.SealShr,
            SealOth = rec.SealOth,
            ImoClass = rec.ImoClass,
            Unno = rec.Unno,
            TempSet = rec.TempSet,
            PackageQty = rec.PackageQty,
            CommodityCode = rec.CommodityCode,
            GoodsDescription = rec.GoodsDescription,
            GoodsDescriptionRu = rec.GoodsDescriptionRu,

        }).ToList();
        var i = 1;
        records.ForEach(record => record.No = i++);
        //TABLE                
        int columns = 30;
        int rows = records.Count();

        int startRow = 5;

        var startCell = WorkSheet.Cells[startRow, 1];
        var endCell = WorkSheet.Cells[rows + startRow - 1, columns];
        Range = WorkSheet.Range[startCell, endCell];

        if (rows > 1) Range.FillDown();

        var dataBulk = new object[rows, columns];

        var result = Parallel.For(0, rows, (row, state) =>
        {
            var seal = new List<string>();
            if (!string.IsNullOrWhiteSpace(records.ElementAt(row).SealNo))
                seal.Add(records.ElementAt(row).SealNo);
            if (!string.IsNullOrWhiteSpace(records.ElementAt(row).SealShr))
                seal.Add(records.ElementAt(row).SealShr);
            if (!string.IsNullOrWhiteSpace(records.ElementAt(row).SealOth))
                seal.Add(records.ElementAt(row).SealOth);
            var _size = records.ElementAt(row).ContainerType?.Substring(0, 2).ToUpper();
            var _type = records.ElementAt(row).ContainerType?.Substring(2, 2).ToUpper();

            dataBulk[row, 1 - 1] = records.ElementAt(row).ContainerNum?.ToUpper();
            dataBulk[row, 2 - 1] = _type;
            dataBulk[row, 3 - 1] = _size;
            dataBulk[row, 4 - 1] = records.ElementAt(row).TareWeight;
            dataBulk[row, 5 - 1] = string.Join("; ", seal)?.ToUpper();
            dataBulk[row, 6 - 1] = records.ElementAt(row).CargoWeight;
            dataBulk[row, 7 - 1] = records.ElementAt(row).PackageQty;
            dataBulk[row, 8 - 1] = records.ElementAt(row).GoodsDescription?.ToUpper();
            dataBulk[row, 9 - 1] = records.ElementAt(row).GoodsDescriptionRu?.ToUpper();
            dataBulk[row, 10 - 1] = records.ElementAt(row).Num?.ToUpper();
            dataBulk[row, 11 - 1] = records.ElementAt(row).IssueDate.HasValue ? records.ElementAt(row).IssueDate.Value.Date : null;
            dataBulk[row, 12 - 1] = VesselCall.DeparturePortName.ToUpper();
            dataBulk[row, 13 - 1] = VesselCall.DeparturePortCountryCode?.ToUpper();

            dataBulk[row, 14 - 1] = records.ElementAt(row).ShipperName.ToUpper();
            dataBulk[row, 15 - 1] = records.ElementAt(row).ShipperCountryRu?.ToUpper();
            dataBulk[row, 16 - 1] = records.ElementAt(row).POL.ToUpper();
            dataBulk[row, 17 - 1] = records.ElementAt(row).ShipperAddress?.ToUpper();

            dataBulk[row, 18 - 1] = records.ElementAt(row).ConsigneeNameRu?.ToUpper();
            dataBulk[row, 19 - 1] = records.ElementAt(row).ConsigneeCountryRu?.ToUpper();
            dataBulk[row, 20 - 1] = "";
            dataBulk[row, 21 - 1] = records.ElementAt(row).ConsigneeAddressRu?.ToUpper();
            dataBulk[row, 22 - 1] = records.ElementAt(row).CustomsDeliveryMode switch
            {
                CustomsDeliveryMode.GTD => "ГТД",
                CustomsDeliveryMode.VTT => "ВТТ",

            };
            dataBulk[row, 23 - 1] = records.ElementAt(row).CommodityCode?.ToUpper();


            dataBulk[row, 29 - 1] = records.ElementAt(row).ImoClass?.ToUpper();
            dataBulk[row, 30 - 1] = records.ElementAt(row).Unno?.ToUpper();


        });

        Range.Value = dataBulk;

        SaveTempFile();

        byte[] fileBytes = File.ReadAllBytes(TemporaryFilePath);


        return fileBytes;

    }

    public async Task<byte[]> CreateExcelTemplateArrivalNotice(ImportVesselCallDto VesselCall)
    {
        TemplateFilePath = Path.Combine(DirResources, "ArrivalNoticeTemplate.xlsx");

        if (!File.Exists(TemplateFilePath)) return Array.Empty<byte>();

        CreateTempFile(TemporaryFilePath);

        if (WorkSheet is null) return Array.Empty<byte>();

        WorkSheet.Cells[3, 3] = VesselCall.VesselName.ToUpper();  //Vessel Name 
        WorkSheet.Cells[3, 7] = VesselCall.VesselVoyage.ToUpper();  //Vessel Voyage 
        WorkSheet.Cells[4, 3] = VesselCall.VesselFlag.ToUpper();  //Vessel Flag 
        WorkSheet.Cells[7, 3] = VesselCall.DeparturePortName.ToUpper();  //POL 
        WorkSheet.Cells[6, 3] = VesselCall.ETA;  //ETA 

        var records = VesselCall.BillofLadings.SelectMany(s => s.ContainerRecords, (bl, rec) => new ManifestBillOfLadingDto()
        {
            Num = bl.Num,
            IssueDate = bl.IssueDate,
            ServiceCode = bl.ServiceCode,
            ShipperName = bl.ShipperName,
            ShipperAddress = bl.ShipperAddress,
            ConsigneeName = bl.ConsigneeName,
            ConsigneeAddress = bl.ConsigneeAddress,
            ConsigneeTaxNo = bl.ConsigneeTaxNo,
            NotifyName = bl.NotifyName,
            NotifyAddress = bl.NotifyAddress,
            NotifyEmail = bl.NotifyEmail,
            AdditionalInfo = bl.AdditionalInfo,
            ShipperNameRu = bl.ShipperNameRu,
            ShipperAddressRu = bl.ShipperAddressRu,
            ShipperCountryRu = bl.ShipperCountryRu,
            ConsigneeNameRu = bl.ConsigneeNameRu,
            ConsigneeAddressRu = bl.ConsigneeAddressRu,
            ConsigneeCountryRu = bl.ConsigneeCountryRu,
            CustomsDeliveryMode = bl.CustomsDeliveryMode,
            POR = bl.POR,
            POL = bl.POL,
            TS_PORT = bl.TS_PORT,
            POD = bl.POD,
            F_POD = bl.F_POD,
            ContainerNum = rec.ContainerNum,
            ContainerType = rec.ContainerType,
            TareWeight = rec.TareWeight,
            CargoWeight = rec.CargoWeight,
            SealNo = rec.SealNo,
            SealShr = rec.SealShr,
            SealOth = rec.SealOth,
            ImoClass = rec.ImoClass,
            Unno = rec.Unno,
            TempSet = rec.TempSet,
            PackageQty = rec.PackageQty,
            CommodityCode = rec.CommodityCode,
            GoodsDescription = rec.GoodsDescription,
            GoodsDescriptionRu = rec.GoodsDescriptionRu,

        }).ToList();
        var i = 1;
        records.ForEach(record => record.No = i++);
        //TABLE                
        int columns = 14;
        int rows = records.Count();

        int startRow = 10;

        var startCell = WorkSheet.Cells[startRow, 1];
        var endCell = WorkSheet.Cells[rows + startRow - 1, columns];
        Range = WorkSheet.Range[startCell, endCell];

        if (rows > 1) Range.FillDown();

        var dataBulk = new object[rows, columns];

        var result = Parallel.For(0, rows, (row, state) =>
        {
            var seal = new List<string>();
            if (!string.IsNullOrWhiteSpace(records.ElementAt(row).SealNo))
                seal.Add(records.ElementAt(row).SealNo);
            if (!string.IsNullOrWhiteSpace(records.ElementAt(row).SealShr))
                seal.Add(records.ElementAt(row).SealShr);
            if (!string.IsNullOrWhiteSpace(records.ElementAt(row).SealOth))
                seal.Add(records.ElementAt(row).SealOth);

            var _shipper = $"{records.ElementAt(row).ShipperCountryRu}, {records.ElementAt(row).ShipperName}, {records.ElementAt(row).ShipperAddress}";
            var _consignee = $"{records.ElementAt(row).ConsigneeCountryRu}, {records.ElementAt(row).ConsigneeNameRu}, {records.ElementAt(row).ConsigneeAddressRu}";

            var _size = records.ElementAt(row).ContainerType.Substring(0, 2)?.ToUpper();
            var _type = records.ElementAt(row).ContainerType.Substring(2, 2)?.ToUpper();

            dataBulk[row, 1 - 1] = records.ElementAt(row).No;
            dataBulk[row, 2 - 1] = records.ElementAt(row).ContainerNum?.ToUpper();
            dataBulk[row, 3 - 1] = _size;
            dataBulk[row, 4 - 1] = _type;
            dataBulk[row, 5 - 1] = string.Join("; ", seal)?.ToUpper();
            dataBulk[row, 6 - 1] = records.ElementAt(row).PackageQty;
            dataBulk[row, 7 - 1] = records.ElementAt(row).GoodsDescriptionRu?.ToUpper();
            dataBulk[row, 8 - 1] = records.ElementAt(row).CargoWeight;
            dataBulk[row, 9 - 1] = records.ElementAt(row).TareWeight;
            dataBulk[row, 10 - 1] = records.ElementAt(row).Num?.ToUpper();
            dataBulk[row, 11 - 1] = records.ElementAt(row).IssueDate.HasValue ? records.ElementAt(row).IssueDate.Value.Date : null;
            dataBulk[row, 12 - 1] = _shipper.ToUpper();
            dataBulk[row, 13 - 1] = _consignee.ToUpper();
            dataBulk[row, 14 - 1] = records.ElementAt(row).CustomsDeliveryMode switch
            {
                CustomsDeliveryMode.GTD => "ГТД",
                CustomsDeliveryMode.VTT => "ВТТ",

            };

        });

        Range.Value = dataBulk;

        SaveTempFile();

        byte[] fileBytes = File.ReadAllBytes(TemporaryFilePath);


        return fileBytes;

    }

    public async Task<byte[]> CreateExcelTemplateCargoManifest(ImportVesselCallDto VesselCall)
    {
        TemplateFilePath = Path.Combine(DirResources, "CargoManifest.xlsx");

        if (!File.Exists(TemplateFilePath)) return Array.Empty<byte>();

        CreateTempFile(TemporaryFilePath);

        if (WorkSheet is null) return Array.Empty<byte>();

        WorkSheet.Cells[1, 3] = VesselCall.VesselName.ToUpper();  //Vessel Name 
        WorkSheet.Cells[2, 3] = VesselCall.VesselVoyage.ToUpper();  //Vessel Voyage 
        WorkSheet.Cells[1, 5] = VesselCall.VesselFlag.ToUpper();  //Vessel Flag 
        WorkSheet.Cells[1, 10] = VesselCall.DeparturePortName.ToUpper();  //POL 
        WorkSheet.Cells[1, 12] = VesselCall.ETA;  //ETA 

        var records = VesselCall.BillofLadings.SelectMany(s => s.ContainerRecords, (bl, rec) => new ManifestBillOfLadingDto()
        {
            Num = bl.Num,
            IssueDate = bl.IssueDate,
            ServiceCode = bl.ServiceCode,
            ShipperName = bl.ShipperName,
            ShipperAddress = bl.ShipperAddress,
            ConsigneeName = bl.ConsigneeName,
            ConsigneeAddress = bl.ConsigneeAddress,
            ConsigneeTaxNo = bl.ConsigneeTaxNo,
            NotifyName = bl.NotifyName,
            NotifyAddress = bl.NotifyAddress,
            NotifyEmail = bl.NotifyEmail,
            AdditionalInfo = bl.AdditionalInfo,
            ShipperNameRu = bl.ShipperNameRu,
            ShipperAddressRu = bl.ShipperAddressRu,
            ShipperCountryRu = bl.ShipperCountryRu,
            ConsigneeNameRu = bl.ConsigneeNameRu,
            ConsigneeAddressRu = bl.ConsigneeAddressRu,
            ConsigneeCountryRu = bl.ConsigneeCountryRu,
            CustomsDeliveryMode = bl.CustomsDeliveryMode,
            POR = bl.POR,
            POL = bl.POL,
            TS_PORT = bl.TS_PORT,
            POD = bl.POD,
            F_POD = bl.F_POD,
            ContainerNum = rec.ContainerNum,
            ContainerType = rec.ContainerType,
            TareWeight = rec.TareWeight,
            CargoWeight = rec.CargoWeight,
            SealNo = rec.SealNo,
            SealShr = rec.SealShr,
            SealOth = rec.SealOth,
            ImoClass = rec.ImoClass,
            Unno = rec.Unno,
            TempSet = rec.TempSet,
            PackageQty = rec.PackageQty,
            CommodityCode = rec.CommodityCode,
            GoodsDescription = rec.GoodsDescription,
            GoodsDescriptionRu = rec.GoodsDescriptionRu,

        }).ToList();
        var i = 1;
        records.ForEach(record => record.No = i++);
        //TABLE                
        int columns = 14;
        int rows = records.Count();

        int startRow = 4;

        var startCell = WorkSheet.Cells[startRow, 1];
        var endCell = WorkSheet.Cells[rows + startRow - 1, columns];
        Range = WorkSheet.Range[startCell, endCell];

        if (rows > 1) Range.FillDown();

        var dataBulk = new object[rows, columns];

        var result = Parallel.For(0, rows, (row, state) =>
        {
            var seal = new List<string>();
            if (!string.IsNullOrWhiteSpace(records.ElementAt(row).SealNo))
                seal.Add(records.ElementAt(row).SealNo);
            if (!string.IsNullOrWhiteSpace(records.ElementAt(row).SealShr))
                seal.Add(records.ElementAt(row).SealShr);
            if (!string.IsNullOrWhiteSpace(records.ElementAt(row).SealOth))
                seal.Add(records.ElementAt(row).SealOth);


            var _size = records.ElementAt(row).ContainerType.Substring(0, 2)?.ToUpper();
            var _type = records.ElementAt(row).ContainerType.Substring(2, 2)?.ToUpper();
            var _imo = string.IsNullOrWhiteSpace(records.ElementAt(row).ImoClass) ? null : $"{records.ElementAt(row).ImoClass}-{records.ElementAt(row).Unno}";
            dataBulk[row, 1 - 1] = records.ElementAt(row).No;
            dataBulk[row, 2 - 1] = records.ElementAt(row).Num?.ToUpper();
            dataBulk[row, 3 - 1] = records.ElementAt(row).ShipperName.ToUpper();
            dataBulk[row, 4 - 1] = records.ElementAt(row).ConsigneeName.ToUpper();
            dataBulk[row, 5 - 1] = records.ElementAt(row).ContainerNum?.ToUpper();
            dataBulk[row, 6 - 1] = records.ElementAt(row).TareWeight;
            dataBulk[row, 7 - 1] = _size;
            dataBulk[row, 8 - 1] = _type;
            dataBulk[row, 9 - 1] = string.Join("; ", seal)?.ToUpper();
            dataBulk[row, 10 - 1] = records.ElementAt(row).GoodsDescription?.ToUpper();
            dataBulk[row, 11 - 1] = records.ElementAt(row).PackageQty;
            dataBulk[row, 12 - 1] = records.ElementAt(row).CargoWeight;
            dataBulk[row, 13 - 1] = _imo?.ToUpper();

        });

        Range.Value = dataBulk;


        #region Footer

        var footer = rows + startRow + 5;

        Range = WorkSheet.Range[WorkSheet.Cells[footer - 2, 1], WorkSheet.Cells[footer - 2, 2]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New"; // Установка шрифта
        Range.Font.Bold = true;
        Range.Font.Size = 12;
        Range.Value = "LAST PAGE";

        Range = WorkSheet.Range[WorkSheet.Cells[footer - 2, 12], WorkSheet.Cells[footer - 2, 13]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New"; // Установка шрифта
        Range.Font.Bold = true;
        Range.Font.Size = 12;
        Range.Value = "LAST PAGE";

        Range = WorkSheet.Range[WorkSheet.Cells[footer - 2, 1], WorkSheet.Cells[footer - 2, 13]];
        // Настройка нижней границы
        Range.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous; // Сплошная линия
        Range.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = Excel.XlBorderWeight.xlMedium;     // жирная линия



        Range = WorkSheet.Range[WorkSheet.Cells[footer, 5], WorkSheet.Cells[footer, 5]];
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        Range.Value = "Quantity";

        Range = WorkSheet.Range[WorkSheet.Cells[footer, 6], WorkSheet.Cells[footer, 8]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        //Range.Cells.Borders.Value = true;
        //Range.Cells.Borders.Weight = 2;
        Range.Value = "Tare weight, kgs";

        Range = WorkSheet.Range[WorkSheet.Cells[footer, 9], WorkSheet.Cells[footer, 10]];
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;

        WorkSheet.Cells[footer, 9] = "Cargo weight, kgs";
        WorkSheet.Cells[footer, 10] = "Container tare + cargo weight, kgs";

        WorkSheet.Cells[footer + 1, 4] = "Full Container Loaded 20'";
        WorkSheet.Cells[footer + 2, 4] = "Full Container Loaded 40'";
        WorkSheet.Cells[footer + 3, 4] = "Empty Container 20'";
        WorkSheet.Cells[footer + 4, 4] = "Empty Container 40'";

        Range = WorkSheet.Range[WorkSheet.Cells[footer + 6, 4], WorkSheet.Cells[footer + 6, 4]];
        Range.Font.Size = 12; // Например, 12pt
        Range.Font.Bold = true;  // Жирный шрифт
        Range.Value = "Total";

        var _records = records.DistinctBy(s => s.ContainerNum);

        var full_20_qty = _records.Where(s => s.ContainerType.Substring(0, 2) == "20").Where(s => !s.IsEmpty).Count();
        var full_40_qty = _records.Where(s => s.ContainerType.Substring(0, 2) == "40").Where(s => !s.IsEmpty).Count();
        var mty_20_qty = _records.Where(s => s.ContainerType.Substring(0, 2) == "20").Where(s => s.IsEmpty).Count();
        var mty_40_qty = _records.Where(s => s.ContainerType.Substring(0, 2) == "40").Where(s => s.IsEmpty).Count();

        var full_20_tare = records.Where(s => s.ContainerType.Substring(0, 2) == "20").Where(s => !s.IsEmpty).Sum(s => s.TareWeight);
        var full_40_tare = records.Where(s => s.ContainerType.Substring(0, 2) == "40").Where(s => !s.IsEmpty).Sum(s => s.TareWeight);
        var mty_20_tare = records.Where(s => s.ContainerType.Substring(0, 2) == "20").Where(s => s.IsEmpty).Sum(s => s.TareWeight);
        var mty_40_tare = records.Where(s => s.ContainerType.Substring(0, 2) == "40").Where(s => s.IsEmpty).Sum(s => s.TareWeight);

        var full_20_wt = records.Where(s => s.ContainerType.Substring(0, 2) == "20").Where(s => !s.IsEmpty).Sum(s => s.CargoWeight);
        var full_40_wt = records.Where(s => s.ContainerType.Substring(0, 2) == "40").Where(s => !s.IsEmpty).Sum(s => s.CargoWeight);
        // var mty_20_wt = records.Where(s => s.ContainerType.Substring(0, 2) == "20").Where(s => s.IsEmpty).Sum(s => s.CargoWeight);
        //var mty_40_wt = records.Where(s => s.ContainerType.Substring(0, 2) == "40").Where(s => s.IsEmpty).Sum(s => s.CargoWeight);

        #region Container tare weight, kgs
        Range = WorkSheet.Range[WorkSheet.Cells[footer + 1, 6], WorkSheet.Cells[footer + 1, 8]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New"; // Установка шрифта
        Range.Font.Size = 12; // Например, 12pt
        Range.Font.Bold = true;  // Жирный шрифт
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###";

        if (full_20_tare != 0)
            Range.Value = full_20_tare;
        else
            Range.Value = "-";


        Range = WorkSheet.Range[WorkSheet.Cells[footer + 2, 6], WorkSheet.Cells[footer + 2, 8]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New"; // Установка шрифта
        Range.Font.Size = 12; // Например, 12pt
        //Range.Cells.Borders.Value = true;
        //Range.Cells.Borders.Weight = 2;
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###";
        if (full_40_tare != 0)
            Range.Value = full_40_tare;
        else
            Range.Value = "-";

        Range = WorkSheet.Range[WorkSheet.Cells[footer + 3, 6], WorkSheet.Cells[footer + 3, 8]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New"; // Установка шрифта
        Range.Font.Size = 12; // Например, 12pt
        //Range.Cells.Borders.Value = true;
        //Range.Cells.Borders.Weight = 2;
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###";
        if (mty_20_tare != 0)
            Range.Value = mty_20_tare;
        else
            Range.Value = "-";


        Range = WorkSheet.Range[WorkSheet.Cells[footer + 4, 6], WorkSheet.Cells[footer + 4, 8]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New"; // Установка шрифта
        Range.Font.Size = 12; // Например, 12pt
        //Range.Cells.Borders.Value = true;
        //Range.Cells.Borders.Weight = 2;
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###";
        if (mty_40_tare != 0)
            Range.Value = mty_40_tare;
        else
            Range.Value = "-";

        Range = WorkSheet.Range[WorkSheet.Cells[footer + 6, 6], WorkSheet.Cells[footer + 6, 8]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New"; // Установка шрифта
        Range.Font.Size = 12; // Например, 12pt
        Range.Font.Bold = true;  // Жирный шрифт
        //Range.Cells.Borders.Value = true;
        //Range.Cells.Borders.Weight = 2;
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###";
        if ((full_20_tare + full_40_tare + mty_20_tare + mty_40_tare) != 0)
            Range.Value = full_20_tare + full_40_tare + mty_20_tare + mty_40_tare;
        else
            Range.Value = "-";

        #endregion

        #region Quantity
        Range = WorkSheet.Range[WorkSheet.Cells[footer + 1, 5], WorkSheet.Cells[footer + 4, 5]];
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New"; // Установка шрифта
        Range.Font.Size = 12; // Например, 12pt
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###";

        Range = WorkSheet.Range[WorkSheet.Cells[footer + 6, 5], WorkSheet.Cells[footer + 6, 5]];
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New"; // Установка шрифта
        Range.Font.Size = 12; // Например, 12pt
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###";

        if (full_20_qty != 0)
            WorkSheet.Cells[footer + 1, 5] = full_20_qty;
        else
            WorkSheet.Cells[footer + 1, 5] = "-";

        if (full_40_qty != 0)
            WorkSheet.Cells[footer + 2, 5] = full_40_qty;
        else
            WorkSheet.Cells[footer + 2, 5] = "-";

        if (mty_20_qty != 0)
            WorkSheet.Cells[footer + 3, 5] = mty_20_qty;
        else
            WorkSheet.Cells[footer + 3, 5] = "-";

        if (mty_40_qty != 0)
            WorkSheet.Cells[footer + 4, 5] = mty_40_qty;
        else
            WorkSheet.Cells[footer + 4, 5] = "-";



        WorkSheet.Cells[footer + 6, 5] = _records.Count();


        #endregion


        Range = WorkSheet.Range[WorkSheet.Cells[footer + 1, 9], WorkSheet.Cells[footer + 4, 10]];
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New"; // Установка шрифта
        Range.Font.Size = 12; // Например, 12pt
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###.000"; //"# ### ### ##0.000";

        Range = WorkSheet.Range[WorkSheet.Cells[footer + 6, 9], WorkSheet.Cells[footer + 6, 10]];
        Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New"; // Установка шрифта
        Range.Font.Size = 12; // Например, 12pt
        Range.Font.Bold = true;  // Жирный шрифт
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###.000";//"# ### ### ##0.000";

        WorkSheet.Cells[footer + 1, 9] = full_20_wt != 0 ? full_20_wt : "-";
        WorkSheet.Cells[footer + 2, 9] = full_40_wt != 0 ? full_40_wt : "-";
        WorkSheet.Cells[footer + 3, 9] = "-";
        WorkSheet.Cells[footer + 4, 9] = "-";


        WorkSheet.Cells[footer + 6, 9] = (full_20_wt + full_40_wt) != 0 ? (full_20_wt + full_40_wt) : "-";


        WorkSheet.Cells[footer + 1, 10] = full_20_wt + full_20_tare != 0 ? full_20_wt + full_20_tare : "-";
        WorkSheet.Cells[footer + 2, 10] = full_40_wt + full_40_tare != 0 ? full_40_wt + full_40_tare : "-";
        WorkSheet.Cells[footer + 3, 10] = mty_20_tare != 0 ? mty_20_tare : "-";
        WorkSheet.Cells[footer + 4, 10] = mty_40_tare != 0 ? mty_40_tare : "-";

        WorkSheet.Cells[footer + 6, 10] = full_20_wt + full_20_tare + full_40_wt + full_40_tare + mty_20_tare + mty_40_tare != 0 ? full_20_wt + full_20_tare + full_40_wt + full_40_tare + mty_20_tare + mty_40_tare : "-";

        #endregion



        SaveTempFile();

        byte[] fileBytes = File.ReadAllBytes(TemporaryFilePath);


        return fileBytes;

    }




}
