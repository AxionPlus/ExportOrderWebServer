namespace ExportOrderWebServer.Areas.Commodity.Provider
{
    public interface ICommodityProvider : IEntityProvider<CommodityCatalog>
    {
        Task<IEnumerable<string>> GetHScodes();
    }
}
