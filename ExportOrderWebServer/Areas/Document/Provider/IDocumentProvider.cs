namespace ExportOrderWebServer.Areas.Document.Provider;

public interface IDocumentProvider : IEntityProvider<DocumentEntity>
{
    Task<DocumentEntity> GetDocumentAsync(string Num);
}
