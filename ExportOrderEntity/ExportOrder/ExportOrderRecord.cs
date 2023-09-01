
using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.ExportOrder;

public class ExportOrderRecord
{
    public long Id { get; set; }

#pragma warning disable CS8618
    [Required]
    public CntrEntity Cntr { get; set; }
    //[Required]
    //public CntrEntity CntrNum { get; set; }
    [Required]
    public string Seal { get; set; }
    //public CntrTpSz CntrType { get; set; }
    //public double? CntrTareWt { get; set; }

    public IList<ContainerContent> Contents { get; set; } = new List<ContainerContent>();

    public ExportOrderEntity ExportOrder { get; set; }

}