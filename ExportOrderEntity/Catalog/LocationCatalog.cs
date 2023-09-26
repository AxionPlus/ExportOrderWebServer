using System.ComponentModel.DataAnnotations.Schema;

namespace ExportOrderEntites.Catalog;

public class LocationCatalog : CatalogEntity
{

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    public string? Name { get; set; }    
    public string? NameEn { get; set; }
    public string? UnLocode { get; set; }

    public CountryCatalog Country { get; set; }
}