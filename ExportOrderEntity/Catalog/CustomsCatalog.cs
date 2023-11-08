using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.Catalog;

public class CustomsCatalog : CatalogEntity
{
    public string? Code { get; set; }            // "10317090";
    public string? Office { get; set; }          // "НОВОРОССИЙСКИЙ ЗАПАДНЫЙ ТАМОЖЕННЫЙ ПОСТ";
    public string? OfficeShort { get; set; }     // "ЗАПАДНЫЙ Т/П";
    public string? Dapartment { get; set; }      // "ОТО И ТК НОВОРОССИЙСК";
    public string? Email { get; set; }           // e-mail для отправки xml файла
}
