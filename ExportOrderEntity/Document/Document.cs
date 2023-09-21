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
//#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    public DocumentCustomer? Shipper { get; set; }
    [Required]
    public DocumentCustomer? Consignee { get; set; }
//#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.


    public IList<DocumentRecord> Records { get; set; } = new List<DocumentRecord>();

    [JsonIgnore]
    public IEnumerable<ExportOrderEntity>? ExportOrders { get; set; }

    [NotMapped]
    public bool ShowDetails { get; set; } = false;

}
