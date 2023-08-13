using Microsoft.EntityFrameworkCore;

namespace ExportOrderEntites.Document;

[Keyless]
public class DocumentCustomer
{
    public string? Name { get; set; }
    public string? EngName { get; set; }

    public string? CountryRUS { get; set; }
    public string? CountryENG { get; set; }
}
