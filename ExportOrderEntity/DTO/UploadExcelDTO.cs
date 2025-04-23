using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class UploadExcelDTO
{
    [Key]
    public long Id { get; set; }
    public string? DocumentName {get; set; }
    public int SeqCommodity { get; set; }
    public string? CntrNum { get; set; }
    public string? CntrType { get; set; }
    public double CntrTareWt { get; set; } = 0;
    public string? Seal { get; set; }
    public uint? PackageQty { get; set; }
    public string? PackageName { get; set; }
    public double? NetWt { get; set; }
    public double? GrossWt { get; set; }
    public double? SupplementaryUnitQuantity { get; set; }
    public ushort? SupplementaryUnitCode { get; set; }    
}

public class UploadExcelSummary
{
    //public List<UploadExcelDTO>? UploadedDTO { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public IEnumerable<string> Errors { get; set; } = new List<string>();
    public bool HasErrors => Errors.Any();
    public bool HasCriticalErrors { get; set; } = false;
}