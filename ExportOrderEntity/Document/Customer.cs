using ExportOrderEntites.Catalog;
using ExportOrderEntites.ExportOrder;

namespace ExportOrderEntites.Document;

public class CustomerCatalog : CatalogEntity
{
#pragma warning disable CS8618
    public string Name { get; set; }
    public string EngName { get; set; }

    public CountryCatalog Country { get; set; }
}
