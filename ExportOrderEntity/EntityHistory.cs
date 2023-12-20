using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites;

public abstract class EntityHistory
{
    [Key]
    public long Id { get; set; } = 0;

    public EntityStatus Status { get; set; } = EntityStatus.New;
    public HistoryEventMode Mode { get; set; }
    //public string? Remarks { get; set; }
    public ApplicationUser? CreateUser { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
}

public enum HistoryEventMode
{
    New, Modify, Delete
}
