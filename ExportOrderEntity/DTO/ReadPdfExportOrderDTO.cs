using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class ReadPdfExportOrderDTO
{
    [Key]
    public long Id { get; set; }
    public string? Shipper { get; set; }
    public string? Consignee { get; set; }
    public string? POD { get; set; }
    public string? ShippingLine { get; set; }
    public string? Commodity { get; set; }
    public int CntrsCount { get; set; } = 0;
}
