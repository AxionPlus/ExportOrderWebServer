using ExportOrderEntites.BillofLading;
using ExportOrderWebServer.Areas.Import.BillofLadings.Dto;
using Microsoft.Office.Interop.Excel;

namespace ExportOrderWebServer.Areas.Import.BillofLadings.Provider
{
    public interface IBillofLadingProvider
    {
        Task AddRecords(IEnumerable<BillOfLadingDto> records, string userName);
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

                    }).ToArrayAsync();
                return BillofLadings;
            }
        }
    }
}
