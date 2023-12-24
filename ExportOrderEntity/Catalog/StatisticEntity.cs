namespace ExportOrderEntites.Catalog;

public class StatisticEntity
{
    //public EntityStatus Status { get; set; }
    public DateTime? DatedFrom { get; set; } = DateTime.Today;
    public DateTime? DatedTo { get; set; } = DateTime.Today;
    public uint CountUnderway { get; set; } = 0;
    public uint CountCompleted { get; set; } = 0;
    public uint CountCancelled { get; set; } = 0;
    public uint CountOverall { get; set; } = 0;
}
