namespace ExportOrderEntites
{

    public abstract class EntityBase : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }

        /// <summary>Row version</summary>
        [System.ComponentModel.DataAnnotations.Timestamp]
        public uint Version { get; set; }

        public long LockToken { get; set; }

        public long Timestamp { get; set; }
    }
}
