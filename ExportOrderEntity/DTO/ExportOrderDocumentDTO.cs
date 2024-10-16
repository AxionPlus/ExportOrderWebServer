namespace ExportOrderEntites.DTO;

public class ExportOrderDocumentDTO
{
    public long Id { get; set; }
    public string? DocumentName { get; set; }
    public List<ExportOrderDocumentRecordDTO> DocumentRecords { get; set; } = new List<ExportOrderDocumentRecordDTO>();
}
