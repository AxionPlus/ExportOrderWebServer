namespace ExportOrderWebServer.Areas.VesselCall.Dto;

public class VesselCallDto
{
    public long Id { get; set; }
    public DateTime? ETA { get; set; }
    public DateTime? ETS { get; set; }
    public EntityStatus? Status { get; set; }
    public string? VesselName { get; set; }    
    public string? VesselVoyage { get; set; }    
    public string? PortOfDischarge { get; set; }
    public string? TerminalName { get; set; }

    public long VesselCallDetailId { get; set; }
    public long PortOfDischargeId { get; set; }
}
