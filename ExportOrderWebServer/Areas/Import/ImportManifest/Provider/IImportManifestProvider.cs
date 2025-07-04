using ExportOrderEntites.BillofLading;
using ExportOrderEntites.BillofLading.Dto;
using ExportOrderEntites.ImportVesselCall;
using ExportOrderEntites.ImportVesselCall.Dto;
using ExportOrderWebServer.DataSet;
using Microsoft.Office.Interop.Excel;

namespace ExportOrderWebServer.Areas.Import.ImportManifest.Provider
{
    public interface IImportManifestProvider
    {
        Task AddRecords(IEnumerable<BillOfLadingDto> records, string userName);
        Task<IEnumerable<BillOfLadingDto>> GetItemsAsync(FilterParameters filter);
        Task<IEnumerable<BillOfLadingDto>> GetBillOfLadingsAsync(VesselCallDetailDTO VesselCallDetail);
        Task<BillOfLadingDto> GetBillofLadingAsync(string billofLadingNum);
        Task<IEnumerable<ManifestBillOfLadingDto>> GetBillofLadingsNumAsync(VesselCallDetailDTO VesselCallDetail);
        Task UpdateBillOfLadingContainerRecords(IEnumerable<BillOfLadingContainerRecordDto> records);
        Task<BillOfLadingContainerRecordDto> UpdateBillOfLadingContainerRecordAsync(BillOfLadingContainerRecordDto element);
        Task UpdateBillofLadingAsync(BillOfLadingDto billofLading);

        Task<ImportVesselCallDto> GetVesselCallData(string vesselCallId);
    }
    public class ImportManifestProvider : IImportManifestProvider
    {


        private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

        private AppObjectResponse appObjResponse = new();

        public ImportManifestProvider(IDbContextFactory<ApplicationDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddRecords(IEnumerable<BillOfLadingDto> records, string userName)
        {

            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var bolList = records.Select(s => s.Num);
                var existRecords = await db.Set<BillofLadingEntity>()
                                                    .Where(s => bolList.Any(b => b == s.Num))
                                                    .Select(s => s.Num)
                                                    .ToArrayAsync();


                foreach (var billofLading in records)
                    if (!existRecords.Any(s => s == billofLading.Num))
                    {
                        var BillofLading = new BillofLadingEntity()
                        {
                            UserName = userName,
                            Num = billofLading.Num,
                            ServiceCode = billofLading.ServiceCode,
                            IssueDate = billofLading.IssueDate,
                            SobDate = billofLading.SobDate,
                            ShipperName = billofLading.ShipperName,
                            ShipperAddress = billofLading.ShipperAddress,
                            ConsigneeName = billofLading.ConsigneeName,
                            ConsigneeAddress = billofLading.ConsigneeAddress,
                            ConsigneeTaxNo = billofLading.ConsigneeTaxNo,
                            NotifyName = billofLading.NotifyName,
                            NotifyAddress = billofLading.NotifyAddress,
                            NotifyEmail = billofLading.NotifyEmail,
                            AdditionalInfo = billofLading.AdditionalInfo,
                            POR = billofLading.POR,
                            POL = billofLading.POL,
                            POD = billofLading.POD,
                            ContainerRecords = billofLading.ContainerRecords.Select(rec => new BillofLadingContainerRecord()
                            {
                                UserName = userName,
                                ContainerNum = rec.ContainerNum,
                                ContainerType = rec.ContainerType,
                                TareWeight = rec.TareWeight,
                                CargoWeight = rec.CargoWeight,
                                PackageQty = rec.PackageQty,
                                CommodityCode = rec.CommodityCode,
                                GoodsDescription = rec.GoodsDescription,

                                IsAlcohol = rec.IsAlcohol,
                                IsMilitaryCargo = rec.IsMilitaryCargo,

                                IsSoc = rec.IsSoc,
                                IsRef = rec.IsRef,
                                IsOog = rec.IsOog,
                                IsImo = rec.IsImo,
                                SealNo = rec.SealNo,
                                SealShr = rec.SealShr,
                                SealOth = rec.SealOth,
                                TempSet = rec.TempSet,
                                CreatedAt = DateTimeOffset.UtcNow,
                                LockToken = DateTime.Now.Ticks,
                            }).ToList(),
                            CreatedAt = DateTimeOffset.UtcNow,
                            LockToken = DateTime.Now.Ticks,
                        };

                        db.Entry(BillofLading).State = EntityState.Added;

                        foreach (var containerRecord in BillofLading.ContainerRecords)
                            db.Entry(containerRecord).State = EntityState.Added;

                        var bug = db.ChangeTracker.DebugView.LongView;
                        await db.SaveChangesAsync();
                        db.ChangeTracker.Clear();
                    }
            }
        }

        public async Task<BillOfLadingDto> GetBillofLadingAsync(string billofLadingNum)
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;


                var BillofLading = await db.Set<BillofLadingEntity>().Include(s => s.ContainerRecords)
                    .Select(billofLading => new BillOfLadingDto()
                    {

                        Num = billofLading.Num,
                        ServiceCode = billofLading.ServiceCode,
                        IssueDate = billofLading.IssueDate,
                        SobDate = billofLading.SobDate,
                        ShipperName = billofLading.ShipperName,
                        ShipperNameRu = billofLading.ShipperNameRu,
                        ShipperAddress = billofLading.ShipperAddress,
                        ShipperAddressRu = billofLading.ShipperAddressRu,
                        ShipperCountryRu = billofLading.ShipperCountryRu,
                        ConsigneeName = billofLading.ConsigneeName,
                        ConsigneeNameRu = billofLading.ConsigneeNameRu,
                        ConsigneeAddress = billofLading.ConsigneeAddress,
                        ConsigneeAddressRu = billofLading.ConsigneeAddressRu,
                        ConsigneeTaxNo = billofLading.ConsigneeTaxNo,
                        ConsigneeCountryRu = billofLading.ConsigneeCountryRu,
                        NotifyName = billofLading.NotifyName,
                        NotifyAddress = billofLading.NotifyAddress,
                        NotifyEmail = billofLading.NotifyEmail,
                        AdditionalInfo = billofLading.AdditionalInfo,
                        POR = billofLading.POR,
                        POL = billofLading.POL,
                        POD = billofLading.POD,
                        Version = billofLading.Version,
                        ContainerRecords = billofLading.ContainerRecords.Select(rec => new BillOfLadingContainerRecordDto()
                        {
                            Id = rec.Id,
                            ContainerNum = rec.ContainerNum,
                            ContainerType = rec.ContainerType,
                            TareWeight = rec.TareWeight,
                            CargoWeight = rec.CargoWeight,
                            PackageQty = rec.PackageQty,
                            CommodityCode = rec.CommodityCode,
                            GoodsDescription = rec.GoodsDescription,
                            GoodsDescriptionRu = rec.GoodsDescriptionRu,

                            IsAlcohol = rec.IsAlcohol,
                            IsMilitaryCargo = rec.IsMilitaryCargo,

                            IsSoc = rec.IsSoc,
                            IsRef = rec.IsRef,
                            IsOog = rec.IsOog,
                            IsImo = rec.IsImo,
                            SealNo = rec.SealNo,
                            SealShr = rec.SealShr,
                            SealOth = rec.SealOth,
                            TempSet = rec.TempSet,
                            Version = rec.Version,
                        }).OrderBy(s => s.ContainerNum).ToList(),
                        CustomsDeliveryMode = billofLading.CustomsDeliveryMode,
                    }).FirstOrDefaultAsync(s => s.Num == billofLadingNum);
                return BillofLading;
            }
        }

        public async Task<IEnumerable<BillOfLadingDto>> GetBillOfLadingsAsync(VesselCallDetailDTO VesselCallDetail)
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var BillofLadings = await db.Set<ImportVesselCallDetail>()
                    .Include(s => s.BillofLadings).ThenInclude(s => s.ContainerRecords)
                    .AsNoTracking().Where(s => s.Id == VesselCallDetail.Id)
                    .SelectMany(vsl => vsl.BillofLadings, (XlPivotTableVersionList, billofLading) => new BillOfLadingDto()
                    {

                        Num = billofLading.Num,
                        ServiceCode = billofLading.ServiceCode,
                        IssueDate = billofLading.IssueDate,
                        SobDate = billofLading.SobDate,
                        ShipperName = billofLading.ShipperName,
                        ShipperNameRu = billofLading.ShipperNameRu,
                        ShipperAddress = billofLading.ShipperAddress,
                        ShipperAddressRu = billofLading.ShipperAddressRu,
                        ShipperCountryRu = billofLading.ShipperCountryRu,
                        ConsigneeName = billofLading.ConsigneeName,
                        ConsigneeNameRu = billofLading.ConsigneeNameRu,
                        ConsigneeAddress = billofLading.ConsigneeAddress,
                        ConsigneeAddressRu = billofLading.ConsigneeAddressRu,
                        ConsigneeCountryRu = billofLading.ConsigneeCountryRu,
                        ConsigneeTaxNo = billofLading.ConsigneeTaxNo,
                        NotifyName = billofLading.NotifyName,
                        NotifyAddress = billofLading.NotifyAddress,
                        NotifyEmail = billofLading.NotifyEmail,
                        AdditionalInfo = billofLading.AdditionalInfo,
                        POR = billofLading.POR,
                        POL = billofLading.POL,
                        POD = billofLading.POD,
                        Version = billofLading.Version,
                        ContainerRecords = billofLading.ContainerRecords.Select(rec => new BillOfLadingContainerRecordDto()
                        {
                            Id = rec.Id,
                            ContainerNum = rec.ContainerNum,
                            ContainerType = rec.ContainerType,
                            TareWeight = rec.TareWeight,
                            CargoWeight = rec.CargoWeight,
                            PackageQty = rec.PackageQty,
                            CommodityCode = rec.CommodityCode,
                            GoodsDescription = rec.GoodsDescription,
                            GoodsDescriptionRu = rec.GoodsDescriptionRu,

                            IsAlcohol = rec.IsAlcohol,
                            IsMilitaryCargo = rec.IsMilitaryCargo,

                            IsSoc = rec.IsSoc,
                            IsRef = rec.IsRef,
                            IsOog = rec.IsOog,
                            IsImo = rec.IsImo,
                            SealNo = rec.SealNo,
                            SealShr = rec.SealShr,
                            SealOth = rec.SealOth,
                            TempSet = rec.TempSet,
                            Version = rec.Version,
                        }).OrderBy(s => s.ContainerNum).ToList(),
                        CustomsDeliveryMode = billofLading.CustomsDeliveryMode,
                    }).OrderBy(s => s.Num).ToArrayAsync();

                return BillofLadings;
            }
        }

        public async Task<IEnumerable<ManifestBillOfLadingDto>> GetBillofLadingsNumAsync(VesselCallDetailDTO VesselCallDetail)
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var BillofLadings = await db.Set<ImportVesselCallDetail>()
                    .Include(s => s.BillofLadings).ThenInclude(s => s.ContainerRecords)
                    .AsNoTracking().Where(s => s.Id == VesselCallDetail.Id)
                    .SelectMany(vsl => vsl.BillofLadings, (vsl, bl) => new ManifestBillOfLadingDto()
                    {
                        Num = bl.Num,
                        IsImo = bl.ContainerRecords.Any(s => s.IsImo),
                        IsOog = bl.ContainerRecords.Any(s => s.IsOog),
                        IsRef = bl.ContainerRecords.Any(s => s.IsRef),
                        IsSoc = bl.ContainerRecords.Any(s => s.IsSoc),
                        IsAlcohol = bl.ContainerRecords.Any(s => s.IsAlcohol),
                        IsMilitaryCargo = bl.ContainerRecords.Any(s => s.IsMilitaryCargo),
                        HasTranslate = bl.ContainerRecords.Any(s => !string.IsNullOrWhiteSpace(s.GoodsDescriptionRu)),
                        HasShipper = (!string.IsNullOrWhiteSpace(bl.ShipperCountryRu) || !string.IsNullOrWhiteSpace(bl.ShipperAddressRu) || !string.IsNullOrWhiteSpace(bl.ShipperNameRu)),
                        HasConsignee = (!string.IsNullOrWhiteSpace(bl.ConsigneeNameRu) || !string.IsNullOrWhiteSpace(bl.ConsigneeCountryRu) || !string.IsNullOrWhiteSpace(bl.ConsigneeAddressRu)),

                    }).OrderBy(s => s.Num).ToArrayAsync();

                return BillofLadings;
            }
        }

        public async Task<IEnumerable<BillOfLadingDto>> GetItemsAsync(FilterParameters filter)
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var containersNum = filter.CntrsNum.Any() ? filter.CntrsNum : string.IsNullOrWhiteSpace(filter.CntrNum) ? new() : new() { filter.CntrNum };
                var billofLadingsNum = filter.BolsNo.Any() ? filter.BolsNo : string.IsNullOrWhiteSpace(filter.BolNo) ? new() : new() { filter.BolNo };


                var BillofLadings = await db.Set<BillofLadingEntity>().Include(s => s.ContainerRecords)
                    .Where(s => filter.BolNo == "*" ? true : !billofLadingsNum.Any() || billofLadingsNum.Any(c => c == s.Num))
                    .Where(s => !containersNum.Any() || s.ContainerRecords.Any(cntr => containersNum.Any(c => c == cntr.ContainerNum)))


                    .Select(billofLading => new BillOfLadingDto()
                    {

                        Num = billofLading.Num,
                        ServiceCode = billofLading.ServiceCode,
                        IssueDate = billofLading.IssueDate,
                        SobDate = billofLading.SobDate,
                        ShipperName = billofLading.ShipperName,
                        ShipperAddress = billofLading.ShipperAddress,
                        ConsigneeName = billofLading.ConsigneeName,
                        ConsigneeAddress = billofLading.ConsigneeAddress,
                        ConsigneeTaxNo = billofLading.ConsigneeTaxNo,
                        NotifyName = billofLading.NotifyName,
                        NotifyAddress = billofLading.NotifyAddress,
                        NotifyEmail = billofLading.NotifyEmail,
                        AdditionalInfo = billofLading.AdditionalInfo,
                        POR = billofLading.POR,
                        POL = billofLading.POL,
                        POD = billofLading.POD,
                        ContainerRecords = billofLading.ContainerRecords.Select(rec => new BillOfLadingContainerRecordDto()
                        {
                            Id = rec.Id,
                            ContainerNum = rec.ContainerNum,
                            ContainerType = rec.ContainerType,
                            TareWeight = rec.TareWeight,
                            CargoWeight = rec.CargoWeight,
                            PackageQty = rec.PackageQty,
                            CommodityCode = rec.CommodityCode,
                            GoodsDescription = rec.GoodsDescription,

                            IsAlcohol = rec.IsAlcohol,
                            IsMilitaryCargo = rec.IsMilitaryCargo,

                            IsSoc = rec.IsSoc,
                            IsRef = rec.IsRef,
                            IsOog = rec.IsOog,
                            IsImo = rec.IsImo,
                            SealNo = rec.SealNo,
                            SealShr = rec.SealShr,
                            SealOth = rec.SealOth,
                            TempSet = rec.TempSet,
                            Version = rec.Version,
                        }).ToList(),
                        Version = billofLading.Version
                    }).ToArrayAsync();
                return BillofLadings;
            }
        }

        public async Task<ImportVesselCallDto> GetVesselCallData(string vesselCallId)
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;


                var VesselCall = await db.Set<ImportVesselCallDetail>().Include(s => s.VesselCall).ThenInclude(s => s.Vessel).ThenInclude(s => s.Flag)
                    .Include(s => s.VesselCall).ThenInclude(s => s.Terminal).ThenInclude(s => s.Customs)
                    .Include(s => s.POL).ThenInclude(s => s.Country)
                    .Include(s => s.BillofLadings).ThenInclude(s => s.ContainerRecords)
                    .Where(s => s.Id.ToString() == vesselCallId)
                    .Select(vsl => new ImportVesselCallDto()
                    {
                        VesselFlag = vsl.VesselCall.Vessel.Flag.RUS,
                        VesselName = vsl.VesselCall.Vessel.Name!,
                        VesselVoyage = vsl.VesselCall.VoyageNo,
                        ETA = vsl.VesselCall.ETA!.Value,
                        DeparturePortName = vsl.POL.NameEn,
                        DeparturePortCode = vsl.POL.UnLocode,
                        DeparturePortCountryCode = vsl.POL.Country.Code,
                        CustomsPostCode = vsl.VesselCall.Terminal.Customs.Code,
                        BillofLadings = vsl.BillofLadings.Select(billofLading => new BillOfLadingDto()
                        {

                            Num = billofLading.Num,
                            ServiceCode = billofLading.ServiceCode,
                            IssueDate = billofLading.IssueDate,
                            SobDate = billofLading.SobDate,
                            ShipperName = billofLading.ShipperName,
                            ShipperNameRu = billofLading.ShipperNameRu,
                            ShipperAddress = billofLading.ShipperAddress,
                            ShipperAddressRu = billofLading.ShipperAddressRu,
                            ShipperCountryRu = billofLading.ShipperCountryRu,
                            ConsigneeName = billofLading.ConsigneeName,
                            ConsigneeNameRu = billofLading.ConsigneeNameRu,
                            ConsigneeAddress = billofLading.ConsigneeAddress,
                            ConsigneeAddressRu = billofLading.ConsigneeAddressRu,
                            ConsigneeTaxNo = billofLading.ConsigneeTaxNo,
                            ConsigneeCountryRu = billofLading.ConsigneeCountryRu,
                            NotifyName = billofLading.NotifyName,
                            NotifyAddress = billofLading.NotifyAddress,
                            NotifyEmail = billofLading.NotifyEmail,
                            AdditionalInfo = billofLading.AdditionalInfo,
                            POR = billofLading.POR,
                            POL = billofLading.POL,
                            POD = billofLading.POD,
                            Version = billofLading.Version,
                            ContainerRecords = billofLading.ContainerRecords.Select(rec => new BillOfLadingContainerRecordDto()
                            {
                                Id = rec.Id,
                                ContainerNum = rec.ContainerNum,
                                ContainerType = rec.ContainerType,
                                TareWeight = rec.TareWeight,
                                CargoWeight = rec.CargoWeight,
                                PackageQty = rec.PackageQty,
                                CommodityCode = rec.CommodityCode,
                                GoodsDescription = rec.GoodsDescription,
                                GoodsDescriptionRu = rec.GoodsDescriptionRu,

                                IsAlcohol = rec.IsAlcohol,
                                IsMilitaryCargo = rec.IsMilitaryCargo,

                                IsSoc = rec.IsSoc,
                                IsRef = rec.IsRef,
                                IsOog = rec.IsOog,
                                IsImo = rec.IsImo,
                                SealNo = rec.SealNo,
                                SealShr = rec.SealShr,
                                SealOth = rec.SealOth,
                                TempSet = rec.TempSet,
                                Version = rec.Version,
                            }).OrderBy(s => s.ContainerNum).ToList(),
                            CustomsDeliveryMode = billofLading.CustomsDeliveryMode,
                        }).ToList(),
                    })
                    .FirstOrDefaultAsync();

                return VesselCall!;

            }
        }

        public async Task UpdateBillofLadingAsync(BillOfLadingDto billofLading)
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var UpdateBillofLading = await db.Set<BillofLadingEntity>().AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Num == billofLading.Num);
                if (UpdateBillofLading is null)
                    throw new InvalidOperationException($"Bill of Lading with number {billofLading.Num} is not existed");
                if (UpdateBillofLading.Version != billofLading.Version)
                    throw new InvalidOperationException($"Bill of Lading with number {billofLading.Num} has been updated by another user"); ;

                if (billofLading.HasSwitched)
                {
                    var IsExistBillofLading = await db.Set<BillofLadingEntity>().AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Num == billofLading.Num);
                    if (IsExistBillofLading != null)
                        throw new InvalidOperationException($"Bill of Lading with number {billofLading.Num} already exists");

                    UpdateBillofLading.Num = billofLading.Num?.ToUpper();
                   // UpdateBillofLading.HasSwitched =true;
                }

                

                UpdateBillofLading.ShipperNameRu = billofLading.ShipperNameRu?.ToUpper();
                UpdateBillofLading.ShipperAddressRu = billofLading.ShipperAddressRu?.ToUpper();
                UpdateBillofLading.ShipperCountryRu = billofLading.ShipperCountryRu?.ToUpper();
                UpdateBillofLading.ConsigneeNameRu = billofLading.ConsigneeNameRu?.ToUpper();
                UpdateBillofLading.ConsigneeAddressRu = billofLading.ConsigneeAddressRu?.ToUpper();
                UpdateBillofLading.ConsigneeCountryRu = billofLading.ConsigneeCountryRu?.ToUpper();
                UpdateBillofLading.CustomsDeliveryMode = billofLading.CustomsDeliveryMode.HasValue ? billofLading.CustomsDeliveryMode.Value : CustomsDeliveryMode.GTD;
                //UpdateBillofLading.SobDate = billofLading.SobDate;
                //UpdateBillofLading.IssueDate = billofLading.IssueDate;

                db.Entry(UpdateBillofLading).State = EntityState.Modified;
                await db.SaveChangesAsync();

            }
        }

        public async Task<BillOfLadingContainerRecordDto> UpdateBillOfLadingContainerRecordAsync(BillOfLadingContainerRecordDto element)
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var record = await db.Set<BillofLadingContainerRecord>().AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == element.Id);

                if (record == null) return element;
                if (record.Version != element.Version)
                    await db.Set<BillofLadingContainerRecord>().AsNoTracking()
                        .Select(rec => new BillOfLadingContainerRecordDto()
                        {
                            Id = rec.Id,
                            ContainerNum = rec.ContainerNum,
                            ContainerType = rec.ContainerType,
                            TareWeight = rec.TareWeight,
                            CargoWeight = rec.CargoWeight,
                            PackageQty = rec.PackageQty,
                            CommodityCode = rec.CommodityCode,
                            GoodsDescription = rec.GoodsDescription,

                            IsAlcohol = rec.IsAlcohol,
                            IsMilitaryCargo = rec.IsMilitaryCargo,

                            IsSoc = rec.IsSoc,
                            IsRef = rec.IsRef,
                            IsOog = rec.IsOog,
                            IsImo = rec.IsImo,
                            SealNo = rec.SealNo,
                            SealShr = rec.SealShr,
                            SealOth = rec.SealOth,
                            TempSet = rec.TempSet,
                            Version = rec.Version,
                        })
                        .FirstOrDefaultAsync(s => s.Id == element.Id);
                ;
                record.UpdatedAt = DateTimeOffset.Now;
                record.CommodityCode = element.CommodityCode;
                record.GoodsDescription = element.GoodsDescription;
                record.GoodsDescriptionRu = element.GoodsDescriptionRu;
                db.Entry(record).State = EntityState.Modified;
                await db.SaveChangesAsync();
                var updateRecord = await db.Set<BillofLadingContainerRecord>().AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == element.Id);
                element.Version = updateRecord!.Version;
                return element;
            }
        }

        public async Task UpdateBillOfLadingContainerRecords(IEnumerable<BillOfLadingContainerRecordDto> records)
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                foreach (var record in records)
                {

                    var updateRecord = await db.Set<BillofLadingContainerRecord>().AsNoTracking()
                                               .FirstOrDefaultAsync(s => s.Id == record.Id);

                    if (updateRecord is null) continue;
                    if (updateRecord.Version != record.Version) continue;


                    updateRecord.GoodsDescription = record.GoodsDescription;
                    updateRecord.GoodsDescriptionRu = record.GoodsDescriptionRu;
                    updateRecord.UpdatedAt = DateTimeOffset.Now;

                    db.Entry(updateRecord).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }


            }
        }
    }
}
