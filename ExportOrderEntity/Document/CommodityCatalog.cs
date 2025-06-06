using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.Document;

public class CommodityCatalog : CatalogEntity
{
#pragma warning disable CS8618
    public string Name { get; set; } = string.Empty; //public required string
    public string NameEn { get; set; } = string.Empty; //public required string
    [MaxLength(10)]
    public string HSCode { get; set; } = string.Empty; //public required string
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public bool IsIMO { get; set; } = false;
}
