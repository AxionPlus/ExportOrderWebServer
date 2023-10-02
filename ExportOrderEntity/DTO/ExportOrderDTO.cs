using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class ExportOrderDTO
{
    [Key]
    public long Id { get; set; }

#pragma warning disable CS8618
    public string Num { get; set; }
    public long VesselCallId { get; set; }
    public long CarrierId { get; set; }
    public string Dated { get; set; }
    public string? CarrierNameEn { get; set; }
    public string VesselName { get; set; }
    public string VesselFlag { get; set; }
    public string VesselFlagEn { get; set; }    
    public string Voyage { get; set; }
    public string DateOfLoading { get; set; }                   // дата поручения
    public string BLDate { get; set; }                          // дата коносамента (added for BL)
    public string POL { get; set; } = "Новороссийск, Россия";
    public string POLEn { get; set; } = "NOVOROSSIYSK";
    public string POD { get; set; }
    public string PODEn { get; set; }
    public string? PODAgent { get; set; }
    public int TotalCntrCount { get; set; }
    public double? TotalGrossWeight { get; set; }
    public double? TotalTareWeight { get; set; }
    public string? Contract { get; set; }
    public string? ContractDate { get; set; }
    public string MyCompanyName { get; set; }
    public string Person { get; set; }

    public IEnumerable<ExportOrderRecordDTO> exportOrderRecordsDTO { get; set; } = new List<ExportOrderRecordDTO>();
}
