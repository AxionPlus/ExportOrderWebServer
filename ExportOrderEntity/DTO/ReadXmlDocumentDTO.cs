namespace ExportOrderEntites.DTO;

public class ReadXmlDocumentDTO
{
    public string? Name { get; set; }
    public string? DocumentType { get; set; }
    public string? ShipperName { get; set; }
    public string? ShipperCountry { get; set; }
    public string? ConsigneeName { get; set; }
    public string? ConsigneeCountry { get; set; }
    public List<ReadXmlDocumentRecordsDTO> Records { get; set; } = new();
}

public class ReadXmlDocumentRecordsDTO
{
    public int Seq { get; set; } = 0;
    public string? CommodityName { get; set; }
    public string? CommodityHSCode { get; set; }
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public bool IsIMO { get; set; } = false;
    public string? NetWt { get; set; }
    public string? GrossWt { get; set; }
    public string? Volume { get; set; }
}
