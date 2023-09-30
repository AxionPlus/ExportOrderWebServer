using ExportOrderEntites.DTO;
using ExportOrderEntites.MyCompany;

namespace ExportOrderWebServer.Areas.ExpOrder.Provider;

public interface IExportOrderProvider : IEntityProvider<ExportOrderEntity>
{
    Task<ExportOrderDTO> GetItemDTOAsync(long eoId);
    Task<IEnumerable<ExportOrderDTO>> _GetManifestItemsAsync(long vslCallId, long carrierId);

    Task<IEnumerable<BLDTO>> GetManifestItemsAsync(long vslCallId, long carrierId);

    Task<IEnumerable<PersonEntity>> GetPersonAsync();
    
}
