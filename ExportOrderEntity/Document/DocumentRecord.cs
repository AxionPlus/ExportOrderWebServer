using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExportOrderEntites.Document;

public class DocumentRecord
{
    [Key]
    public int Id { get; set; }
#pragma warning disable CS8618
    public int Seq { get; set; }    // change to uint

    [Required]
    public string CommodityName { get; set; }
    [Required]
    public string CommodityEngName { get; set; }

    [MaxLength(10)]
    public string? CommodityHSCode { get; set; }
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public bool IsIMO { get; set; }

    [Required]
    public double? NetWt { get; set; }
    [Required]
    public double? GrossWt { get; set; }
    public double? Volume { get; set; }
    

    [JsonIgnore]
    public DocumentEntity Document { get; set; }

    [NotMapped]
    public bool IsShowWT_Details { get; set; } = false;
}
