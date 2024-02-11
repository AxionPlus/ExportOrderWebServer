namespace ExportOrderWebServer.Areas.ExpOrder.Provider;

public interface IExportOrderProvider : IEntityProvider<ExportOrderEntity>
{   
    Task<ExportOrderDTO> GetExportOrderDTOAsync(long Id);
    Task<ExportOrderDTO> GetBLDTOAsync(long Id);
    Task<IEnumerable<VoyageManifestDTO>> GetVoyageManifestDTOAsync(long id, bool isImo);
    Task<IEnumerable<PersonEntity>> GetPersonsAsync();


    Task<int> LastVersionAsync();
    Task<AppObjectResponse> GetVersionAsync(int version);    
    Task<AppObjectResponse> GetExportOrderRecordItemAsync(long id);
    Task<AppObjectResponse> IsExistRecordItemAsync(string recordItemName);
    Task<AppObjectResponse> ModifyExportOrderRecordItemAsync(ExportOrderRecord item);
}