
using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.Catalog;

public abstract class CatalogEntity
{
    [Key]
    public uint Id { get; set; } 
    public ApplicationUser CreateUser { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
}
