using System.ComponentModel.DataAnnotations.Schema;

namespace ExportOrderEntites.MyCompany;

public class MyCompanyEntity : Entity
{
    public string? Name { get; set; }    
    public List<PersonEntity> Persons { get; set; } = new List<PersonEntity>();
}

public class PersonEntity : CatalogEntity
{
    public string? Name { get; set; }           // Имя    
    public string? SurName { get; set; }        // Отчетсво
    public string? FamilyName { get; set; }     // Фамилия    
    public string? Phone { get; set; }          // +7 918 000 0000;
    public string? BirthYear { get; set; }       // Год рождения
    public string? BirthPlace { get; set; }      // Место рождения (КРАСНОДАРСКИЙ КРАЙ)
    public string? Company { get; set; }    // Место работы
    public string? Address { get; set; }        // Место жительства
    public string? Passport { get; set; }       // паспорт: номер, кем и когда выдан

    [NotMapped]
    public bool IsShowPersonDetails { get; set; } = false;
    [NotMapped]
    public bool IsNewPerson { get; set; } = false;
}