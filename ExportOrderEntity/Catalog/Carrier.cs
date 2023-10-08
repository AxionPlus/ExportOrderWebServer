using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExportOrderEntites.Catalog;

public class CarrierCatalog : CatalogEntity
{
    public string? Name { get; set; }
    public string? NameEn { get; set; }
    public BLTemplate BlTemplate { get; set; } = 0;

    public IList<CarrierTerminalDetails> CarrierDetails { get; set; } = new List<CarrierTerminalDetails>();
}

public class CarrierTerminalDetails
{
    [Key]
    public long Id { get; set; }
#pragma warning disable CS8618
    public string TerminalName { get; set; }        // change to TerminalId
    public string? Contract { get; set; }
    public DateTime? DateContract { get; set; }
    public string? AgentPOL { get; set; }    


    [JsonIgnore]
    public CarrierCatalog Carrier { get; set; }
}

public enum BLTemplate
{
    standard,
    ametist,
    certa_lam,
    safetrans
}
