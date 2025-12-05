using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportOrderEntites.ImportDocument;


public interface IBaseEntity
{

    Guid Id { get; set; }
    BaseEntityStatus Status { get; set; }
    int Version { get; set; }
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    long LockToken { get; set; }
    long Timestamp { get; set; }
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
    string? DeletedBy { get; set; }
    string? DeleteReason { get; set; }
    bool HandledBySystem { get; set; }
}
public class BaseEntity : IBaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public BaseEntityStatus Status { get; set; } = BaseEntityStatus.New;
    public int Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public long LockToken { get; set; }
    public long Timestamp { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? DeletedBy { get; set; }
    public string? DeleteReason { get; set; }

    public bool HandledBySystem { get; set; }
}

public enum BaseEntityStatus
{
    New,
    Checked,
    Completed,
    Canceled
}