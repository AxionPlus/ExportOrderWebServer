
namespace ExportOrderEntites.DTO;

public class BLDTO
{

#pragma warning disable CS8618
    public string? BLNum { get; set; }
    public string? BLDate { get; set; }
    public string? BLTemplate { get; set; }
    public string? VesselName { get; set; }
    public string? VesselFlagEn { get; set; }
    public string? Voyage { get; set; }    
    public string? POLEn { get; set; } = "NOVOROSSIYSK, RUSSIA";
    public string? PODEn { get; set; }
    public string? POLAgent { get; set; }
    public string? PODAgent { get; set; }
    public string? FinalDestination { get; set; }
    public string? Shippers { get; set; }
    public string? Consignees { get; set; }
    public string? NotifyParties { get; set; }

    public uint? TotalCntrCount { get; set; }
    public uint? TotalPackages { get; set; }
    public double? TotalGrossWeight { get; set; }
    public double? TotalVolume { get; set; }
    public string? Measurement { get; set; }

    // RECORDS
    public string? Cntr { get; set; }
    public string? CntrType { get; set; }
    public double? CntrTareWt { get; set; }
    public string? Seal { get; set; }

    public uint? PackageQtys { get; set; }
    public string? PackageNames { get; set; }
    public string? CntrCommodities { get; set; }
    public double? GrossWts { get; set; }
    public double? Volumes { get; set; }    
    
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public bool IsIMO { get; set; }

}
