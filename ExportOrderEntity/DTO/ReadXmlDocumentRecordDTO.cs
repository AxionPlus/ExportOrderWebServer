namespace ExportOrderEntites.DTO;

public class ReadXmlDocumentRecordDTO
{
    public string? DocumentName { get; set; }
    public int Seq { get; set; } = 0;
    public string? CommodityHSCode { get; set; }
    public string? CommodityName { get; set; }
    public string? NetWt { get; set; }
    public string? GrossWt { get; set; }
}
