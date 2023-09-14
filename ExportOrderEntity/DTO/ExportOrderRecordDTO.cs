
namespace ExportOrderEntites.DTO;

public class ExportOrderRecordDTO
{
    public uint Id { get; set; }

#pragma warning disable CS8618
    
    public string Cntr { get; set; }
    public string CntrType { get; set; }
    public double? CntrTareWt { get; set; }
    public string Seal { get; set; }
    

    public uint Quantity { get; set; }    
    public double NetWt { get; set; }    
    public double GrossWt { get; set; }



    //#region DocumentRecord
    //public uint Seq { get; set; }
    //public string? CommodityName { get; set; }
    //public string? CommodityEngName { get; set; }
    //public string? CommodityHSCode { get; set; }
    //public string? IMO { get; set; }
    //public string? UNNO { get; set; }
    //#endregion


    public ExportOrderEntity ExportOrder { get; set; }  // ???
}
