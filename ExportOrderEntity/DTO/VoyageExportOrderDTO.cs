using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class VoyageExportOrderDTO
{
    [Key]
    public long Id { get; set; }

#pragma warning disable CS8618
    public long VesselCallId { get; set; }        // для группировки записей RDCL Report
    public string Num { get; set; }
    public string BLNum { get; set; }
    public string BLtemplate { get; set; }
    public string? Dated { get; set; }                   // дата поручения
    public string? BLDate { get; set; }                  // дата коносамента (format "dd.MM.yyyy")
    public string? BLDateOEL { get; set; }               // дата коносамента (format "dd/MM/yyyy")  
    public string? DateOfLoading { get; set; }           // дата погрузки в поручении
    public string? XmlDated { get; set; }                // дата для xml файла
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
    public string? Measurement { get; set; }

    public string? Commodities { get; set; }
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

    // RECORD
    public uint Seq { get; set; }             // Record index for RDLC Report
    public uint SeqContent { get; set; }      // Content index for xml File
    public string Cntr { get; set; }
    public string CntrType { get; set; }
    public double? CntrTareWt { get; set; }
    public string? Seal { get; set; }
    public string? RecordCommoditiesEn { get; set; }

    // Content
    public string? Commodity { get; set; }
    public string? CommodityEn { get; set; }
    public uint? PackageQty { get; set; }
    public string? PackageName { get; set; }
    public double? NetWt { get; set; }
    public double? GrossWt { get; set; }
    public double? GrossAndTare { get; set; }
    public double? Volume { get; set; }

    public string DocumentName { get; set; }
    public string Shipper { get; set; }
    public string ShipperEn { get; set; }
    public string Consignee { get; set; }
    public string ConsigneeEn { get; set; }
    public string Notify { get; set; }
    public string NotifyEn { get; set; }
    public string HSCode { get; set; }
    public string IMO { get; set; }
    public string UNNO { get; set; }
    public bool IsIMO { get; set; }
}