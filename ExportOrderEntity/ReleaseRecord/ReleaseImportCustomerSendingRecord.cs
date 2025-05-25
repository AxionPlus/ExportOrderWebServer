using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.ReleaseRecord
{
    public class ReleaseImportCustomerSendingRecord
    {
        [Key]
        public long Id { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime? SendTime { get; set; }
        public string? UserName { get; set; }
        public string? SendTo { get; set; }
        public ImportReleaseMailSendingMode Mode { get; set; }
    }
    public enum ImportReleaseMailSendingMode
    {
        WaitResponse,
        NeedSend,
        Sent,
        XCancelled,
    }
}
