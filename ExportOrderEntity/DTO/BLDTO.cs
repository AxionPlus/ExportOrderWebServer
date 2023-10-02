using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class BLDTO
{
    //[Key]
    //public long Id { get; set; }

#pragma warning disable CS8618
    public string BLNum { get; set; }
    public string VesselName { get; set; }
    public string VesselFlagEn { get; set; }
    public string Voyage { get; set; }
    public string BLDate { get; set; }                          // дата коносамента (added for BL)
    public string POLEn { get; set; } = "NOVOROSSIYSK";
    public string PODEn { get; set; }
    public int? TotalCntrCount { get; set; }
    public double? TotalGrossWeight { get; set; }
    public double? TotalTareWeight { get; set; }

    // RECORDS
    public uint Seq { get; set; }               // Record index for RDLC Report    
    public string Cntr { get; set; }
    public string CntrType { get; set; }
    public double? CntrTareWt { get; set; }
    public string Seal { get; set; }

    public uint PackageQty { get; set; }
    public string? PackageName { get; set; }
    public double? NetWt { get; set; }
    public double? GrossWt { get; set; }
    public double? Volume { get; set; }    
    public string Commodity { get; set; }
    public string IMO { get; set; }
    public string UNNO { get; set; }
    public bool IsIMO { get; set; }

    public string Shipper { get; set; }
    public string Consignee { get; set; }
}
