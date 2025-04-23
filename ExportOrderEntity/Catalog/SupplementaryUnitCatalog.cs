namespace ExportOrderEntites.Catalog;

/// Дополнительные еденицы измерения
public class SupplementaryUnitCatalog : CatalogEntity
{
#pragma warning disable CS8618
    public ushort Code { get; set; }           // Код
    public string ShortName { get; set; }      // Наименование 
    public string FullName { get; set; }       // Значение
}
