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
    public string? AdditionalUnitCode { get; set; }     // Код доп. еденицы измерения
    public string? AdditionalUnitName { get; set; }     // Наименование доп. еденицы измерения
    public double? AdditionalUnitQuantity { get; set; }    // Значение доп. еденицы измерения

    public DocumentRecord DocumentRecord { get; set; }
    [JsonIgnore]
    public ExportOrderRecord ExportOrderRecord { get; set; }
    [NotMapped] public string? DocumentName { get; set; }
}