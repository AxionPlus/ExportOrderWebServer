using ExportOrderEntites.MyCompany;

namespace ExportOrderWebServer.Areas.ExpOrder.Provider;

public interface IExportOrderProvider : IEntityProvider<ExportOrderEntity>
{
    Task<ExportOrderDTO> GetItemDTOAsync(long Id);
    //Task<ExportOrderDTO> GetItemDTOAsync(string Voyage);
    Task<IEnumerable<ManifestDTO>> GetManifestAsync(string voyage);
    Task<AppObjectResponse> NewExportOrderRecordAsync(ExportOrderRecord item);
    Task<IEnumerable<PersonEntity>> GetPersonAsync();



    // DELETE all below
    Task<IEnumerable<BLDTO>> GetManifestItemsAsync(long vslCallId, long carrierId);    
    Task<IEnumerable<ManifestDTO>> GetManifestItemsAsync(List<long> vesselCallDeatailIds);
    
}
