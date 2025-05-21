namespace ExportOrderWebServer.Areas.Document.Provider;

public interface IDocumentProvider : IEntityProvider<DocumentEntity>
{
    Task<IEnumerable<DocumentEntity>?> GetItemsAsync(long[] ids); 
    Task<IEnumerable<DocumentEntity>?> GetItemsAsync(string[]? nums);
    Task<IEnumerable<string>> GetDocumentNames(bool isSelectable);
    Task<AppObjectResponse> NewItemsUploadXmlAsync(List<ReadXmlDocumentRecordDTO> readDTOs, string? UserName = "");
    Task<AppObjectResponse> ModifyItemAsync(DocumentEntity item, string? UserName = "");
    Task<AppObjectResponse> NewItemAsync(DocumentEntity item, string? UserName = "");
}