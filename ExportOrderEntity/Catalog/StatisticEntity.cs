namespace ExportOrderEntites.Catalog;

public class StatisticEntity
{
    public DateTime? DatedFrom { get; set; } = DateTime.Today;
    public DateTime? DatedTo { get; set; } = DateTime.Today;

    // Export Orders
    public uint EOsUnderway { get; set; } = 0;
    public uint EOsCompleted { get; set; } = 0;
    public uint EOsCancelled { get; set; } = 0;
    public uint EOsOverall { get; set; } = 0;
    // Containers
    public uint CntrsUnderway { get; set; } = 0;
    public uint CntrsCompleted { get; set; } = 0;
    public uint CntrsCancelled { get; set; } = 0;
    public uint CntrsOverall { get; set; } = 0;
}
