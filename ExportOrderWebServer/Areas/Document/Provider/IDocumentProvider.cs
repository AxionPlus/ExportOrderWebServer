namespace ExportOrderWebServer.Areas.Document.Provider;

public interface IDocumentProvider : IEntityProvider<DocumentEntity>
{
    Task<IEnumerable<DocumentEntity>> GetDocumentsAsync();
}
