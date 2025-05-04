using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExportOrderEntites.ExportOrder;

public class ContainerContent
{
    [Key]
    public long Id { get; set; }

#pragma warning disable CS8618
    public uint? PackageQty { get; set; }
    public string? PackageName { get; set; }
    public double? NetWt { get; set; }
    public double? GrossWt { get; set; }
    public double? Volume { get; set; }
    public double? SupplementaryUnitQuantity { get; set; }      // Значение доп. еденицы измерения
    public SupplementaryUnitCatalog? SupplementaryUnit { get; set; }   // Доп. еденица измерения    
    public DocumentRecord DocumentRecord { get; set; }

    [JsonIgnore]
    public ExportOrderRecord ExportOrderRecord { get; set; }
    [NotMapped]
    public string? DocumentName { get; set; }
}