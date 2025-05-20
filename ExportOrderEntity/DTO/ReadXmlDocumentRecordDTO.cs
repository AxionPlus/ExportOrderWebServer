namespace ExportOrderEntites.DTO;

//public class ReadXmlDocumentDTO
//{
//    public string? Name { get; set; }
//    public string? DocumentType { get; set; }
//    public string? ShipperName { get; set; }
//    //public string? ShipperCountry { get; set; }
//    public string? ConsigneeName { get; set; }
//    //public string? ConsigneeCountry { get; set; }
//    public List<ReadXmlDocumentRecordsDTO> Records { get; set; } = new();
//}

public class ReadXmlDocumentRecordDTO
{
    public string? DocumentName { get; set; }
    public int Seq { get; set; } = 0;
    public string? CommodityHSCode { get; set; }
    public string? CommodityName { get; set; }
    public string? NetWt { get; set; }
    public string? GrossWt { get; set; }
}
