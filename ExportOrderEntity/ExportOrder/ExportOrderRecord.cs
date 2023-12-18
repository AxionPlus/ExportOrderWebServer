
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExportOrderEntites.ExportOrder;

public class ExportOrderRecord
{
    [Key]
    public long Id { get; set; }

#pragma warning disable CS8618
    [Required]
    [MaxLength(11)]
    public string CntrNum { get; set; }
    public CntrTpSz? CntrType { get; set; }
    public double CntrTareWt { get; set; }
    [Required]
    public string Seal { get; set; }        // change to string? and delete [Required]

    //public IList<ContainerContent> Contents { get; set; } = new List<ContainerContent>();
    public List<ContainerContent> Contents { get; set; } = new List<ContainerContent>();
    [JsonIgnore]
    public ExportOrderEntity ExportOrder { get; set; }
    [NotMapped]
    public bool IsShowCntrContent { get; set; } = false;
}