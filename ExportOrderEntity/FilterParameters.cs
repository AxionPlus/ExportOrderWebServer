
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites;

[Keyless]
public class FilterParameters
{
    //public string? POL { get; set; }
    //public string? POD { get; set; }
    public string? CntrNum { get; set; }
    public string? CntrType { get; set; }
    public IEnumerable<string>? CntrTypes { get; set; } = new List<string>();
    //public bool? CntrSOC { get; set; }
    //public int? CntrMaxPayLoad { get; set; }
    //public bool IsOverCntrMaxPayLoad { get; set; } = false;
    public string? BolNo { get; set; }
    public List<string> BolsNo { get; set; } = new List<string>();
    [MaxLength(10)]
    public string? HScode { get; set; }
    public string? Name { get; set; }
    public string? NameEn { get; set; }



    public string? Place { get; set; }
    public string? Contractor { get; set; }
    public string? ContractorINN { get; set; }
    public string? ContractorKPP { get; set; }
    public string? Carrier { get; set; }
    public string? Owner { get; set; }
    public bool IsCustomer { get; set; } = false;
    public bool IsContractor { get; set; } = false;
    public string? Status { get; set; }                // for any enum class
    public string? VesselName { get; set; }
    public string? VesselVoyage { get; set; }
    public string? CntrEventType { get; set; }
    public IEnumerable<string>? CntrEventMovements { get; set; } = new List<string>();
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public DateTime? DateDISL { get; set; }             // date of Discharged Laden
    public DateTime? DateGOLD { get; set; }             // date of Gate Out Laden Depot
    public DateTime? DateGIED { get; set; }             // date of Gate In Empty Depot (returned Empty on terminal)

    public bool? HasErrors { get; set; }
    public object? Errors { get; set; }

    public bool IsDamaged { get; set; }
    public bool IsSpecialInstruction { get; set; }


    public string? TerminalName { get; set; }
    public string? DepotName { get; set; }
    public string? BookingParty { get; set; }

    public string? AgreementNo { get; set; }
    public string? AgreementType { get; set; }
    public string? AgreementStatus { get; set; }


    public string? InvoiceNo { get; set; }
    public bool IsInvoiceVAT { get; set; } = false;
    public bool? IsPaid { get; set; }
    public DateTime? Dated { get; set; }


    public bool HasAgreement { get; set; } = false;
    public bool? HasInvoice { get; set; }

    public bool? IsSend { get; set; }
    public bool? IsPassedTo1C { get; set; }
}
