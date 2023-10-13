namespace ExportOrderEntites.VesselCall;

public class VesselCallDetail : Entity
{
    public VesselCallEntity VesselCall { get; set; }   // поле связи
    public LocationCatalog? POD { get; set; }           // Порт выгрузки
    public string? AgentPOD { get; set; }               // Агент в порту выгрузки
}
