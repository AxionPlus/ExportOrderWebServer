namespace ExportOrderEntites.VesselCall;

public class VesselCallDetail : Entity
{
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.

    public VesselCallEntity VesselCall { get; set; }        // поле связи
    public LocationCatalog? POD { get; set; }               // Порт выгрузки    
    public LocationCatalog? FinalDestination { get; set; }  // Порт выгрузки
    public string? AgentPOD { get; set; }                   // Агент в порту выгрузки
    public List<ExportOrderEntity> ExportOrders { get; set; } = new List<ExportOrderEntity>();  //public IList<ExportOrderEntity>
}
