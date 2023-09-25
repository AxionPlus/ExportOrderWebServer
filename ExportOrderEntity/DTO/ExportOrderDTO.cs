using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class ExportOrderDTO
{
    [Key]
    public long Id { get; set; }

#pragma warning disable CS8618
    public string Num { get; set; }
    public string Dated { get; set; }
    public string? CarrierNameEn { get; set; }
    public string VesselName { get; set; }
    public string VesselNameEn { get; set; }
    public string Voyage { get; set; }
    public string DateOfLoading { get; set; }                   // дата поручения
    public string BLDate { get; set; }                          // дата коносамента (added for BL)
    public string POL { get; set; } = "Новороссийск, Россия";
    public string POLEn { get; set; } = "NOVOROSSIYSK, RUSSIA";
    public string POD { get; set; }
    public string PODEn { get; set; }
    public string? PODAgent { get; set; }
    public int TotalCntrsCount { get; set; }
    public double TotalCntrWeight { get; set; }
    public double TotalTareWeight { get; set; }
    //public string TotalMeasures { get; set; }
    public string? Contract { get; set; }
    public string ContractDate { get; set; }
    public string MyCompanyName { get; set; }
    public string Person { get; set; }

    public IEnumerable<ExportOrderRecordDTO> exportOrderRecordsDTO { get; set; } = new List<ExportOrderRecordDTO>();
}
