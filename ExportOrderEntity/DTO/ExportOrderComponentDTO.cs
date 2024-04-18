using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class ExportOrderComponentDTO
{
    [Key]
    public long Id { get; set; }

    public string? Num { get; set; }
    public string? Cntr { get; set; }
    public string? Dated { get; set; }
    public long VesselCallId { get; set; }
    public string? Vessel { get; set; }
    public string? VoyageNo { get; set; } // Voyage
    public string? POD { get; set; }    
    public string? Carrier { get; set; }
    public bool? IsImo { get; set; }
    public bool? IsEmpty { get; set; }
    public bool IsSelected { get; set; }

    public EntityStatus Status { get; set; }
    public BLTemplate BlTemplate { get; set; }
    public DateTime? CreateTime { get; set; }
    public int? VersionNo { get; set; }
}