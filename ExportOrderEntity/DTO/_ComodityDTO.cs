using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class _ComodityDTO
{
    [Key]
    public uint Id { get; set; }
    public string? Commodity { get; set; }


    public ExportOrderEntity? ExportOrder { get; set; }
}
