namespace ExportOrderEntites.Catalog;

/// Дополнительные еденицы измерения
public class SupplementaryUnitCatalog : CatalogEntity
{
    public ushort? Code { get; set; }                       // Код
    public string ShortName { get; set; } = string.Empty;   // Наименование 
    public string FullName { get; set; } = string.Empty;    // Полное наименование
}
