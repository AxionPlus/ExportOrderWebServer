namespace ExportOrderWebServer.Areas.Document.Provider;

public interface IDocumentProvider : IEntityProvider<DocumentEntity>
{   
    Task<DocumentEntity?> GetDocumentAsync(string Num);
    Task<IEnumerable<DocumentEntity>> GetDocumentItemsAsync();
    Task<DocumentRecord> GetDocumentRecordAsync(string Num, int index);
}
