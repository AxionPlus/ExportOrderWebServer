namespace ExportOrderWebServer.Areas.ExpOrder.Provider;

public interface IExportOrderProvider : IEntityProvider<ExportOrderEntity>
{
    Task<ExportOrderDTO> GetItemDTOAsync(long Id);
    Task<IEnumerable<ManifestDTO>> GetManifestAsync(long id);
    //Task<IEnumerable<ManifestDTO>> GetFillBillAsync(long id);
    Task<IEnumerable<PersonEntity>> GetPersonAsync();

    // DELETE
    Task<IEnumerable<ManifestDTO>> GetManifestItemAsync(string voyage);
}
