
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExportOrderEntites;

public abstract class Entity
{
    [Key]
    public long Id { get; set; }

    public EntityStatus Status { get; set; } = EntityStatus.New;

#pragma warning disable CS8618
    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public byte[] Version { get; set; }
    public ApplicationUser CreateUser { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
#pragma warning restore CS8618
  
}
