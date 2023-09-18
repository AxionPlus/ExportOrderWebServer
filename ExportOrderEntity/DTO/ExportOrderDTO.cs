using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class ExportOrderDTO
{
    [Key]
    public long Id { get; set; }

#pragma warning disable CS8618
    public string? Num { get; set; }
    public string? Dated { get; set; }
    public string? VesselName { get; set; }
    public string? Voyage { get; set; }
    public string? DateOfLoading { get; set; }
    public string? POL { get; set; } = "Новороссийск, Россия";    
    public string? POD { get; set; }
    //public string? Shipper { get; set; }
    //public string? Consignee { get; set; }
    public string? Person { get; set; } = "Д.В. Меркульцев т. +7 918 6624251";

    //public IEnumerable<DocumentDTO>? _Documents { get; set; } = new List<DocumentDTO>();
    public IEnumerable<ExportOrderRecordDTO>? exportOrderRecordsDTO { get; set; } = new List<ExportOrderRecordDTO>();

}
