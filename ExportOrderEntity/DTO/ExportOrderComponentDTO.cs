using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class ExportOrderComponentDTO
{
    [Key]
    public long Id { get; set; }                // ?? delete

#pragma warning disable CS8618
    
    public string Num { get; set; }
    //public long VesselCallId { get; set; }
    public string Dated { get; set; }
    public string Vessel { get; set; }
    public string Voyage { get; set; }
    public string POD { get; set; }    
    public string? Carrier { get; set; }
    public EntityStatus Status { get; set; }
}
