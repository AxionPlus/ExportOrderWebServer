using ExportOrderEntites.ExportOrder;
using System.ComponentModel.DataAnnotations;


namespace ExportOrderEntites.DTO.ExportOrder;

public class ContainerRecordDTO
{
    public int Id { get; set; }

#pragma warning disable CS8618
    [Required]
    public CntrEntity Cntr { get; set; }
    [Required]
    public string Seal { get; set; }
    [Required]
    public double NetWt { get; set; }
    [Required]
    public double GrossWt { get; set; }




    #region DocumentRecord 

    public int Seq { get; set; }
    public string? CommodityName { get; set; }
    public string? CommodityEngName { get; set; }
    public string? CommodityHSCode { get; set; }

    #endregion



    public ExportOrderEntity ExportOrder { get; set; }
}
