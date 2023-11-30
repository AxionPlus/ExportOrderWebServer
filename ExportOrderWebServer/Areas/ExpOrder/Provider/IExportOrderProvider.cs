namespace ExportOrderWebServer.Areas.ExpOrder.Provider;

public interface IExportOrderProvider : IEntityProvider<ExportOrderEntity>
{
    Task<ExportOrderDTO> GetExportOrderDTOAsync(long Id);
    Task<ExportOrderDTO> GetBLDTOAsync(long Id);
    Task<IEnumerable<ManifestDTO>> GetManifestDTOAsync(long id);
    Task<IEnumerable<PersonEntity>> GetPersonsAsync();
    Task<AppObjectResponse> GetExportOrderRecordItemAsync(long id);
    Task<AppObjectResponse> IsExistRecordItemAsync(string recordItemName);
    Task<AppObjectResponse> ModifyExportOrderRecordItemAsync(ExportOrderRecord item);
}
