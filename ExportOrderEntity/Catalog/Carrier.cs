using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExportOrderEntites.Catalog;

public class CarrierCatalog : CatalogEntity
{
    //public string? FullName { get; set; }
    //public string? ShortName { get; set; }

    public string? Name { get; set; }
    public string? NameEn { get; set; }
    [JsonIgnore]
    public IEnumerable<CarrierTerminalDetails> TerminalDetails { get; set; } = new List<CarrierTerminalDetails>();
}

public class CarrierTerminalDetails
{
    [Key]
    public long Id { get; set; }
    public string? TerminalName { get; set; }
    public string? Contract { get; set; }
    public DateTime? DateContract { get; set; }
    [JsonIgnore]
    public CarrierCatalog? Carrier { get; set; }
}
