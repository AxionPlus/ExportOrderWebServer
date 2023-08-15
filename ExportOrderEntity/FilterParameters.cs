
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites;

[Keyless]
public class FilterParameters
{
    [MaxLength(10)]
    public string? HScode { get; set; }
    public string? Name { get; set; }
    public string? NameEn { get; set; }
    public string? Country { get; set; }
    public string? UNLocode { get; set; }
    public string? Location { get; set; }



}
