namespace ExportOrderWebServer.Areas.ExpOrder.Provider;

public interface IExportOrderProvider : IEntityProvider<ExportOrderEntity>
{
    Task<ExportOrderDTO> GetItemDTOAsync(long Id);
    //Task<IEnumerable<BLDTO>> GetBLDTOAsync(long Id);
    Task<ExportOrderDTO> GetBLDTOAsync(long Id);
    Task<IEnumerable<ManifestDTO>> GetManifestAsync(long id);
    Task<IEnumerable<PersonEntity>> GetPersonsAsync();
}
