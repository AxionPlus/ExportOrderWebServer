using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class ExportOrderComponentDTO
{
    [Key]
    public long Id { get; set; }

#pragma warning disable CS8618
    public string? Num { get; set; }
    public string? Cntr { get; set; }
    public string? Dated { get; set; }
    public string? Vessel { get; set; }
    public string? Voyage { get; set; }
    public string? POD { get; set; }    
    public string? Carrier { get; set; }
    public bool? IsImo { get; set; }
    public bool? IsEmpty { get; set; }

    public EntityStatus Status { get; set; }
    public BLTemplate BlTemplate { get; set; }
    public DateTime? CreateTime { get; set; }
    public int? VersionNo { get; set; }
}