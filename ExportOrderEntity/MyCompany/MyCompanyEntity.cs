using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.MyCompany;

public class MyCompanyEntity : Entity
{
    public string? Name { get; set; }
    public IList<PersonEntity>? Persons { get; set; } = new List<PersonEntity>();

    //public int VersionNo { get; set; }
    //public EntityStatus Status { get; set; } = EntityStatus.New;
    //public ApplicationUser? ConfirmedBy { get; set; }
    //public DateTime? ConfirmedDate { get; set; }

}

public class PersonEntity : CatalogEntity
{    
    public string? Name { get; set; }   // = "Д.В. Меркульцев";
    public string? Phone { get; set; }  // = "+7 918 6624251";
    public string? Document { get; set; }   // паспорт: 0322 137064 ГУ МВД ПО КРАСНОДАРСКОМУ КРАЮ 24.05.2022
}
