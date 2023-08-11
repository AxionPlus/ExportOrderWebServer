using ExportOrderEntites.ExportOrder;

namespace ExportOrderEntites.Catalog
{
    public class TerminalCatalog : CatalogEntity
    {
        public string Name { get; set; }
        public LocationCatalog Location { get; set; }
    }
}