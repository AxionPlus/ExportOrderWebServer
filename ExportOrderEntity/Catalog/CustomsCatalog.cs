using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.Catalog;

public class CustomsCatalog : CatalogEntity
{
    public string? CustomsCode { get; set; }            // "10317090";
    public string? CustomsOffice { get; set; }          // "НОВОРОССИЙСКИЙ ЗАПАДНЫЙ ТАМОЖЕННЫЙ ПОСТ";
    public string? CustomsOfficeShort { get; set; }     // "ЗАПАДНЫЙ Т/П";
    public string? CustomsDapartment { get; set; }      // "ОТО И ТК НОВОРОССИЙСК";
    public string? CustomsEmail { get; set; }           // e-mail для отправки xml файла
    public string? CustomsArticle { get; set; }         // "в соответствии со статьей 323 ТК ЕАЭС";
}
