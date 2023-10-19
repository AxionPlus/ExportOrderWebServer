using ExportOrderEntites.MyCompany;

namespace ExportOrderWebServer.Areas.ExpOrder.Provider;

public interface IExportOrderProvider : IEntityProvider<ExportOrderEntity>
{
    Task<ExportOrderDTO> GetItemDTOAsync(long Id);
    //Task<ExportOrderDTO> GetItemDTOAsync(string Voyage);
    Task<AppObjectResponse> NewExportOrderRecordAsync(ExportOrderRecord item);
    Task<IEnumerable<BLDTO>> GetManifestItemsAsync(long vslCallId, long carrierId);
    Task<IEnumerable<PersonEntity>> GetPersonAsync();    
}
