using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class DocumentDTO
{
    [Key]
    public uint Id { get; set; }
    public int IndexDocument { get; set; }               // Record index for RDLC Report
    public string? DocumentName { get; set; }
    public uint Seq { get; set; }
    public string? CommodityName { get; set; }
    public string? CommodityEngName { get; set; }
    public string? CommodityHSCode { get; set; }
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public bool? IsIMO { get; set; }


    public ExportOrderEntity? ExportOrder { get; set; }
}
