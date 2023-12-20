using System.Text.Json.Serialization;

namespace ExportOrderEntites.ExportOrder;

public class ExportOrderHistory : EntityHistory
{
    public string? Num { get; set; }
    public DateTime? Dated { get; set; }
    public string? Carrier { get; set; }
    public string? Person { get; set; }
    public string? VoyageNo { get; set; }
    public string? VesselName { get; set; }
    public string? POD { get; set; }

    public List<DocumentHistory>? Documents { get; set; }
    public List<ExportOrderRecordHistory>? Records { get; set; } = new List<ExportOrderRecordHistory>(); 
}

public class ExportOrderRecordHistory
{
    public long Id { get; set; }
    public string? CntrNum { get; set; }
    public string? CntrType { get; set; }
    public double CntrTareWt { get; set; }
    public string? Seal { get; set; }
    public List<ContainerContentHistory>? Contents { get; set; } = new List<ContainerContentHistory>();

    [JsonIgnore]
    public ExportOrderHistory? ExportOrder { get; set; }
}

public class ContainerContentHistory
{
    public long Id { get; set; }
    public uint? PackageQty { get; set; }
    public string? PackageName { get; set; }
    public double? NetWt { get; set; }
    public double? GrossWt { get; set; }
    public double? Volume { get; set; }
    public string? Document{ get; set; }
    public int? SeqDocument { get; set; }
    public string? CommodityName { get; set; }
    public string? CommodityEngName { get; set; }
    public bool IsIMO { get; set; }
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
}
