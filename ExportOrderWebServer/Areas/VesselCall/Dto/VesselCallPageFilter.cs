namespace ExportOrderWebServer.Areas.VesselCall.Dto;

public class VesselCallPageFilter
{
    public long VesselCallId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public EntityStatus? Status { get; set; }
    public string? VesselName { get; set; }
    public string? VesselVoyage { get; set; }
    public string? PortOfDischarge { get; set; }
    public string? TerminalName { get; set; }
    public FileType FileType { get; set; } = 0;
}
