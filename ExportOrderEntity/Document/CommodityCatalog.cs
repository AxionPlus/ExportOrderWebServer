using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.Document;

public class CommodityCatalog : CatalogEntity
{
#pragma warning disable CS8618
    [Required]
    public string? Name { get; set; }
    [Required]
    public string? EngName { get; set; }
    [Required]
    [MaxLength(10)]
    public string HSCode { get; set; }
}
