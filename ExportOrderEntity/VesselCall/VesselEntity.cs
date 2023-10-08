using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.VesselCall;

public class VesselEntity: Entity
{
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.

    [MaxLength(10)]
    public string? IMO { get; set; }
    public string? Name { get; set; }    
    public string? TerminalId { get; set; }         // Принятый Терминалом Идентификатор судна
    public CountryCatalog? Flag { get; set; }
}