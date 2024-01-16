using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.DTO;

public class UploadExcelDTO
{
    [Key]
    public long Id { get; set; }
    public string? DocumentName {get; set; }
    public int? SeqCommodity { get; set; }
    public string? CntrNum { get; set; }
    public string? CntrType { get; set; }
    public double CntrTareWt { get; set; } = 0;
    public string? Seal { get; set; }
    public uint? PackageQty { get; set; }
    public string? PackageName { get; set; }
    public double? NetWt { get; set; }
    public double? GrossWt { get; set; }
}
public class CheckedUploadResult
{
    public IEnumerable<UploadExcelDTO>? UploadedDTO { get; set; } = new List<UploadExcelDTO>();
    public string Summary { get; set; } = string.Empty;
    public IEnumerable<string> Errors { get; set; } = new List<string>();
    public bool HasErrors => Errors.Count() > 0;
}

//public class UploadResult
//{
//    public IEnumerable<ExportOrderRecord>? _ExportOrderRecords { get; set; }
//    public IEnumerable<DocumentEntity>? _Documents { get; set; }
//    public IEnumerable<string>? Summary { get; set; } = new List<string>();
//    public IEnumerable<string>? Errors { get; set; } = new List<string>();
//}

//public class CheckingResult
//{
//    public int CntrCount { get; set; }
//    //public int CntrContentCount { get; set; }
//    public int DocumentCount { get; set; }
//    public IEnumerable<string> Errors { get; set; } = new List<string>();
//    public bool HasErrors => Errors.Count() > 0;
//}