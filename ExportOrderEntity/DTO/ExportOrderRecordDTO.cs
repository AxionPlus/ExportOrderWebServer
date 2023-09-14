using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class ExportOrderRecordDTO
{
    [Key]
    public uint Id { get; set; }

#pragma warning disable CS8618

    public int IndexExpRecord { get; set; }               // Record index for RDLC Report    
    public string Cntr { get; set; }
    public string CntrType { get; set; }
    public double? CntrTareWt { get; set; }
    public string Seal { get; set; }

    public int Quantity { get; set; }
    public double NetWt { get; set; }
    public double GrossWt { get; set; }

    public string? DocumentName { get; set; }              // Declaration number
    public string? CommodityName { get; set; }
    public string? HSCode { get; set; }
    public string? IMO { get; set; }
    public string? UNNO { get; set; }
    public bool? IsIMO { get; set; }


    //public IEnumerable<ContainerContentDTO>? ContentsDTO { get; set; } = new List<ContainerContentDTO>();


    public ExportOrderEntity? ExportOrder { get; set; }  // ???
}
