
namespace ExportOrderEntites.MyCompany;

public class MyCompanyEntity
{
    public string? Name { get; set; } = "Рожки да ножки";                                   // Наименование компании
    public IList<PersonEntity> Persons { get; set; } = new List<PersonEntity>();    // Список сотрудников

}

public class PersonEntity
{
    uint Id;

 #pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    string Name { get; set; } = "Д.В. Меркульцев";
    string Phone { get; set; } = "+7 918 6624251";
}
