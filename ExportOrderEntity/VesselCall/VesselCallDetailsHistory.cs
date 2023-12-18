
namespace ExportOrderEntites.VesselCall;

//public class VesselCallHistory : EntityHistory
//{
//    public string? VesselName { get; set; }
//    public string? VoyageNo { get; set; }
//    public string? VoyageNoTerminal { get; set; }
//    public string? TerminalName { get; set; }
//    public DateTime? ETA { get; set; }
//    public DateTime? ETS { get; set; }
//    public List<VesselCallHistoryDetail> Details { get; set; } = new List<VesselCallHistoryDetail>();
//}

public class VesselCallDetailsHistory : EntityHistory
{
    public EntityStatus Status { get; set; }
    public string? VesselName { get; set; }
    public string? VoyageNo { get; set; }
    public string? VoyageNoTerminal { get; set; }
    public string? TerminalName { get; set; }
    public DateTime? ETA { get; set; }
    public DateTime? ETS { get; set; }

    public string? POD { get; set; }
    public string? FinalDestination { get; set; }
    public string? AgentPOD { get; set; }

    public List<ExportOrderHistory> ExportOrders { get; set; } = new List<ExportOrderHistory>();
}
