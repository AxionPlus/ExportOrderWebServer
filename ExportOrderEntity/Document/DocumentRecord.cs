using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExportOrderEntites.Document;

public class DocumentRecord
{
    [Key]
    public int Id { get; set; }
#pragma warning disable CS8618
    public int Seq { get; set; }    // change to uint

    //[Required]
    public string CommodityName { get; set; } = string.Empty;
    //[Required]
    public string CommodityEngName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string CommodityHSCode { get; set; } = string.Empty;
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public bool IsIMO { get; set; } = false;

    //[Required]
    public double NetWt { get; set; } = 1;
    //[Required]
    public double GrossWt { get; set; } = 1;
    public double? Volume { get; set; }
    

    [JsonIgnore]
    public DocumentEntity Document { get; set; }
}
