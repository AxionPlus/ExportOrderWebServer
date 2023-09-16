using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class DocumentDTO
{
    [Key]
    public uint Id { get; set; }
    public uint IndexDocument { get; set; }               // Record index for RDLC Report
    public string? DocumentName { get; set; }    
    public string? Shipper { get; set; }
    public string? Consignee { get; set; }

    // Document Records
    public uint Seq { get; set; }
    public string? CommodityName { get; set; }    
    public string? CommodityHSCode { get; set; }
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public bool? IsIMO { get; set; }

    // Container Content
    public uint Quantity { get; set; }
    public double NetWt { get; set; }
    public double GrossWt { get; set; }


    public ExportOrderDTO? exportOrderDTO { get; set; }
}
