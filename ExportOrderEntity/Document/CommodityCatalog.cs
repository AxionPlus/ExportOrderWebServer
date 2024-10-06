using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.Document;

public class CommodityCatalog : CatalogEntity
{
#pragma warning disable CS8618    
    public required string Name { get; set; }
    public required string NameEn { get; set; }    
    [MaxLength(10)]
    public required string HSCode { get; set; }
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public bool IsIMO { get; set; } = false;
}
