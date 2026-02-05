using System.ComponentModel.DataAnnotations;
using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Port.Dto;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;

public class BillOfLadingBaseDto : BaseEntity
{
    public string? FileName { get; set; } = string.Empty;
    [Required]
    public string Num { get; set; } = string.Empty;
    public DateTime? Date { get; set; }

    public DateTime? TsDate { get; set; }
    public PortDto? TsPort { get; set; } 
    public string? Carrier { get; set; } = string.Empty;

    public string CustomerCode { get; set; } = string.Empty;
    public string BookingParty { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public PortDto? Pol { get; set; }
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
    public string? ConsigneeCountryRu { get; set; } = "РОССИЯ";

    public string PartBl { get; set; } = string.Empty;
    public string CargoDescription { get; set; } = string.Empty;
    public string? CargoDescriptionRu { get; set; }

    public string CustomsMode { get; set; } = "ГТД";
    public Guid VesselCallId { get; set; }
    public VesselCallDto VesselCall { get; set; }

    public List<BillOfLadingContainerRecordBaseDto> ContainerRecords { get; set; } = new();

    public int TotalContainers => ContainerRecords?.Count ?? 0;


    public bool HasTranslate
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ConsigneeNameRu))
                return false;
            if (string.IsNullOrWhiteSpace(ConsigneeAddressRu))
                return false;
            if (string.IsNullOrWhiteSpace(ConsigneeCountryRu))
                return false;
            if (string.IsNullOrWhiteSpace(CargoDescriptionRu))
                return false;
            if (string.IsNullOrWhiteSpace(ShipperNameRu))
                return false;
            if (ContainerRecords.Any(s => string.IsNullOrWhiteSpace(s.CargoDescriptionRu)))
                return false;

            return true;
        }
    }

    public double TotalGrossWeight => ContainerRecords.Sum(s => s.GrossWeight);
    public double TotalNoOfPackage => ContainerRecords.Sum(s => s.NoOfPackage);


    public string? ShipperFullName
    {
        get
        {
            if (Pol == null || string.IsNullOrWhiteSpace(Pol.CountryRu) || string.IsNullOrWhiteSpace(ShipperNameRu))
                return null;
            return $"{Pol.CountryRu}, {ShipperNameRu}";
        }
    }

    public string? ConsigneeFullName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ConsigneeCountryRu) || string.IsNullOrWhiteSpace(ConsigneeNameRu) || string.IsNullOrWhiteSpace(ConsigneeAddressRu))
                return null;
            return $"{ConsigneeCountryRu}, {ConsigneeNameRu} {ConsigneeAddressRu}";
        }
    }

}

public class BillOfLadingContainerRecordBaseDto : BaseEntity
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
    public string? ReeferFullTemp =>
        string.IsNullOrEmpty(ReeferTempSign)
            ? ReeferTemp
            : $"{ReeferTempSign.Substring(0, 1)}{ReeferTemp}";
    public string? ReeferTempUOM { get; set; }
    public string? ReeferHumidity { get; set; }
    public string? ReeferVentilation { get; set; }
    public string BookingNo { get; set; } = string.Empty;

    public bool ContainerAsCargo { get; set; }
    public string? CargoDescriptionRu { get; set; }

}