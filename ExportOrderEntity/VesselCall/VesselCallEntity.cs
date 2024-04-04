using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.VesselCall;

public class VesselCallEntity : Entity
{

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.

    [Required] public VesselEntity Vessel { get; set; }
    [Required] public string VoyageNo { get; set; }
    [Required] public string VoyageNoTerminal { get; set; }       // Terminal's Voy number
    [Required] public TerminalCatalog Terminal { get; set; }
    [Required] public DateTime? ETA { get; set; }
    [Required] public DateTime? ETS { get; set; }
    public List<VesselCallDetail> Details { get; set; } = new List<VesselCallDetail>();
}
