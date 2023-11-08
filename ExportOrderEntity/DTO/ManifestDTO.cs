
using System.ComponentModel.DataAnnotations.Schema;

namespace ExportOrderEntites.DTO;

public class ManifestDTO
{

#pragma warning disable CS8618
    public string BLNum { get; set; }
    public string BLDate { get; set; }          // дата коносамента (отхода судна)
    public long VesselCallId { get; set; }
    public string? VesselName { get; set; }
    public string? VesselFlag { get; set; }
    public string? VesselFlagEn { get; set; }
    public string? CaptainFamily { get; set; }
    public string? CaptainName { get; set; }
    public string Voyage { get; set; }
    public string? CarrierNameEn { get; set; }             // Перевозчик - линия
    public string? CarrierCountryEn { get; set; }          // Перевозчик страна
    public string? CarrierLocation { get; set; }           // Перевозчик город
    public string? CarrierContract { get; set; }           // Перевозчик город
    public string? CarrierContractDate { get; set; }       // Перевозчик город

    public string POLEn { get; set; } = "NOVOROSSIYSK";
    public string? PODEn { get; set; }
    public string? PODunlocode { get; set; }
    public string? CustomsOfiiceCode { get; set; }    // Код таможенного поста в порту погрузки

    // RECORDS
    public uint Seq { get; set; }               // Record index for RDLC Report    
    public string Cntr { get; set; }
    public string CntrType { get; set; }
    public double CntrTareWt { get; set; }
    public string Seal { get; set; }
    public uint? PackageQtys { get; set; }
    public string? PackageNames { get; set; }
    public double? NetWeights { get; set; }
    public double? GrossWeights { get; set; }
    public double CntrTotalWeight { get; set; }    
    public double? Volumes { get; set; }
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public string? Commodities { get; set; }
    public string? CommoditiesEn { get; set; }
    public string? Shippers { get; set; }
    public string? ShippersCountries { get; set; }
    public string? Consignees { get; set; }
    public string? ConsigneesCountries { get; set; }

    public string? CustomsOfficeCode { get; set; }
    public string? CustomsOfficeName { get; set; }
    public string? CustomsOfficeShortName { get; set; }
    public string? CustomsDapartment { get; set; } 


    public string? PersonSign { get; set; }
    public string? PersonFamily { get; set; }
    public string? PersonSurName { get; set; }
    public string? PersonBirthYear { get; set; }
    public string? PersonBirthPlace { get; set; }
    public string? PersonCompany { get; set; }
    public string? PersonAddress { get; set;}
    public string? PersonPass { get; set; }
    public string? DateExplanation { get; set; }
    
}

public class ManifestSummaryDTO
{
    // Count
    public int Full20Count { get; set; }
    public int Full40Count { get; set; }
    public int Empty20Count { get; set; }
    public int Empty40Count { get; set; }

    // Total Full
    public double Full20Weight { get; set; }
    public double Full40Weight { get; set; }

    // Total Tare
    public double Full20Tare { get; set; }
    public double Full40Tare { get; set; }
    public double Empty20Tare { get; set; }
    public double Empty40Tare { get; set; }

    // Grand Total
    public double Total20 { get; set; }
    public double Total40 { get; set; }
    public int TotalCntrs { get; set; }
    public double TotalWeight { get; set; }
    public double TotalTare { get; set; }
    public double Total { get; set; }
}
