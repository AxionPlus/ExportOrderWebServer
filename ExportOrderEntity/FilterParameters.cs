
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites;

[Keyless]
public class FilterParameters
{
    public string? Name { get; set; }
    public string? NameEn { get; set; }
    public string? Country { get; set; }
    public string? UNLocode { get; set; }
    public string? Location { get; set; }
    public string? Carrier { get; set; }
    public string? IMO { get; set; }
    [MaxLength(10)]
    public string? HScode { get; set; }
    public string? VesselName { get; set; }
    public string? Voyage { get; set; }
    public string? TerminalName { get; set; }
    public string? POD { get; set; }    
    public DateTime? ETA { get; set; }
    public DateTime? ETS { get; set; }
    public bool? IsOnmit { get; set; }

    [MaxLength(11)]
    public string? CntrNum { get; set; }
    public string? CntrType { get; set; }
    public bool? CntrIsSOC { get; set; }

    public string? Document { get; set; }
    public string? Shipper { get; set; }
    public string? Consignee { get; set; }
    public string? CargoDescriptionShort { get; set; }

    public string? ExportOrderNum { get; set; }      // номер поручения

}
