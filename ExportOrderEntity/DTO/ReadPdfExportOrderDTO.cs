using Microsoft.EntityFrameworkCore;

namespace ExportOrderEntites.DTO;

[Keyless]
public class ReadPdfExportOrderDTO
{    
    public int Id { get; set; }
    public string? ExpOrderNum { get; set; }    // номер поручения
    public string? VesselName { get; set; }
    public string? VesselVoyage { get; set; }
    public string? Shipper { get; set; }
    public string? Consignee { get; set; }
    public string? POD { get; set; }
    public string? ShippingLine { get; set; }
    public string? Commodity { get; set; }
    public double? GrossWeight { get; set; }
    public int CntrsCount { get; set; } = 0;
}
