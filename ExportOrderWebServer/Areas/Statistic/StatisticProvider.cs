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

                var exportOrders = await db.ExportOrders
                                                        .Select(x => new ExportOrderEntity()
                                                        {
                                                            Num = x.Num,
                                                            Dated = x.Dated,
                                                            Status = x.Status,
                                                        })
                                                        .Where(x => filter.DateFrom.HasValue ? x.Dated >= filter.DateFrom : true)
                                                        .Where(x => filter.DateTo.HasValue ? x.Dated <= filter.DateTo : true)
                                                        .ToListAsync();

                if (exportOrders is null)
                {
                    appObjResponse.ErrorAdd("There is no data for this period");
                    return appObjResponse;
                }

                var ItemsStatistic = new StatisticEntity()
                {
                    DatedFrom = filter.DateFrom,
                    DatedTo = filter.DateTo,
                    CountUnderway = (uint)exportOrders.Count(eo => eo.Status == EntityStatus.New && eo.Status == EntityStatus.Issued),
                    CountCancelled = (uint)exportOrders.Count(eo => eo.Status == EntityStatus.Cancelled),
                    CountCompleted = (uint)exportOrders.Count(eo => eo.Status == EntityStatus.Completed),
                    CountOverall = (uint)exportOrders.Count(),
                };

                appObjResponse.Object = ItemsStatistic;
            }

            return appObjResponse;
        }
    }
}
