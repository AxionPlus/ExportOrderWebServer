namespace ExportOrderEntites.Catalog;

public class LocationCatalog : CatalogEntity
{
    public string? Name { get; set; }    
    public string? NameEn { get; set; }
    public string? UnLocode { get; set; }
    public CountryCatalog? Country { get; set; }
}