using ExportOrderEntites.BillofLading;
using ExportOrderEntites.BillofLading.Dto;
using ExportOrderEntites.ImportVesselCall;
using ExportOrderWebServer.Service;
using Microsoft.Office.Interop.Excel;

namespace ExportOrderWebServer.Areas.Import.BillofLadings.Provider
{
    public interface IBillofLadingProvider
    {
        Task AddRecords(IEnumerable<BillOfLadingDto> records, string vesselCallDetailId, string userName);
        Task UpdateRecordsFromXls(IEnumerable<BillOfLadingDto> records, string userName);
        Task<IEnumerable<BillOfLadingDto>> GetItemsAsync(FilterParameters filter);


    }
    public class BillofLadingProvider : IBillofLadingProvider
    {


        private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

        private AppObjectResponse appObjResponse = new();

        public BillofLadingProvider(IDbContextFactory<ApplicationDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddRecords(IEnumerable<BillOfLadingDto> records, string vesselCallDetailId, string userName)
        {

            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var bolList = records.Select(s => s.Num);
                var existRecords = await db.Set<BillofLadingEntity>().Include(s => s.ContainerRecords)
                                                    .Where(s => bolList.Any(b => b == s.Num))
                                                    .Select(s => s.Num)
                                                    .ToArrayAsync();
                //var existBlRecords = await db.Set<BillofLadingEntity>().Include(s => s.ContainerRecords)
                //    .Where(s => bolList.Any(b => b == s.Num))

                //.ToArrayAsync();

                //foreach (var existRecord in existBlRecords)
                //{

                //    foreach (var ContainerRecord in existRecord.ContainerRecords)
                //    {
                //        db.Entry(ContainerRecord).State = EntityState.Deleted;
                //    }
                //    db.Entry(existRecord).State = EntityState.Deleted;
                //    await db.SaveChangesAsync();
                //}



                var vesselCallDetail = await db.Set<ImportVesselCallDetail>().AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id.ToString() == vesselCallDetailId);

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
                            ShipperName = billofLading.ShipperName.RemoveExtraSymbols(),
                            ShipperAddress = billofLading.ShipperAddress.RemoveExtraSymbols(),
                            ConsigneeName = billofLading.ConsigneeName.RemoveExtraSymbols(),
                            ConsigneeAddress = billofLading.ConsigneeAddress.RemoveExtraSymbols(),
                            ConsigneeTaxNo = billofLading.ConsigneeTaxNo,
                            NotifyName = billofLading.NotifyName.RemoveExtraSymbols(),
                            NotifyAddress = billofLading.NotifyAddress.RemoveExtraSymbols(),
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
                                PackageType = rec.PackageType,
                                CommodityCode = rec.CommodityCode,
                                GoodsDescription = rec.GoodsDescription.RemoveExtraSymbols(),

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
                            VesselCallDetail = vesselCallDetail,
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

        public async Task<IEnumerable<BillOfLadingDto>> GetItemsAsync(FilterParameters filter)
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var containersNum = filter.CntrsNum.Any() ? filter.CntrsNum : string.IsNullOrWhiteSpace(filter.CntrNum) ? new() : new() { filter.CntrNum };
                var billofLadingsNum = filter.BolsNo.Any() ? filter.BolsNo : string.IsNullOrWhiteSpace(filter.BolNo) ? new() : new() { filter.BolNo };


                var BillofLadings = await db.Set<BillofLadingEntity>().Include(s => s.ContainerRecords)
                    .Where(s => !billofLadingsNum.Any() || billofLadingsNum.Any(c => c == s.Num))
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
                        }).ToList(),
                        Status = billofLading.Status,
                    }).ToArrayAsync();
                return BillofLadings;
            }
        }

        public async Task UpdateRecordsFromXls(IEnumerable<BillOfLadingDto> billOfladings, string userName)
        {

            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;


                foreach (var billOfladingDto in billOfladings)
                {
                    var billOflading = await db.Set<BillofLadingEntity>().Include(s => s.ContainerRecords)
                        .AsNoTracking().FirstOrDefaultAsync(s => s.Num == billOfladingDto.Num);
                    if (billOflading == null)
                        continue;


                    billOflading.ShipperNameRu = billOfladingDto.ShipperNameRu.RemoveExtraSymbols();
                    billOflading.ShipperCountryRu = billOfladingDto.POL?.RemoveExtraSymbols();
                    billOflading.ShipperAddressRu = billOfladingDto.ShipperAddressRu?.RemoveExtraSymbols();
                    billOflading.ConsigneeNameRu = billOfladingDto.ConsigneeNameRu?.RemoveExtraSymbols();
                    billOflading.ConsigneeAddressRu = billOfladingDto.ConsigneeAddressRu?.RemoveExtraSymbols();
                    billOflading.ConsigneeCountryRu = "РОССИЯ";

                    foreach (var containerRecord in billOflading.ContainerRecords)
                    {
                        containerRecord.CommodityCode = billOfladingDto.ContainerRecords
                            .FirstOrDefault(s => s.ContainerNum == containerRecord.ContainerNum)?.CommodityCode?.RemoveExtraSymbols();

                        containerRecord.GoodsDescriptionRu = billOfladingDto.ContainerRecords
                            .FirstOrDefault(s => s.ContainerNum == containerRecord.ContainerNum)?.GoodsDescriptionRu?.RemoveExtraSymbols(); 
                        containerRecord.GoodsDescription = billOfladingDto.ContainerRecords
                            .FirstOrDefault(s => s.ContainerNum == containerRecord.ContainerNum)?.GoodsDescription?.RemoveExtraSymbols();

                        db.Entry(containerRecord).State = EntityState.Modified;
                    }

                    db.Entry(billOflading).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                }
            }
        }
    }
}
