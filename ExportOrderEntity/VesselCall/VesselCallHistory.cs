namespace ExportOrderEntites.VesselCall;

public class VesselCallHistory : EntityHistory
{
    public string? VesselName { get; set; }
    public string? VoyageNo { get; set; }
    public string? VoyageNoTerminal { get; set; }
    public string? TerminalName { get; set; }
    public DateTime? ETA { get; set; }
    public DateTime? ETS { get; set; }
    public List<VesselCallDetailsHistory>? Details { get; set; }
}

public class VesselCallDetailsHistory //: EntityHistory
{
    public string? POD { get; set; }
    public string? FinalDestination { get; set; }
    public string? AgentPOD { get; set; }
}
