namespace ExportOrderWebServer.Areas.ExportOrder.Dto;

public class ExportOrderPageFilter : PageFilter
{
    public string? Num { get; set; }
    public List<string> Nums { get; set; } = new();
    public string? ContainerNum { get; set; }
    public List<string> ContainerNums { get; set; } = new();
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public EntityStatus? Status { get; set; }
    public string? VesselName { get; set; }
    public string? VesselVoyage { get; set; }
    public string? PortOfDischarge { get; set; }
    public string? Carrier { get; set; }
    public string? TerminalName { get; set; }
    public bool IsIMO { get; set; } = false;
    public FileType FileType { get; set; } = 0;
}