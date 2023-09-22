using Microsoft.EntityFrameworkCore;

namespace ExportOrderEntites.DTO;

[Keyless]
public class _CntrDTO
{
    public string? Carrier { get; set; }
    public string? Num { get; set; }
    public string? TpSz { get; set; }
    public double? TareWt { get; set; }
    public double? MaxPayLoad { get; set; }
    public bool IsSOC { get; set; } = false;
}
