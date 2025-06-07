using ExportOrderEntites.BillofLading;
using ExportOrderEntites.BillofLading.Dto;
using ExportOrderEntites.ImportVesselCall;
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
                                CreatedAt = new DateTimeOffset().UtcDateTime,
                                LockToken = DateTime.Now.Ticks,
                            }).ToList(),
                            CreatedAt = new DateTimeOffset().UtcDateTime,
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
                        ConsigneeName = billofLading.ConsigneeName,
                        ConsigneeNameRu = billofLading.ConsigneeNameRu,
                        ConsigneeAddress = billofLading.ConsigneeAddress,
                        ConsigneeAddressRu = billofLading.ConsigneeAddressRu,
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
                            ConsigneeName = billofLading.ConsigneeName,
                            ConsigneeNameRu = billofLading.ConsigneeNameRu,
                            ConsigneeAddress = billofLading.ConsigneeAddress,
                            ConsigneeAddressRu = billofLading.ConsigneeAddressRu,
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

        public async Task UpdateBillofLadingAsync(BillOfLadingDto billofLading)
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var UpdateBillofLading = await db.Set<BillofLadingEntity>().AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Num == billofLading.Num);
                if (UpdateBillofLading is null) return;
                if (UpdateBillofLading.Version != billofLading.Version) return;

                UpdateBillofLading.ShipperNameRu = billofLading.ShipperNameRu;
                UpdateBillofLading.ShipperAddressRu = billofLading.ShipperAddressRu;
                UpdateBillofLading.ShipperCountryRu = billofLading.ShipperCountryRu;
                UpdateBillofLading.ConsigneeNameRu = billofLading.ConsigneeNameRu;
                UpdateBillofLading.ConsigneeAddressRu = billofLading.ConsigneeAddressRu;
                UpdateBillofLading.ConsigneeCountryRu = billofLading.ConsigneeCountryRu;
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
