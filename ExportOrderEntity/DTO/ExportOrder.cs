
namespace ExportOrderEntites.DTO;

public class ExportOrder
{
    public string? Num { get; set; }
    public DateTime? Dated { get; set; }
    public string? VesselName { get; set; }
    public string? Voyage { get; set; }
    public string? POL { get; set; }
    public DateTime? DateOfLoading { get; set; }
    public string? POD { get; set; }
    public string? Shipper { get; set; }
    public string? Consignee { get; set; }

}
