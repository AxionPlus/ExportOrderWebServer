using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class ExportOrderDTO
{
    [Key]
    public long Id { get; set; }

#pragma warning disable CS8618
    public string Num { get; set; }
    public string BLNum { get; set; }
    public string BLtemplate { get; set; }
    public string? Dated { get; set; }                   // дата поручения
    public string? BLDate { get; set; }                  // дата коносамента (format "dd.MM.yyyy")
    public string? BLDateOEL { get; set; }               // дата коносамента (format "dd/MM/yyyy")  
    public string? DateOfLoading { get; set; }           // дата погрузки в поручении
    public string? xmlDated { get; set; }                // дата для xml файла
    public string? CarrierNameEn { get; set; }
    public string? TerminalName { get; set; }
    public string VesselName { get; set; }
    public string VesselFlag { get; set; }
    public string VesselFlagEn { get; set; }    
    public string Voyage { get; set; }
    public string? Shippers { get; set; }
    public string? Consignees { get; set; }    
    
    public string POL { get; set; } = "Новороссийск, Россия";
    public string POLEn { get; set; } = "NOVOROSSIYSK, RUSSIA";
    public string POD { get; set; }
    public string PODEn { get; set; }
    public string? PODunlocode { get; set; }
    public string? PODwithCountryEn { get; set; }
    public string? PODwithCountryRus { get; set; }
    public string? POLAgent { get; set; }
    public string? PODAgent { get; set; }    
    public string? FinalDestination { get; set; }    
    public string? Measurement { get; set;}

    public string? Commodities { get; set; }
    public string? CommodityShort { get; set; }
    public uint TotalCntrCount { get; set; }
    public uint? TotalPackages { get; set; }
    public double? TotalGrossWeight { get; set; }
    public double? TotalNetWeight { get; set; }
    public double? TotalTareWeight { get; set; }
    public double? TotalGrossNTareWeight { get; set; }

    public string? Contract { get; set; }
    public string? ContractDate { get; set; }
    public string? CustomsOfficeCode { get; set; }
    public string? CustomsOfficeNameShort { get; set; }
    public string? MyCompanyName { get; set; }
    public string? Person { get; set; }
    public string? PersonXml { get; set; }
    public string? PersonPass { get; set; }
    public string? PersonPhone { get; set; }

    public IEnumerable<ExportOrderRecordDTO> ExportOrderRecordsDTO { get; set; } = new List<ExportOrderRecordDTO>();
}
