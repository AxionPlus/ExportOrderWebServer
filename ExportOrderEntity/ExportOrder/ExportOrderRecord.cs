
using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.ExportOrder;

public class ExportOrderRecord
{
    public long Id { get; set; }

#pragma warning disable CS8618
    [Required]
    [MaxLength(11)]
    public string CntrNum { get; set; }
    public CntrTpSz CntrType { get; set; }
    public double? CntrTareWt { get; set; }
    [Required]
    public string Seal { get; set; }

    public IList<ContainerContent> Contents { get; set; } = new List<ContainerContent>();
    public ExportOrderEntity ExportOrder { get; set; }   // added: = new ExportOrderEntity();
}