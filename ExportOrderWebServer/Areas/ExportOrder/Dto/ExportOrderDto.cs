namespace ExportOrderWebServer.Areas.ExportOrder.Dto
{
    public class ExportOrderDto
    {
        public long Id { get; set; }
        public string Num { get; set; } = string.Empty;
        public BLTemplate BLtemplate { get; set; }
        public DateTime? Dated { get; set; }            // дата поручения
        public string? TerminalName { get; set; }
        public long VesselCallId { get; set; }          // Vessel Call (рейс)
        public string? VesselName { get; set; }
        public string? VesselVoyage { get; set; }
        public string? PortOfDischarge { get; set; }
        public long CarrierId { get; set; }
        public string? CarrierNameEn { get; set; }
        public bool IsIMO { get; set; }
        public bool IsEmpty { get; set; }
        public EntityStatus Status { get; set; }
        public List<ExportOrderRecordDto> Records { get; set; } = new();

        public DateTime CreateTime { get; set; } = DateTime.Now;
        public ApplicationUser CreateUser { get; set; } = new();
    }

    public class ExportOrderRecordDto
    {
        public long Id { get; set; }
        public uint Seq { get; set; }             // Record index for RDLC Report
        public uint SeqContent { get; set; }      // Content index for xml File
        public string? Cntr { get; set; }
        public string? CntrType { get; set; }
        public string? CntrTypeISO { get; set; }
        public double? CntrTareWt { get; set; }
        public string? Seal { get; set; }
        public string? RecordCommoditiesEn { get; set; }
        public List<ContainerContentDto> ContainerContents { get; set; } = new();
    }

    public class ContainerContentDto
    {
        public string? Commodity { get; set; }
        public string? CommodityEn { get; set; }
        public uint PackageQty { get; set; }
        public string? PackageName { get; set; }
        public double? NetWt { get; set; }
        public double? GrossWt { get; set; }
        public double? GrossAndTare { get; set; }
        public double? Volume { get; set; }
        public double? SupplementaryUnitQuantity { get; set; }
        public string? SupplementaryUnitCode { get; set; }
        public string? SupplementaryUnitShortName { get; set; }

        public string? DocumentName { get; set; }
        public string? DocumentType { get; set; }
        public string? Shipper { get; set; }
        public string? ShipperEn { get; set; }
        public string? Consignee { get; set; }
        public string? ConsigneeEn { get; set; }
        public string? Notify { get; set; }
        public string? NotifyEn { get; set; }
        public string? HSCode { get; set; }
        public string? IMO { get; set; }
        public string? UNNO { get; set; }
        public bool IsIMO { get; set; }
    }
}