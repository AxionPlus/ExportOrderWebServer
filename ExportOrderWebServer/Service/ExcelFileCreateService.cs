using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
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
    Task<byte[]> CreateExcelFromPdf(string pdfPath);
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
            int rows = item.ExportOrderRecordsDTO.Count;

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

        WorkSheet.Cells[2, 1] = VesselCall.VesselName.ToUpper();
        WorkSheet.Cells[2, 5] = VesselCall.VesselVoyage.ToUpper();
        WorkSheet.Cells[2, 2] = VesselCall.VesselFlag.ToUpper();
        WorkSheet.Cells[2, 4] = VesselCall.DeparturePortName.ToUpper();

        WorkSheet.Cells[1, 10] = "";  //Captain Surname
        WorkSheet.Cells[2, 10] = "";  //Captain Name

        WorkSheet.Cells[2, 15] = VesselCall.CustomsPostCode.ToUpper();

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
        int rows = records.Count;

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
                _ => "ГТД"
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

        WorkSheet.Cells[3, 3] = VesselCall.VesselName.ToUpper();
        WorkSheet.Cells[3, 7] = VesselCall.VesselVoyage.ToUpper();
        WorkSheet.Cells[4, 3] = VesselCall.VesselFlag.ToUpper();
        WorkSheet.Cells[7, 3] = VesselCall.DeparturePortName.ToUpper();
        WorkSheet.Cells[6, 3] = VesselCall.ETA;

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
        int rows = records.Count;

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
                _ => "ГТД"
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

        WorkSheet.Cells[1, 3] = VesselCall.VesselName.ToUpper();
        WorkSheet.Cells[2, 3] = VesselCall.VesselVoyage.ToUpper();
        WorkSheet.Cells[1, 5] = VesselCall.VesselFlag.ToUpper();
        WorkSheet.Cells[1, 10] = VesselCall.DeparturePortName.ToUpper();
        WorkSheet.Cells[1, 12] = VesselCall.ETA;

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
        int rows = records.Count;

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
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New";
        Range.Font.Bold = true;
        Range.Font.Size = 12;
        Range.Value = "LAST PAGE";

        Range = WorkSheet.Range[WorkSheet.Cells[footer - 2, 12], WorkSheet.Cells[footer - 2, 13]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New";
        Range.Font.Bold = true;
        Range.Font.Size = 12;
        Range.Value = "LAST PAGE";

        Range = WorkSheet.Range[WorkSheet.Cells[footer - 2, 1], WorkSheet.Cells[footer - 2, 13]];
        // Настройка нижней границы
        Range.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous; // Сплошная линия
        Range.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = Excel.XlBorderWeight.xlMedium;     // жирная линия

        Range = WorkSheet.Range[WorkSheet.Cells[footer, 5], WorkSheet.Cells[footer, 5]];
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
        Range.Value = "Quantity";

        Range = WorkSheet.Range[WorkSheet.Cells[footer, 6], WorkSheet.Cells[footer, 8]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
        //Range.Cells.Borders.Value = true;
        //Range.Cells.Borders.Weight = 2;
        Range.Value = "Tare weight, kgs";

        Range = WorkSheet.Range[WorkSheet.Cells[footer, 9], WorkSheet.Cells[footer, 10]];
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

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

        #region Container tare weight, kgs
        Range = WorkSheet.Range[WorkSheet.Cells[footer + 1, 6], WorkSheet.Cells[footer + 1, 8]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New";
        Range.Font.Size = 12;
        Range.Font.Bold = true;
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###";

        if (full_20_tare != 0)
            Range.Value = full_20_tare;
        else
            Range.Value = "-";

        Range = WorkSheet.Range[WorkSheet.Cells[footer + 2, 6], WorkSheet.Cells[footer + 2, 8]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New";
        Range.Font.Size = 12;
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###";
        if (full_40_tare != 0)
            Range.Value = full_40_tare;
        else
            Range.Value = "-";

        Range = WorkSheet.Range[WorkSheet.Cells[footer + 3, 6], WorkSheet.Cells[footer + 3, 8]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New";
        Range.Font.Size = 12;
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###";
        if (mty_20_tare != 0)
            Range.Value = mty_20_tare;
        else
            Range.Value = "-";

        Range = WorkSheet.Range[WorkSheet.Cells[footer + 4, 6], WorkSheet.Cells[footer + 4, 8]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New";
        Range.Font.Size = 12;
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###";
        if (mty_40_tare != 0)
            Range.Value = mty_40_tare;
        else
            Range.Value = "-";

        Range = WorkSheet.Range[WorkSheet.Cells[footer + 6, 6], WorkSheet.Cells[footer + 6, 8]];
        Range.Cells.Merge();
        Range.WrapText = false;
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New";
        Range.Font.Size = 12;
        Range.Font.Bold = true;
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###";
        if ((full_20_tare + full_40_tare + mty_20_tare + mty_40_tare) != 0)
            Range.Value = full_20_tare + full_40_tare + mty_20_tare + mty_40_tare;
        else
            Range.Value = "-";

        #endregion

        #region Quantity
        Range = WorkSheet.Range[WorkSheet.Cells[footer + 1, 5], WorkSheet.Cells[footer + 4, 5]];
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New";
        Range.Font.Size = 12;
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###";

        Range = WorkSheet.Range[WorkSheet.Cells[footer + 6, 5], WorkSheet.Cells[footer + 6, 5]];
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New";
        Range.Font.Size = 12;
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
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New";
        Range.Font.Size = 12;
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###.000"; //"# ### ### ##0.000";

        Range = WorkSheet.Range[WorkSheet.Cells[footer + 6, 9], WorkSheet.Cells[footer + 6, 10]];
        Range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
        Range.Font.Name = "Courier New";
        Range.Font.Size = 12;
        Range.Font.Bold = true;
        Range.Cells.NumberFormat = $"#{(char)160}###{(char)160}###.000";

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

    public async Task<byte[]> CreateExcelFromPdf(string pdfPath)
    {
        /// Read Pdf
        var extractedData = ExtractTextFromPdf(pdfPath);

        if (extractedData is null || !extractedData.Any() ||
            extractedData.First() != "SHIPPER / ON BEHALF OF SHIPPER")
            return Array.Empty<byte>();

        int seq = 0;
        
        try
        {            
            List<List<string>> data = new();
            List<string> dataLine = new();

            foreach (string line in extractedData)
            {
                if (line == "SHIPPER / ON BEHALF OF SHIPPER")
                {
                    if (dataLine.Any())
                        data.Add(dataLine);

                    dataLine = new();
                }

                dataLine.Add(line);
            }

            if (dataLine.Any())
                data.Add(dataLine);

            List<ReadPdfExportOrderDTO> exportOrders = new();
            
            foreach (var order in data)
            {                
                ReadPdfExportOrderDTO exportOrderDTO = new();

                string[] lines = order.ToArray();

                /// SHIPPER
                int indexConsignee = Array.IndexOf(lines, "CONSIGNEE / ON BEHALF OF CONSIGNEE");
                if (indexConsignee < 0)
                    indexConsignee = Array.IndexOf(lines, "CONSIGNEE / ON BEHALF OF");

                StringBuilder shipper = new();

                for (int i = 1; i < indexConsignee; i++)
                {
                    if (//lines[i] == "SHIPPER / ON BEHALF OF SHIPPER" ||
                        lines[i] == "Отправитель / Представитель отправителя" ||
                        lines[i].StartsWith("Отправитель / Представитель") ||
                        lines[i].StartsWith("отправителя") ||
                        lines[i] == "ПОРУЧЕНИЕ №" ||
                        lines[i] == "____________" ||
                        lines[i] == "НА ОТГРУЗКУ ЭКСПОРТНЫХ ТОВАРОВ" ||
                        lines[i].StartsWith("Экспортное разрешение №"))
                        continue;

                    string lineShipper = lines[i];

                    int indexTrashShipper = lineShipper.IndexOf("Экспортное разрешение №");                    
                    if (indexTrashShipper >= 0)
                        lineShipper = lineShipper.Substring(0, indexTrashShipper).Trim();

                    shipper.AppendLine(lineShipper.Trim());
                }
                
                exportOrderDTO.Shipper = shipper.ToString().Replace("\r\n", " ").Trim();

                /// CONSIGNEE                
                int indexNotify = Array.FindIndex(lines, s => s.StartsWith("NOTIFY PARTY"));
                StringBuilder consignee = new();

                for (int i = indexConsignee + 1; i < indexNotify; i++)
                {
                    if (lines[i] == "Получатель / Представитель получателя" ||
                        lines[i].StartsWith("Получатель / Представитель") ||
                        lines[i].StartsWith ("получателя") ||
                        lines[i].StartsWith("Экспортное разрешение №") ||
                        lines[i] == "НА ОТГРУЗКУ ЭКСПОРТНЫХ ТОВАРОВ" ||
                        lines[i] == "CONSIGNEE")
                        continue;

                    string lineConsignee = lines[i];

                    //if (Regex.IsMatch(lineConsignee, "[0-9]{2}.[0-9]{2}.[0-9]{4}"))
                    //    continue;

                    if (Regex.IsMatch(lineConsignee, @"^(0[1-9]|[12][0-9]|3[01])\.(0[1-9]|1[0-2])\.\d{4}$"))    // для даты
                        continue;

                    consignee.AppendLine(lineConsignee);
                }
                
                exportOrderDTO.Consignee = consignee.ToString().Replace("\r\n", " ").Trim();

                /// PORT OF DISCHARGE
                exportOrderDTO.POD = lines[Array.IndexOf(lines, "Порт выгрузки Пункт назначения груза") + 1];

                /// SHIPPING LINE
                string trashInLine = "Manager/менеджер (конт. телефон):";
                int indexShippingLine = Array.FindIndex(lines, s => s.StartsWith(trashInLine));
                string lineShippingLine = lines[indexShippingLine];
                string textShippingLine = lineShippingLine.Substring(trashInLine.Length);

                int indexEnd = textShippingLine.IndexOf(", оформил");
                if (indexEnd < 0)
                    indexEnd = textShippingLine.IndexOf(", телефон:");

                if (indexEnd < 0)
                    exportOrderDTO.ShippingLine = textShippingLine;
                else
                    exportOrderDTO.ShippingLine = textShippingLine.Substring(0, indexEnd);

                /// COMMODITY & Cntrs               
                int indexCommodity = Array.IndexOf(lines, "Товары") + 4;

                List<string> commodities = new();
                List<string> cntrNums = new();
                
                StringBuilder subCommodity = new();
                string trashInCommodity = "(код:";

                for (int i = indexCommodity; i < lines.Length; i++)
                {
                    string lineCommodity = lines[i];

                    /// Trash
                    if (lineCommodity.Contains(trashInCommodity, StringComparison.InvariantCultureIgnoreCase))
                    {
                        StringBuilder lineCleared = new();
                        string[] lineCommodityArray = lineCommodity.Split(trashInCommodity);
                        lineCleared.Append(lineCommodityArray[0]);

                        int endIndex = lineCommodityArray[1].IndexOf(')');

                        if (endIndex < lineCommodityArray[1].Length - 1)
                            lineCleared.Append(lineCommodityArray[1].Substring(endIndex + 1));

                        /// Cleared line
                        lineCommodity = lineCleared.ToString();
                    }

                    string[] lineArray = lineCommodity.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (lineArray.Length >= 10)
                    {
                        if (subCommodity.Length > 0)
                        {
                            commodities.Add(subCommodity.ToString().TrimEnd());
                            subCommodity = new();
                        }

                        /// Commodity                        
                        for (int j = 0; j < lineArray.Length - 9; j++)
                        {
                            subCommodity.Append(lineArray[j + 3] + " ");
                        }

                        /// Cntr
                        string possibleCntrNum = lineArray[^2];                     /// второй с конца элемент массива
                        if (Regex.IsMatch(possibleCntrNum, "[a-zA-Z]{4}[0-9]{7}"))  /// номер контейнера
                            cntrNums.Add(possibleCntrNum);
                    }
                    else
                    {
                        foreach (var word in lineArray)
                        {
                            if (Regex.IsMatch(word, "[a-zA-Z]{4}[0-9]{7}"))
                                cntrNums.Add(word);
                            else
                                subCommodity.Append(word + ' ');
                        }
                    }
                }

                if (subCommodity.Length > 0)
                    commodities.Add(subCommodity.ToString().TrimEnd());


                exportOrderDTO.Commodity = string.Join(", ", commodities.Distinct());

                exportOrderDTO.CntrsCount = cntrNums.Distinct().Count();

                exportOrderDTO.Id = ++seq;
                exportOrders.Add(exportOrderDTO);
            }

            ///-----------------------------------------------------------

            /// Create Excel
            TemplateFilePath = Path.Combine(DirResources, "ExportOrder List.xlsx");
            if (!File.Exists(TemplateFilePath)) return Array.Empty<byte>();

            CreateTempFile(TemporaryFilePath);

            if (WorkSheet is null) return Array.Empty<byte>();

            /// TABLE                
            int columns = 6;
            int rows = exportOrders.Count;

            int startRow = 2;

            var startCell = WorkSheet.Cells[startRow, 1];
            var endCell = WorkSheet.Cells[rows + startRow - 1, columns];
            Range = WorkSheet.Range[startCell, endCell];

            if (rows > 1) Range.FillDown();

            var dataBulk = new object[rows, columns];

            var result = Parallel.For(0, rows, (row, state) =>
            {
                dataBulk[row, 0] = exportOrders.ElementAt(row).Shipper!;
                dataBulk[row, 1] = exportOrders.ElementAt(row).Consignee!;
                dataBulk[row, 2] = exportOrders.ElementAt(row).CntrsCount;
                dataBulk[row, 3] = exportOrders.ElementAt(row).ShippingLine!;
                dataBulk[row, 4] = exportOrders.ElementAt(row).POD!;
                dataBulk[row, 5] = exportOrders.ElementAt(row).Commodity!;
            });

            Range.Value = dataBulk;

            SaveTempFile();

            byte[] fileBytes = File.ReadAllBytes(TemporaryFilePath);

            await Task.Run(async () => { await Task.Delay(SetDelay(exportOrders.Count)); });

            return fileBytes;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Record {seq + 1}: {ex.Message}");
            return Array.Empty<byte>();
        }
    }

    #region SUPPORT METHODS

    private void CreateTempFile(string destinationPath)
    {
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
       
    private List<string> ExtractTextFromPdf(string _pdfPath)
    {
        var result = new List<string>();

        try
        {
            using (var document = PdfDocument.Open(_pdfPath))
            {
                foreach (var page in document.GetPages())
                {
                    /// Получаем все слова на странице с их координатами
                    var words = page.GetWords();

                    /// Группируем слова по строкам (на основе Y-координат)
                    var lines = words.GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
                                     .OrderByDescending(g => g.Key);

                    foreach (var line in lines)
                    {
                        /// Сортируем слова по X-координате и объединяем в строку
                        var row = line.OrderBy(w => w.BoundingBox.Left)
                                     .Select(w => w.Text)
                                     .ToList();

                        //var rowText = string.Join(" | ", row);
                        var rowText = string.Join(" ", row);

                        result.Add(rowText);
                    }
                }
            }

            if (File.Exists(_pdfPath))
                File.Delete(_pdfPath);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            if (File.Exists(_pdfPath))
                File.Delete(_pdfPath);
            return result;
        }
    }

    //private List<List<string>> ____ExtractTextFromPdf(string _pdfPath)
    //{
    //    var result = new List<List<string>>();

    //    try
    //    {
    //        using (var document = PdfDocument.Open(_pdfPath))
    //        {
    //            // Создаем экстрактор слов
    //            IWordExtractor wordExtractor = new NearestNeighbourWordExtractor();

    //            foreach (var page in document.GetPages())
    //            {
    //                // Извлекаем слова с их координатами
    //                //var words = wordExtractor.GetWords(page.Letters).ToList();
    //                var words = page.GetWords().ToList();

    //                // Группируем слова по строкам (основано на Y-координате)
    //                var lines = GroupWordsIntoLines(words);

    //                // Обрабатываем каждую строку
    //                foreach (var line in lines.OrderByDescending(l => l.Key))
    //                {
    //                    // Сортируем слова в строке по X-координате (слева направо)
    //                    var sortedWords = line.Value.OrderBy(w => w.BoundingBox.Left)
    //                                                .Select(w => w.Text)
    //                                                .ToList();

    //                    // Формируем текст строки
    //                    //var lineText = string.Join(" ", sortedWords.Select(w => w.Text));
    //                    result.Add(sortedWords);
    //                }
    //            }
    //        }
    //        if (File.Exists(_pdfPath))
    //            File.Delete(_pdfPath);

    //        return result;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.Message);
    //        if (File.Exists(_pdfPath))
    //            File.Delete(_pdfPath);
    //        return result;
    //    }
    //}

    //private static Dictionary<double, List<Word>> GroupWordsIntoLines(List<Word> words)
    //{
    //    var lines = new Dictionary<double, List<Word>>();
    //    const double tolerance = 2.0; // Допуск для группировки по Y-координате

    //    foreach (var word in words)
    //    {
    //        var baseLine = Math.Round(word.BoundingBox.Bottom, 1);
    //        var existingLine = lines.Keys.FirstOrDefault(y => Math.Abs(y - baseLine) <= tolerance);

    //        if (existingLine != 0)
    //        {
    //            lines[existingLine].Add(word);
    //        }
    //        else
    //        {
    //            lines[baseLine] = new List<Word> { word };
    //        }
    //    }

    //    return lines;
    //}

    //private List<List<string>> ___ExtractTextFromPdf(string _pdfPath)
    //{
    //    var result = new List<List<string>>();

    //    try
    //    {
    //        using (var document = PdfDocument.Open(_pdfPath))
    //        {
    //            foreach (var page in document.GetPages())
    //            {
    //                /// Получаем все слова на странице с их координатами
    //                var words = page.GetWords();

    //                /// Группируем слова по строкам (на основе Y-координат)
    //                var lines = words.GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
    //                                 .OrderByDescending(g => g.Key);

    //                var currentTable = new List<string>();

    //                foreach (var line in lines)
    //                {
    //                    /// Сортируем слова по X-координате и объединяем в строку
    //                    var row = line.OrderBy(w => w.BoundingBox.Left)
    //                                 .Select(w => w.Text)
    //                                 .ToList();

    //                    //var rowText = string.Join(" | ", row);
    //                    var rowText = string.Join(" ", row);

    //                    currentTable.Add(rowText);
    //                    result.Add(new List<string>(currentTable));
    //                    currentTable.Clear();
    //                }

    //                //if (currentTable.Count > 0)
    //                //{
    //                //    result.Add(currentTable);
    //                //}
    //            }
    //        }

    //        if (File.Exists(_pdfPath))
    //            File.Delete(_pdfPath);

    //        return result;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.Message);
    //        if (File.Exists(_pdfPath))
    //            File.Delete(_pdfPath);
    //        return result;
    //    }
    //}

    //private List<List<string>> __ExtractTextFromPdf(string _pdfPath)
    //{
    //    var result = new List<List<string>>();

    //    try
    //    {
    //        using (var document = PdfDocument.Open(_pdfPath))
    //        {
    //            foreach (var page in document.GetPages())
    //            {
    //                /// Получаем все слова на странице с их координатами
    //                var words = page.GetWords();

    //                /// Группируем слова по строкам (на основе Y-координат)
    //                var lines = words.GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
    //                                 .OrderByDescending(g => g.Key);

    //                var currentTable = new List<string>();

    //                foreach (var line in lines)
    //                {
    //                    /// Сортируем слова по X-координате и объединяем в строку
    //                    var row = line.OrderBy(w => w.BoundingBox.Left)
    //                                 .Select(w => w.Text)
    //                                 .ToList();

    //                    var rowText = string.Join(" | ", row);

    //                    if (!string.IsNullOrWhiteSpace(rowText))
    //                    {
    //                        currentTable.Add(rowText);
    //                    }
    //                    else if (currentTable.Count > 0)
    //                    {
    //                        result.Add(new List<string>(currentTable));
    //                        currentTable.Clear();
    //                    }
    //                }

    //                if (currentTable.Count > 0)
    //                {
    //                    result.Add(currentTable);
    //                }
    //            }
    //        }

    //        if (File.Exists(_pdfPath))
    //            File.Delete(_pdfPath);

    //        return result;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.Message);            
    //        if (File.Exists(_pdfPath))
    //            File.Delete(_pdfPath);
    //        return result;
    //    }
    //}

    //private List<List<string>> _ExtractTextFromPdf(string _pdfPath)
    //{
    //    var result = new List<List<string>>();

    //    try
    //    {
    //        using (var document = PdfDocument.Open(_pdfPath))
    //        {
    //            foreach (var page in document.GetPages())
    //            {
    //                /// Получаем все слова на странице с их координатами
    //                var words = page.GetWords();

    //                /// Группируем слова по строкам (на основе Y-координат)
    //                var lines = words.GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
    //                                 .OrderByDescending(g => g.Key);

    //                var currentTable = new List<string>();

    //                foreach (var line in lines)
    //                {
    //                    /// Сортируем слова по X-координате и объединяем в строку
    //                    var row = line.OrderBy(w => w.BoundingBox.Left)
    //                                 .Select(w => w.Text)
    //                                 .ToList();

    //                    result.Add(row);                        
    //                }
    //            }
    //        }

    //        if (File.Exists(_pdfPath))
    //            File.Delete(_pdfPath);

    //        return result;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.Message);
    //        if (File.Exists(_pdfPath))
    //            File.Delete(_pdfPath);
    //        return result;
    //    }
    //}
    #endregion

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
}
