using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportOrderEntites.ImportDocument;

public class BillOfLadingBaseEntity : BaseEntity
{
    [Required]
    public string Num { get; set; } = string.Empty;
    public DateTime Date { get; set; } //BLDate

    public DateTime? TsDate { get; set; } //BLDate
    public string? TsPort { get; set; } //BLDate

    public string CustomerCode { get; set; } = string.Empty;
    public string BookingParty { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Pol { get; set; } = string.Empty;
    public string Pod { get; set; } = string.Empty;
    public string FinalPod { get; set; } = string.Empty;
    public string Shipper { get; set; } = string.Empty;
    public string ShipperCode { get; set; } = string.Empty;
    public string ShipperName { get; set; } = string.Empty;
    public string ShipperAddress { get; set; } = string.Empty;

    public string? ShipperNameRu { get; set; }

    public string Consignee { get; set; } = string.Empty;
    public string ConsigneeCode { get; set; } = string.Empty;
    public string ConsigneeName { get; set; } = string.Empty;
    public string ConsigneeAddress { get; set; } = string.Empty;

    public string? ConsigneeNameRu { get; set; }
    public string? ConsigneeAddressRu { get; set; }

    public string PartBl { get; set; } = string.Empty;
    public string CargoDescription { get; set; } = string.Empty;

    public List<BillOfLadingContainerRecordBaseEntity> ContainerRecords { get; set; } = new();

    [ForeignKey("VesselCall")]
    public Guid VesselCallId { get; set; }
    public VesselCallBaseEntity VesselCall {get; set; }

}

public class BillOfLadingContainerRecordBaseEntity : BaseEntity
{

    public string ContainerNo { get; set; } = string.Empty;
    public string ContainerTypeId { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;
    public string TareWt { get; set; } = string.Empty;
    public string FullOrEmpty { get; set; } = string.Empty;
    public bool IsSoc { get; set; } 
    public string SealNo { get; set; } = string.Empty;
    public string PackageType { get; set; } = string.Empty;
    public int NoOfPackage { get; set; }
    public double GrossWeight { get; set; }
    public string GrossWeightUOM { get; set; } = string.Empty;
    public int Volume { get; set; }
    public bool OutOfGauge { get; set; }
    public string? IMCOClass { get; set; } 
    public string? IMCONumber { get; set; } 
    public string? ReeferTempSign { get; set; }
    public string? ReeferTemp { get; set; }
    public string? ReeferTempUOM { get; set; }
    public string? ReeferHumidity { get; set; } 
    public string? ReeferVentilation { get; set; } 
    public string BookingNo { get; set; } = string.Empty;

    public bool ContainerAsCargo { get; set; }



}
