using ExportOrderEntites.BillofLading;

namespace ExportOrderEntites.ImportVesselCall;

public class ImportVesselCallDetail : Entity
{
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.

    public ImportVesselCallEntity VesselCall { get; set; }        // поле связи
    public LocationCatalog? POL { get; set; }               // Порт выгрузки    
    public LocationCatalog? FinalDestination { get; set; }  // Порт выгрузки
    public string? AgentPOL { get; set; }                   // Агент в порту выгрузки
    public List<BillofLadingEntity> BillofLadings { get; set; } = new List<BillofLadingEntity>();  //public IList<ExportOrderEntity>
}
