using System.ComponentModel.DataAnnotations.Schema;

namespace ExportOrderEntites.Catalog;

public class CountryCatalog : CatalogEntity
{
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    public string RUS { get; set; }
    public string ENG { get; set; }
    [NotMapped]
    public string Code { get; set; }
}