namespace ExportOrderEntites.DTO;

public class VesselCallDTO
{
    public long Id { get; set; }
    public string? VesselName { get; set; }
    public string? VoyageNo { get; set; }               // own Voy number
    public string? VoyageNoTerminal { get; set; }       // Terminal's Voy number
    public string? Terminal { get; set; }               // Терминал порта погрузки
    public DateTime? ETA { get; set; }                  // Дата подхода
    public DateTime? ETS { get; set; }                  // Дата отдхода

    public string? POD { get; set; }                    // Порт выгрузки
    public string? AgentPOD { get; set; }               // Агент в порту выгрузки
    public EntityStatus Status { get; set; }
}