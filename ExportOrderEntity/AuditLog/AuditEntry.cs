
namespace ExportOrderEntites.AuditLog
{
    public class AuditEntry
    {
        public int Id { get; set; }
        public string EntityName { get; set; }
        public string Action { get; set; } // "Create", "Update", "Delete"
        public string EntityId { get; set; }
        public Dictionary<string, object> OldValues { get; set; } = new();
        public Dictionary<string, object> NewValues { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
    }
}
