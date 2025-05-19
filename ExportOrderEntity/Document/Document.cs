using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExportOrderEntites.Document;

public class DocumentEntity : Entity
{
    [Required]
    public string? Name { get; set; }                   // Change to Not nullable
    public DocumentType Type { get; set; }
    public string? Description { get; set; }
    public string? ContarctNo { get; set; }                                             // DELETE !!! (in db also)
    public DocumentCustomer? Shipper { get; set; }      // Change to Not nullable
    public DocumentCustomer? Consignee { get; set; }    // Change to Not nullable
    public List<DocumentRecord> Records { get; set; } = new List<DocumentRecord>();

    [JsonIgnore]
    public IEnumerable<ExportOrderEntity>? ExportOrders { get; set; }    
}
