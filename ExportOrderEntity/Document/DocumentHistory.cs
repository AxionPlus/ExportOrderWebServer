using System.Text.Json.Serialization;

namespace ExportOrderEntites.Document;

public class DocumentHistory : EntityHistory
{
    public string? Name { get; set; }
    public DocumentType Type { get; set; }
    public string? ShipperName { get; set; }
    public string? ConsigneeName { get; set; }
    public List<DocumentRecordHistory>? Records { get; set; }
}

public class DocumentRecordHistory
{
    public int Id { get; set; }
    public int Seq { get; set; }
    public string? CommodityName { get; set; }    
    public string? CommodityEngName { get; set; }
    public string? CommodityHSCode { get; set; }
    public bool IsIMO { get; set; }
    public string? IMO { get; set; }
    public string? UNNO { get; set; }    
    public double? NetWt { get; set; }
    public double? GrossWt { get; set; }
    public double? Volume { get; set; }
}
