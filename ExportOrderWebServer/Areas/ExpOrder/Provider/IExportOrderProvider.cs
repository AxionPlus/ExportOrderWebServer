using ExportOrderEntites.DTO;
using ExportOrderEntites.MyCompany;

namespace ExportOrderWebServer.Areas.ExpOrder.Provider;

public interface IExportOrderProvider : IEntityProvider<ExportOrderEntity>
{
    Task<ExportOrderDTO> GetItemDTOAsync(long id);  
    //Task<MyCompanyEntity> GetMyCompanyAsync();
}
