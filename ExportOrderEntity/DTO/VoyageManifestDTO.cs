namespace ExportOrderEntites.DTO;

public class VoyageManifestDTO
{
    // Voyage
    public long VesselCallId { get; set; }
    public string? VesselName { get; set; }
    public string? VesselFlag { get; set; }
    public string? VesselFlagEn { get; set; }
    public string? CaptainFamily { get; set; }
    public string? CaptainName { get; set; }
    public string? Voyage { get; set; }

    public string? PersonSign { get; set; }
    public string? PersonFamily { get; set; }
    public string? PersonSurName { get; set; }
    public string? PersonBirthYear { get; set; }
    public string? PersonBirthPlace { get; set; }
    public string? PersonCompany { get; set; }
    public string? PersonAddress { get; set; }
    public string? PersonPass { get; set; }
    public string? DateExplanation { get; set; }

    // BILL of LADING
    public string? ExpOrderNum { get; set; }
    public string? BLNum { get; set; }
    public string? BLDate { get; set; }          // дата коносамента (отхода судна)
    public string? CarrierNameEn { get; set; }             // Перевозчик - линия
    public string? CarrierCountryEn { get; set; }          // Перевозчик страна
    public string? CarrierLocation { get; set; }           // Перевозчик город
    public string? CarrierContract { get; set; }           // Перевозчик договор
    public string? CarrierContractDate { get; set; }       // Перевозчик дата договора
    public string? POLEn { get; set; } = "NOVOROSSIYSK";
    public string? PODEn { get; set; }
    public string? PODandCountryEn { get; set; }
    public string? PODunlocode { get; set; }
    public string? CustomsOfficeCode { get; set; }    // Код таможенного поста в порту погрузки
    public string? CustomsOfficeName { get; set; }
    public string? CustomsOfficeShortName { get; set; }
    public string? CustomsDapartment { get; set; }
    public string? Commodities { get; set; }
    public string? CommoditiesEn { get; set; }
    public string? Shippers { get; set; }
    public string? Consignees { get; set; }

    // RECORD
    public uint Seq { get; set; }               // Record index for RDLC Report
    public bool IsIMO { get; set; } = false;
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public string? Cntr { get; set; }
    public string? CntrType { get; set; }
    public double CntrTareWt { get; set; }
    public string? Seal { get; set; }
    public uint? PackageQtys { get; set; }
    public string? PackageNames { get; set; }
    public double? NetWeights { get; set; }
    public double? GrossWeights { get; set; }
    public double CntrTotalWeight { get; set; }
    public double? Volumes { get; set; }
    public string? RecordCommodities { get; set; }
    public string? RecordShippers { get; set; }
    public string? RecordShippersCountries { get; set; }
    public string? RecordConsignees { get; set; }
    public string? RecordConsigneesCountries { get; set; }    

    //// CONTENT
    //public int SeqContent { get; set; }      // Content sequence
    //public bool IsIMO { get; set; } = false;
    //public string? Commodity { get; set; }
    //public string? CommodityEn { get; set; }
    //public string? Shipper { get; set; }
    //public string? ShipperCountry { get; set; }
    //public string? Consignee { get; set; }
    //public string? ConsigneeCountry { get; set; }
}
