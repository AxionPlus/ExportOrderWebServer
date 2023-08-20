namespace ExportOrderEntites.DTO;

public class ApplicationUserDTO
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public IEnumerable<string>? Roles { get; set; }
}
