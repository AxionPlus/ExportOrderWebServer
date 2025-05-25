namespace ExportOrderEntites
{
    public interface IEntity
    {
        Guid Id { get; set; }

        DateTimeOffset CreatedAt { get; set; }

        DateTimeOffset? UpdatedAt { get; set; }

        DateTimeOffset? DeletedAt { get; set; }

        long LockToken { get; set; }

        uint Version { get; set; }

        long Timestamp { get; set; }
    }
}
