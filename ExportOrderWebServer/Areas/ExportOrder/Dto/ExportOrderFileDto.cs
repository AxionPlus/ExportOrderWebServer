namespace ExportOrderWebServer.Areas.ExportOrder.Dto;

public class ExportOrderFileDto
{
    public long Id { get; set; }
#pragma warning disable CS8618
    public string Num { get; set; }
    public DateTime? Dated { get; set; }                // дата поручения
    public DateTime? DateExplanation { get; set; }      // дата Объяснения
    public string BLNum { get; set; }
    public BLTemplate BLtemplate { get; set; }
    public string? BLDate { get; set; }               // дата к/с
    public string? LoadingDate { get; set; }            // дата погрузки в поручении
    public string? CarrierNameEn { get; set; }
    public string? CarrierLocation { get; set; }
    public string? CarrierCountryEn { get; set; }
    public string? CarrierContract { get; set; }
    public string? CarrierContractDate { get; set; }
    //
    public string? TerminalName { get; set; }    
    public long VesselCallId { get; set; }              // Vessel Call - id
    public string? VoyageNum { get; set; }              // Vessel Call - number
    public string? VesselName { get; set; }
    public string? VesselFlag { get; set; }
    public string? VesselFlagEn { get; set; }
    public string? CaptainFamily { get; set; }
    public string? CaptainName { get; set; }    

    public string[] Shippers { get; set; }
    public string[] ShippersEn { get; set; }
    public string[] Consignees { get; set; }
    public string[] ConsigneesEn { get; set; }

    public string POL { get; set; } = "Новороссийск, Россия";
    public string POLEn { get; set; } = "NOVOROSSIYSK";
    public string POLEnCountryEn { get; set; } = "NOVOROSSIYSK, RUSSIA";
    public string? PortOfDischarge { get; set; }
    public string? PortOfDischargeEn { get; set; }        
    //public string? PODEnCountryEn { get; set; }
    public string? PortOfDischargeEnCountryRus { get; set; }
    public string? PortOfDischargeUnlocode { get; set; }
    //public string? PlaceReceipt { get; set; }
    //public string? PlaceDelivery { get; set; }
    public string? POLAgent { get; set; }
    public string? PODAgent { get; set; }
    //public string? FinalDestination { get; set; }    

    public string[] Commodities { get; set; }
    public string[] CommoditiesEn { get; set; }
    public string? CommodityShort { get; set; }
    public string? CommodityShortEn { get; set; }

    public string? Contract { get; set; }
    public string? ContractDate { get; set; }
    public string? CustomsOfficeCode { get; set; }
    public string? CustomsOfficeName { get; set; }
    public string? CustomsOfficeNameShort { get; set; }
    public string? CustomsDapartment { get; set; }
    public string? MyCompanyName { get; set; }
    public string? MyCompanyEmail { get; set; }
    public string? PersonSign { get; set; }
    public string? PersonXml { get; set; }
    public string? PersonFamily { get; set; }
    public string? PersonNameSurname { get; set; }
    public string? PersonPass { get; set; }
    public string? PersonPhone { get; set; }
    public string? PersonBirthYear { get; set; }
    public string? PersonBirthPlace { get; set; }
    public string? PersonAddress { get; set; }
    public string? PersonCompany { get; set; }

    public List<ExportOrderRecordFileDto> Records { get; set; } = new();
}

public class ExportOrderRecordFileDto
{
    public long Id { get; set; }
    public uint? Seq { get; set; }             // Record index
    public string? ContainerNum { get; set; }
    public string? CntrType { get; set; }
    public double? CntrTareWt { get; set; }
    public string? Seal { get; set; }
    public double? GrossAndTare { get; set; }
    public string? Measurement { get; set; }    // ед.измерения
    public string? Shipper { get; set; }
    public string? ShipperEn { get; set; }
    public string? ShipperCountryEn { get; set; }
    public string? Consignee { get; set; }
    public string? ConsigneeEn { get; set; }
    public string? ConsigneeCountryEn { get; set; }
    public bool IsIMO { get; set; } = false;

    // Content
    public uint SeqContent { get; set; }      // Content index for xml File
    public string? DocumentName { get; set; }
    public string? DocumentType { get; set; }    
    public uint? PackageQty { get; set; }
    public string? PackageNames { get; set; }
    public double? NetWt { get; set; }
    public double? GrossWt { get; set; }
    public double? Volume { get; set; }

    public string? Commodity { get; set; }
    public string? CommodityEn { get; set; }
    public string? HSCode { get; set; }
    public string? IMO { get; set; }
    public string? UNNO { get; set; }

    public double? SupplementaryUnitQuantity { get; set; }
    public string? SupplementaryUnitCode { get; set; }
    public string? SupplementaryUnitShortName { get; set; }
}