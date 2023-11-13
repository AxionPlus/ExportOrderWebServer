using System.ComponentModel.DataAnnotations.Schema;

namespace ExportOrderEntites.VesselCall;

public class VesselCallDetail : Entity
{
    public VesselCallEntity VesselCall { get; set; }        // поле связи
    public LocationCatalog? POD { get; set; }               // Порт выгрузки
    [NotMapped]
    public LocationCatalog? FinalDestination { get; set; }  // Порт выгрузки
    public string? AgentPOD { get; set; }                   // Агент в порту выгрузки
    public IList<ExportOrderEntity> ExportOrders { get; set; } = new List<ExportOrderEntity>();
}
