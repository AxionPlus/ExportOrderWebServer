using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.Carrier.Provider;

public class CarrierProvider : ICarrierProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public CarrierProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }


}
