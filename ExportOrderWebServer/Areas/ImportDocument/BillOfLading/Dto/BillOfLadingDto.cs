using System.ComponentModel.DataAnnotations;
using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;

public class BillOfLadingDto : BaseEntity
{
    [Required]
    public string Num { get; set; } = string.Empty;
    public DateTime? Date { get; set; }

    public DateTime? TsDate { get; set; }
    public string? TsPort { get; set; } = string.Empty;

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

    public Guid VesselCallId { get; set; }
    public VesselCallDto VesselCall { get; set; }

    public List<BillOfLadingContainerRecordDto> ContainerRecords { get; set; } = new();

    [NotMapped]
    public string BillOfLadingDisplay => $"{Num} - {Date:dd.MM.yyyy}";

    [NotMapped]
    public string FullInfo => $"{Num} ({Date:dd.MM.yyyy}) | Shipper: {ShipperName} | Consignee: {ConsigneeName}";

    [NotMapped]
    public int TotalContainers => ContainerRecords?.Count ?? 0;
}

public class BillOfLadingContainerRecordDto : BaseEntity
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

    public Guid BillOfLadingId { get; set; }
}