using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class UploadExcelDTO
{
    [Key]
    public long Id { get; set; }
    public string? DocumentName {get; set; }
    public uint? SeqCommodity { get; set; }
    public string? CntrNum { get; set; }
    public string? CntrType { get; set; }
    public double? CntrTareWt { get; set; }
    public string? Seal { get; set; }
    public uint? PackageQty { get; set; }
    public string? PackageName { get; set; }
    public double? NetWt { get; set; }
    public double? GrossWt { get; set; }
}
