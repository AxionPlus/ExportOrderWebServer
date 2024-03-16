namespace ExportOrderWebServer.Areas.Document.Provider;

public interface IDocumentProvider : IEntityProvider<DocumentEntity>
{   
    Task<DocumentEntity?> GetDocumentAsync(string Num);
    Task<IEnumerable<DocumentEntity>> GetDocumentItemsAsync();
    Task<IEnumerable<DocumentEntity>> GetDocumentItemsToCheckAsync();
    Task<DocumentRecord> GetDocumentRecordAsync(string Num, int index);

    // DELETE
    Task<List<string?>?> GetDocumentNamesAsync();
}
