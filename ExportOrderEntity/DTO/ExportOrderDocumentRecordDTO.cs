namespace ExportOrderEntites.DTO;

public class ExportOrderDocumentRecordDTO
{
    public long DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public int Seq { get; set; }
    public string? CommodityHSCode { get; set; }
    public string? CommodityName { get; set; }
}
