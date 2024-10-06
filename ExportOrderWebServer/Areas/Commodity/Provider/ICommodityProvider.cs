namespace ExportOrderWebServer.Areas.Commodity.Provider
{
    public interface ICommodityProvider : IEntityProvider<CommodityCatalog>
    {
        Task<AppObjectResponse> GetItemsAsync(FilterParameters filter);
        Task<IEnumerable<string>> GetHScodes();
    }
}
