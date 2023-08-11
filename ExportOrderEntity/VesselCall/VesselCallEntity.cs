

using System.ComponentModel.DataAnnotations;
using ExportOrderEntites.ExportOrder;

namespace ExportOrderEntites.VesselCall;

public class VesselCallEntity : Entity
{

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.

    [Required] public VesselEntity Vessel { get; set; }
    [Required] public string VoyageCarrier { get; set; }
    [Required] public string VoyageTerminal { get; set; }


    [Required] public TerminalCatalog LoadingTerminal { get; set; }
    [Required] public LocationCatalog POD { get; set; }

    [Required] public DateTime? ETA { get; set; }
    [Required] public DateTime? ETS { get; set; }


}
