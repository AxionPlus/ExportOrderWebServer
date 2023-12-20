using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExportOrderEntites.Document;

public class DocumentEntity : Entity
{
    [Required]
    public string? Name { get; set; }
    public DocumentType Type { get; set; }
    public string? Description { get; set; }
    public string? ContarctNo { get; set; }
    [Required]
    public DocumentCustomer? Shipper { get; set; }
    [Required]
    public DocumentCustomer? Consignee { get; set; }

    public List<DocumentRecord> Records { get; set; } = new List<DocumentRecord>();

    [JsonIgnore]
    public IEnumerable<ExportOrderEntity>? ExportOrders { get; set; }
    [NotMapped]
    public bool ShowDetails { get; set; } = false;
}
