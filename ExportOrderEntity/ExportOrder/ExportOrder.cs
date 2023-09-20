using ExportOrderEntites.Document;
using ExportOrderEntites.VesselCall;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExportOrderEntites.ExportOrder;

public class ExportOrderEntity : Entity
{

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    public string Num { get; set; }
    public DateTime Dated { get; set; } = DateTime.Today;
    public VesselCallEntity VesselCall { get; set; }
    public CarrierCatalog Carrier { get; set; }

    public List<DocumentEntity> Documents { get; set; } = new List<DocumentEntity>(); 
    public List<ExportOrderRecord> Records { get; set; } = new List<ExportOrderRecord>();

    [NotMapped]
    public IEnumerable<DocumentCustomer> Shippers
    {
        get
        {
            if (Documents is null)
                return Enumerable.Empty<DocumentCustomer>();

            return Documents.Select(x => x.Shipper).ToList()!;
        }
    }

    [NotMapped]
    public IEnumerable<DocumentCustomer> Consignees
    {
        get
        {
            if (Documents is null)
                return Enumerable.Empty<DocumentCustomer>();

            return Documents.Select(x => x.Consignee).ToList()!;
        }
    }
}
