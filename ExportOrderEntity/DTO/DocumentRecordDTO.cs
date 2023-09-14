

namespace ExportOrderEntites.DTO;

public class DocumentRecordDTO
{
    public uint Id { get; set; }
    public string? DocumentName { get; set; }
    public uint Seq { get; set; }
    public string? CommodityName { get; set; }
    public string? CommodityEngName { get; set; }
    public string? CommodityHSCode { get; set; }
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public bool? IsIMO { get; set; }
}
