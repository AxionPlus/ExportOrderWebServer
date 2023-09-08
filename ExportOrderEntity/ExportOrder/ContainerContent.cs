

using ExportOrderEntites.Document;
using System.Text.Json.Serialization;

namespace ExportOrderEntites.ExportOrder;

public class ContainerContent
{
    public long Id { get; set; }
#pragma warning disable CS8618
    public int Quantity { get; set; }
    public double NetWt { get; set; }
    public double GrossWt { get; set; }
    public double Volume { get; set; }
    [JsonIgnore]
    public DocumentRecord DocumentRecord { get; set; }
    [JsonIgnore]
    public ExportOrderRecord ExportOrderRecord { get; set; }
}