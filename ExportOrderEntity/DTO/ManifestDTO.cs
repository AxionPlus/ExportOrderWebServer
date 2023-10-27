
namespace ExportOrderEntites.DTO;

public class ManifestDTO
{

#pragma warning disable CS8618
    public string BLNum { get; set; }
    public string VesselName { get; set; }
    public string VesselFlagEn { get; set; }
    public string Voyage { get; set; }
    public string BLDate { get; set; }                          // дата коносамента (added for BL)
    public string POLEn { get; set; } = "NOVOROSSIYSK";
    public string PODEn { get; set; }

    // RECORDS
    public uint Seq { get; set; }               // Record index for RDLC Report    
    public string Cntr { get; set; }
    public string CntrType { get; set; }
    public double? CntrTareWt { get; set; }
    public string Seal { get; set; }
    public uint PackageQtys { get; set; }
    public string? PackageNames { get; set; }
    public double? NetWts { get; set; }
    public double? GrossWts { get; set; }
    public double? Volumes { get; set; }
    public string Commodities { get; set; }
    public string Shippers { get; set; }
    public string Consignees { get; set; }
}
