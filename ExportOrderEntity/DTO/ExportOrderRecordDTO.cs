using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExportOrderEntites.DTO;
public class ExportOrderRecordDTO
{
    [Key]
    public uint Id { get; set; }

#pragma warning disable CS8618

    public string BLnum { get; set; }         // поле связи с ExportOrder
    public uint Seq { get; set; }             // Record index for RDLC Report
    public uint SeqContent { get; set; }      // Content index for xml File
    public string Cntr { get; set; }
    public string CntrType { get; set; }
    public string CntrTypeISO { get; set; }
    public double? CntrTareWt { get; set; }
    public string? Seal { get; set; }
    public string? RecordCommoditiesEn { get; set; }

    // Content
    public string Commodity { get; set; }
    public string CommodityEn { get; set; }
    public uint? PackageQty { get; set; }
    public string? PackageName { get; set; }
    public double? NetWt { get; set; }
    public double? GrossWt { get; set; }
    public double? GrossAndTare { get; set; }    
    public double? Volume { get; set; }
    public double? SupplementaryUnitQuantity { get; set; }
    public string? SupplementaryUnitCode { get; set; }
    public string? SupplementaryUnitShortName { get; set; }

    public string DocumentName { get; set; }
    public string DocumentType { get; set; }
    public string Shipper { get; set; }
    public string ShipperEn { get; set; }
    public string Consignee { get; set; }
    public string ConsigneeEn { get; set; }
    public string Notify { get; set; }
    public string NotifyEn { get; set; }
    public string HSCode { get; set; }
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public bool IsIMO { get; set; }


    [JsonIgnore]
    public ExportOrderDTO ExportOrderDTO { get; set; }
}
