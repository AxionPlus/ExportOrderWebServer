using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class ExportOrderDTO
{
    [Key]
    public long Id { get; set; }
    public string? Num { get; set; }
    public DateTime? Dated { get; set; }
    public string? VesselName { get; set; }
    public string? Voyage { get; set; }
    public DateTime? DateOfLoading { get; set; }
    public string? POL { get; set; } = "Новороссийск, Россия";    
    public string? POD { get; set; }
    public string? Shipper { get; set; }
    public string? Consignee { get; set; }


    //public IEnumerable<DocumentDTO>? _Documents { get; set; } = new List<DocumentDTO>();
    public IEnumerable<ExportOrderRecordDTO>? _ExportOrderRecords { get; set; } = new List<ExportOrderRecordDTO>();

}
