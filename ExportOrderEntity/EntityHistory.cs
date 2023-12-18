using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites;

public abstract class EntityHistory
{
    [Key]
    public long Id { get; set; } = 0;

    public string? Remarks { get; set; }
    public ApplicationUser? CreateUser { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
}
