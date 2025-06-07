
namespace ExportOrderEntites.BillofLading.Dto
{
    public class ManifestBillOfLadingDto
    {
        public string? Num { get; set; }

        public bool IsRef { get; set; }
        public bool IsSoc { get; set; }
        public bool IsOog { get; set; }
        public bool IsImo { get; set; }
        public bool IsAlcohol { get; set; }
        public bool IsMilitaryCargo { get; set; }
        public bool HasTranslate { get; set; }

        public string? ServiceCode { get; set; }
        public DateTime? IssueDate { get; set; }

        public DateTime? SobDate { get; set; }

        public string? ShipperName { get; set; }
        public string? ShipperAddress { get; set; }
        public string? ConsigneeName { get; set; }
        public string? ConsigneeAddress { get; set; }
        public string? ConsigneeTaxNo { get; set; }
        public string? NotifyName { get; set; }
        public string? NotifyAddress { get; set; }
        public string? NotifyEmail { get; set; }
        public string? AdditionalInfo { get; set; }

        public string? ShipperNameRu { get; set; }
        public string? ShipperAddressRu { get; set; }
        public string? ShipperCountryRu { get; set; }
        public string? ConsigneeNameRu { get; set; }
        public string? ConsigneeAddressRu { get; set; }
        public string? ConsigneeCountryRu { get; set; }

        public CustomsDeliveryMode? CustomsDeliveryMode { get; set; }
        public string? POR { get; set; }

        public string? POL { get; set; }
        public string? TS_PORT { get; set; }
        public string? POD { get; set; }
        public string? F_POD { get; set; }

        public  string ContainerNum { get; set; }
        public  string ContainerType { get; set; }
        public  int TareWeight { get; set; }
        public  double CargoWeight { get; set; }
        public  string SealNo { get; set; }
        public string? SealShr { get; set; }
        public string? SealOth { get; set; }

        public string? ImoClass { get; set; }
        public string? Unno { get; set; }
        public int? TempSet { get; set; }
        public int PackageQty { get; set; }
        public string? CommodityCode { get; set; }
        public string? GoodsDescription { get; set; }
        public string? GoodsDescriptionRu { get; set; }
  
    }
}
