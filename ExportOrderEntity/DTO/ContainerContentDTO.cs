
// DELETE

using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class ContainerContentDTO
{
    [Key]
    public uint Id { get; set; }
    public int Quantity { get; set; }
    public double NetWt { get; set; }
    public double GrossWt { get; set; }

    public string? DocumentName { get; set; }              // Declaration number
    public string? CommodityName { get; set; }
    public string? HSCode { get; set; }
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public bool? IsIMO { get; set; }
}
