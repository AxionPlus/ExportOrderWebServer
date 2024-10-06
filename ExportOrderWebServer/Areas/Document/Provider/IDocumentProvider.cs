namespace ExportOrderWebServer.Areas.Document.Provider;

public interface IDocumentProvider : IEntityProvider<DocumentEntity>
{
    Task<AppObjectResponse> GetItemsAsync(FilterParameters filter);
    Task<DocumentEntity?> GetDocumentAsync(string Num);
    Task<DocumentRecord> GetDocumentRecordAsync(string Num, int index);
    Task<IEnumerable<string>> GetDocumentNames(bool isSelectable);
}
