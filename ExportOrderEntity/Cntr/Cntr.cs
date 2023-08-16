
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ExportOrderEntites.Cntr;

public class CntrEntity
{
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.

    [Key]
    [MaxLength(11)]
    public string Num { get; set; }
    public CntrTpSz TpSz { get; set; }
    public ushort TareWt { get; set; }
    public ushort MaxPayLoad { get; set; }
    public bool IsSOC { get; set; }
    public CarrierCatalog Carrier { get; set; }

    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public byte[] Version { get; set; }
}
