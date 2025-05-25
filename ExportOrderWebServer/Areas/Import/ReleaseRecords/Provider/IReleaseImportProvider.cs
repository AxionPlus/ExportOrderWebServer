using ExportOrderEntites.BillofLading;
using ExportOrderEntites.ReleaseRecord;
using ExportOrderWebServer.Areas.Import.BillofLadings;
using ExportOrderWebServer.Areas.Import.ReleaseRecords.Dto;
using ExportOrderWebServer.DataSet;

namespace ExportOrderWebServer.Areas.Import.ReleaseRecords.Provider
{
    public interface IReleaseImportProvider
    {
        public Task<IEnumerable<ReleaseRecordDto>> GetRecords(string billofLading);
        public Task<IEnumerable<ReleaseRecordDto>> GetRecords(FilterParameters filter);
        public Task CreateRecords(IEnumerable<ReleaseRecordDto> releaseRecords, string userName);
        public Task ContinueRecords(IEnumerable<ReleaseRecordDto> continueList, string userName);
        public Task CancelRecords(List<ReleaseRecordDto> records, string? applicationUser);
    }
    public class ReleaseImportProvider : IReleaseImportProvider
    {


        private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

        private AppObjectResponse appObjResponse = new();

        public ReleaseImportProvider(IDbContextFactory<ApplicationDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task CancelRecords(List<ReleaseRecordDto> records, string? applicationUser)
        {
            throw new NotImplementedException();
        }

        public async Task ContinueRecords(IEnumerable<ReleaseRecordDto> continueList, string userName)
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;
                var User = await db.Set<ApplicationUser>().AsNoTracking()
                    .FirstOrDefaultAsync(s => s.UserName == userName);
                if (User == null)
                {
                    return;
                }

                var Ids = continueList.Select(s => s.Id);
                var _Records = await db.Set<ReleaseImportContainerRecordEntity>()
                    .Where(rls => Ids.Any(id => id == rls.Id))
                    .AsNoTracking()
                    .ToArrayAsync();

                var Records = _Records.Select(rls => new ReleaseImportContainerRecordEntity()
                {
                    BillOfLadingNum = rls.BillOfLadingNum,
                    ContainerType = rls.ContainerType,
                    ContainerNum = rls.ContainerNum,
                    ReleaseMode = ReleaseMode.Update,
                    DocNumber = rls.DocNumber,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UserName = userName,
                    ReleaseStatus = ReleaseStatus.NeedSendTerminal,
                    ReleaseTo = continueList.First(s => s.Id == rls.Id).ReleaseTo!.Value,
                    ReleaseUID = rls.ReleaseUID,
                    Timestamp = DateTime.Now.Ticks,
                });
                foreach (var record in Records)
                    db.Entry(record).State = EntityState.Added;

                await db.SaveChangesAsync();
            }
        }

        public async Task CreateRecords(IEnumerable<ReleaseRecordDto> releaseRecords, string userName)
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;
                var User = await db.Set<ApplicationUser>().AsNoTracking()
                                   .FirstOrDefaultAsync(s => s.UserName == userName);
                if (User == null)
                { return; }

                var Records = releaseRecords.Select(record => new ReleaseImportContainerRecordEntity()
                {
                    ReleaseUID = $"RLS{DateTime.Now.Ticks}",
                    BillOfLadingNum = record.BillofLadingNum,
                    ContainerNum = record.ContainerNum,
                    ContainerType = record.ContainerType,
                    ReleaseMode = ReleaseMode.Create,
                    ReleaseTo = record.ReleaseTo!.Value,
                    ReleaseStatus = ReleaseStatus.NeedSendTerminal,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UserName = userName,
                    TerminalName = "NLE",
                    LineName = "SOLING_AGE",

                }).ToArray();



                foreach (var record in Records)
                    db.Entry(record).State = EntityState.Added;

                await db.SaveChangesAsync();

            }
        }

        public async Task<IEnumerable<ReleaseRecordDto>> GetRecords(string billofLading)
        {
            var records = new List<ReleaseRecordDto>();
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var containers = await db.Set<BillofLadingEntity>().Include(s => s.ContainerRecords)
                    .Where(s => s.Num == billofLading)
                    .SelectMany(s => s.ContainerRecords, (bill, rec) => new ReleaseRecordDto()
                    {
                        BillofLadingNum = bill.Num,
                        ContainerNum = rec.ContainerNum,
                        ContainerType = rec.ContainerType,
                    })
                    .AsNoTracking()
                    .ToArrayAsync();

                var containersNum = containers.Select(s => s.ContainerNum);
                var _list = await db.Set<ReleaseImportContainerRecordEntity>()
                    .Where(rls => rls.BillOfLadingNum == billofLading)
                    .Where(rls => containersNum.Any(s => rls.ContainerNum == s))
                    .GroupBy(rls => rls.ContainerNum)
                    .ToArrayAsync();
                var list = _list.Select(rls => new ReleaseImportContainerRecordEntity()
                {
                    Id = rls.MaxBy(s => s.CreatedAt)!.Id,
                    BillOfLadingNum = rls.MaxBy(s => s.CreatedAt)!.BillOfLadingNum,
                    ContainerNum = rls.MaxBy(s => s.CreatedAt)!.ContainerNum,
                    ContainerType = rls.MaxBy(s => s.CreatedAt)!.ContainerType,
                    ReleaseTo = rls.MaxBy(s => s.CreatedAt)!.ReleaseTo,
                    ReleaseMode = rls.MaxBy(s => s.CreatedAt)!.ReleaseMode,
                    ReleaseStatus = rls.MaxBy(s => s.CreatedAt)!.ReleaseStatus,
                    ReleaseUID = rls.MaxBy(s => s.CreatedAt)!.ReleaseUID,
                    UserName = rls.MaxBy(s => s.CreatedAt)!.UserName,
                    CreatedAt = rls.MaxBy(s => s.CreatedAt)!.CreatedAt,

                });

                records = containers.SelectMany(s => list
                        .Where(r => s.ContainerNum == r.ContainerNum)
                        .Where(r => s.BillofLadingNum == r.BillOfLadingNum)
                        .DefaultIfEmpty(),
                   (rec, rls) => new ReleaseRecordDto()
                   {
                       Id = rec.Id,
                       BillofLadingNum = rec.BillofLadingNum,
                       ContainerNum = rec.ContainerNum,
                       ContainerType = rec.ContainerType,
                       ReleaseTo = rls?.ReleaseTo,
                       ReleaseMode = rls?.ReleaseMode,
                       ReleaseStatus = rls?.ReleaseStatus,
                       ReleaseUID = rls?.ReleaseUID,


                   }).ToList();



            }

            return records;
        }

        public async Task<IEnumerable<ReleaseRecordDto>> GetRecords(FilterParameters filter)
        {
            var records = new List<ReleaseRecordDto>();
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var containersNum = filter.CntrsNum.Any() ? filter.CntrsNum : string.IsNullOrWhiteSpace(filter.CntrNum) ? new() : new() { filter.CntrNum };
                var billofLadingsNum = filter.BolsNo.Any() ? filter.BolsNo : string.IsNullOrWhiteSpace(filter.BolNo) ? new() : new() { filter.BolNo };


                var containers = await db.Set<BillofLadingEntity>()
                    .Include(s => s.ContainerRecords)
                    .Where(s => !billofLadingsNum.Any() || billofLadingsNum.Any(c => c == s.Num))
                    .Where(s => !containersNum.Any() || s.ContainerRecords.Any(cntr => containersNum.Any(c => c == cntr.ContainerNum)))
                    .SelectMany(s => s.ContainerRecords, (bill, rec) => new ReleaseRecordDto()
                    {
                        BillofLadingNum = bill.Num,
                        ContainerNum = rec.ContainerNum,
                        ContainerType = rec.ContainerType,
                    })
                    .AsNoTracking()
                    .ToArrayAsync();

                var _list = await db.Set<ReleaseImportContainerRecordEntity>()
                    .Where(rls => !billofLadingsNum.Any() || billofLadingsNum.Any(s => rls.BillOfLadingNum == s))
                    .Where(rls => !containersNum.Any() || containersNum.Any(s => rls.ContainerNum == s))
                    .Where(rls => new[] { 0, 1, 10, 3, 4, 5 }.Any(s => s == (int)rls.ReleaseStatus))
                    .GroupBy(rls => rls.ContainerNum)
                    .ToArrayAsync();
                var list = _list.Select(rls => new ReleaseImportContainerRecordEntity()
                {
                    Id = rls.MaxBy(s => s.CreatedAt)!.Id,
                    DocNumber = rls.MaxBy(s => s.CreatedAt)!.DocNumber,
                    BillOfLadingNum = rls.MaxBy(s => s.CreatedAt)!.BillOfLadingNum,
                    ContainerNum = rls.MaxBy(s => s.CreatedAt)!.ContainerNum,
                    ContainerType = rls.MaxBy(s => s.CreatedAt)!.ContainerType,
                    ReleaseTo = rls.MaxBy(s => s.CreatedAt)!.ReleaseTo,
                    ReleaseMode = rls.MaxBy(s => s.CreatedAt)!.ReleaseMode,
                    ReleaseStatus = rls.MaxBy(s => s.CreatedAt)!.ReleaseStatus,
                    ReleaseUID = rls.MaxBy(s => s.CreatedAt)!.ReleaseUID,
                    UserName = rls.MaxBy(s => s.CreatedAt)!.UserName,
                    CreatedAt = rls.MaxBy(s => s.CreatedAt)!.CreatedAt,

                });
                var documents = list.Select(s => s.DocNumber).Distinct();
                var docRemarks = await db.Set<ReleaseRemark>()
                    .Where(doc => documents.Any(s => s == doc.DocNumber))
                    .AsNoTracking().ToArrayAsync();


                records = containers.SelectMany(s => list
                        .Where(r => s.ContainerNum == r.ContainerNum)
                        .Where(r => s.BillofLadingNum == r.BillOfLadingNum)
                        .DefaultIfEmpty(),
                   (rec, rls) => new ReleaseRecordDto()
                   {
                       Id = rec.Id,
                       DocNumber = rls?.DocNumber,
                       BillofLadingNum = rec.BillofLadingNum,
                       ContainerNum = rec.ContainerNum,
                       ContainerType = rec.ContainerType,
                       ReleaseTo = rls?.ReleaseTo,
                       ReleaseMode = rls?.ReleaseMode,
                       ReleaseStatus = rls?.ReleaseStatus,
                       ReleaseUID = rls?.ReleaseUID,
                       Remarks = docRemarks.Where(s => string.IsNullOrWhiteSpace(rls?.DocNumber) ? false : s.DocNumber == rls.DocNumber).Select(doc => doc.Remark),

                   })
                    .Where(s => !billofLadingsNum.Any() || billofLadingsNum.Any(c => c == s.BillofLadingNum))
                    .Where(s => !containersNum.Any() || containersNum.Any(c => c == s.ContainerNum))
                    .ToList();



            }

            return records;
        }
    }
}
