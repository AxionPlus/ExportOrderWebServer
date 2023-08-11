using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.Cntr;

public class CntrTpSz
{
    [Key]
    [MaxLength(4)]
#pragma warning disable CS8618
    [Required]
    public string ISO { get; set; }                 //ISOИндентификатор типа контейнера

    [Required]
    public string? Normolize { get; set; }     //Описание контейнера


    public override string ToString()
    {
        return Normolize!.ToString();
    }

}