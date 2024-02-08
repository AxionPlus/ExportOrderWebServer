namespace ExportOrderWebServer.Areas.Statistic;

public class StatisticProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public StatisticProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppObjectResponse> GetItemsAsync(object parameters)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            if (parameters.GetType() == typeof(FilterParameters))
            {
                var filter = (FilterParameters)parameters;

                var exportOrders = await db.ExportOrders.AsNoTracking()
                                                        .Include(e => e.Records)
                                                        .Select(x => new ExportOrderEntity()
                                                        {
                                                            Num = x.Num,
                                                            Dated = x.Dated,
                                                            Status = x.Status,
                                                            Records = x.Records,
                                                        })
                                                        .Where(x => filter.DateFrom.HasValue ? x.Dated >= filter.DateFrom : true)
                                                        .Where(x => filter.DateTo.HasValue ? x.Dated <= filter.DateTo : true)
                                                        .ToListAsync();

                if (exportOrders is null)
                {
                    appObjResponse.ErrorAdd("There is no data for this period");
                    return appObjResponse;
                }

                var Statistic = new StatisticEntity()
                {
                    DatedFrom = filter.DateFrom,
                    DatedTo = filter.DateTo,
                    EOsUnderway = (uint)exportOrders.Count(eo => eo.Status == EntityStatus.New || eo.Status == EntityStatus.Issued),
                    EOsCompleted = (uint)exportOrders.Count(eo => eo.Status == EntityStatus.Completed),
                    EOsCancelled = (uint)exportOrders.Count(eo => eo.Status == EntityStatus.Cancelled),                    
                    EOsOverall = (uint)exportOrders.Count(),
                    CntrsUnderway = (uint)exportOrders.Where(eo => eo.Status == EntityStatus.New || eo.Status == EntityStatus.Issued).SelectMany(eo => eo.Records).Count(),
                    CntrsCompleted = (uint)exportOrders.Where(eo => eo.Status == EntityStatus.Completed).SelectMany(eo => eo.Records).Count(),
                    CntrsCancelled = (uint)exportOrders.Where(eo => eo.Status == EntityStatus.Cancelled).SelectMany(eo => eo.Records).Count(),
                    CntrsOverall = (uint)exportOrders.SelectMany(eo => eo.Records).Count(),
                };

                appObjResponse.Object = Statistic;
            }

            return appObjResponse;
        }
    }
}
