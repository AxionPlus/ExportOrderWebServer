namespace ExportOrderWebServer.Areas.ExpOrder.Provider;

public interface IExportOrderProvider : IEntityProvider<ExportOrderEntity>
{   
    Task<List<ExportOrderDTO>?> GetItemsOrderDTOAsync(IEnumerable<long> ids);
    Task<List<ExportOrderDTO>?> GetItemsBillDTOAsync(IEnumerable<long> ids);
    Task<IEnumerable<ManifestDTO>?> GetItemsManifestDTOAsync(long id, bool isImo);    
    
    Task<AppObjectResponse> GetVersionAsync(int version);    
    Task<AppObjectResponse> GetExportOrderRecordItemAsync(long id);
    Task<AppObjectResponse> SetNewVesselCall(long[] exportOrderIds, long vesselCallid);
    Task<AppObjectResponse> ModifyItemAsync(ExportOrderEntity item, string UserName = "");

    Task<IEnumerable<PersonEntity>> GetPersonsAsync();
    Task<IEnumerable<string>> GetDocumentExportOrderNums(string documentNum);
    Task<double[]> GetDocumentExportOrderTotalWeights(string documentNum);
    Task<IEnumerable<string>?> GetCntrNumsInVoyage(long eoId, long voyageId);
}