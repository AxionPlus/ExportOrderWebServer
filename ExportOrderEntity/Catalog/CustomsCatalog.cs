using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.Catalog;

public class CustomsCatalog : CatalogEntity
{
    public string? CustomsCode { get; set; } = "10317090";
    public string? CustomsOffice { get; set; } = "НОВОРОССИЙСКИЙ ЗАПАДНЫЙ Т/П";
    public string? CustomsDapartment { get; set; } = "ОТО И ТК НОВОРОССИЙСК";
    public string? CustomsArticle { get; set; } = "в соответствии со статьей 323 ТК ЕАЭС";
}
