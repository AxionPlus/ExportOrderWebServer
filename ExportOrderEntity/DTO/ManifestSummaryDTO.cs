namespace ExportOrderEntites.DTO;

public class ManifestSummaryDTO
{
    // Count
    public int Full20Count { get; set; }
    public int Full40Count { get; set; }
    public int Full45Count { get; set; }
    public int Empty20Count { get; set; }
    public int Empty40Count { get; set; }
    public int Empty45Count { get; set; }

    // Total Full
    public double Full20Weight { get; set; }
    public double Full40Weight { get; set; }
    public double Full45Weight { get; set; }

    // Total Tare
    public double Full20Tare { get; set; }
    public double Full40Tare { get; set; }
    public double Full45Tare { get; set; }
    public double Empty20Tare { get; set; }
    public double Empty40Tare { get; set; }
    public double Empty45Tare { get; set; }

    // Grand Total
    public double Total20 { get; set; }
    public double Total40 { get; set; }
    public double Total45 { get; set; }
    public int TotalCntrs { get; set; }
    public double TotalWeight { get; set; }
    public double TotalTare { get; set; }
    public double Total { get; set; }
}
