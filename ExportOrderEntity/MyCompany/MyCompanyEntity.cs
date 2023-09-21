
using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.MyCompany;

public class MyCompanyEntity
{
    [Key]
    public uint Id { get; set; }
    public string? Name { get; set; }
    public IList<PersonEntity>? Persons { get; set; } = new List<PersonEntity>();

}

public class PersonEntity
{
    [Key]
    public uint Id { get; set; }
    public string? Name { get; set; }   // = "Д.В. Меркульцев";
    public string? Phone { get; set; }  // = "+7 918 6624251";
}
