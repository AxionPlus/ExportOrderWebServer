using System.Text.Json.Serialization;

namespace ExportOrderEntites.MyCompany;

public class MyCompanyEntity : Entity
{
    public string? Name { get; set; }
    public string? NameEn { get; set; }
    public string? Email { get; set; }
    public List<PersonEntity> Persons { get; set; } = new List<PersonEntity>();
}

public class PersonEntity : CatalogEntity
{
#pragma warning disable CS8618
    [JsonIgnore]
    public MyCompanyEntity MyCompany { get; set; }
    public string? Name { get; set; }           // Имя    
    public string? SurName { get; set; }        // Отчетсво
    public string? FamilyName { get; set; }     // Фамилия    
    public string? Phone { get; set; }          // +7 918 000 0000;
    public string? BirthYear { get; set; }      // Год рождения
    public string? BirthPlace { get; set; }     // Место рождения (КРАСНОДАРСКИЙ КРАЙ)
    public string? CompanyName { get; set; }    // Место работы
    public string? CompanyEmail { get; set; }    // Место работы
    public string? Address { get; set; }        // Место жительства
    public string? Passport { get; set; }       // паспорт: номер, кем и когда выдан
}