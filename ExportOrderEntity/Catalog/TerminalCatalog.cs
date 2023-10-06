using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExportOrderEntites.Catalog
{
    public class TerminalCatalog : CatalogEntity
    {
        public string? Name { get; set; }
        public LocationCatalog? Location { get; set; }
        [NotMapped]
        public CustomOfficeCatalog? CustomOffice { get; set; }
    }
}