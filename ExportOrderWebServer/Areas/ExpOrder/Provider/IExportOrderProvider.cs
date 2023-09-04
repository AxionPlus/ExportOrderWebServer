namespace ExportOrderWebServer.Areas.ExpOrder.Provider;

public interface IExportOrderProvider : IEntityProvider<ExportOrderEntity>
{
    Task<AppObjectResponse> AddUploadedFileItemsAsync(IEnumerable<ExportOrderRecord> items);
}
