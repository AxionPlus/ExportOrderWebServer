using ExportOrderEntites.ReleaseRecord;

namespace ExportOrderWebServer.Areas.Import.ReleaseRecords.Dto
{
    public class ReleaseRecordDto
    {
        public Guid Id { get; set; }
        public bool IsSelect { get; set; }
        public string? BillofLadingNum { get; set; }
        public string? ContainerNum { get; set; }
        public string DocNumber { get; set; }

        public required string ContainerType { get; set; }
        public DateTime? ReleaseTo { get; set; }
        public ReleaseMode? ReleaseMode { get; set; }
        public ReleaseStatus? ReleaseStatus { get; set; }
        public string? ReleaseUID { get; set; }   // Key to the Client
        public string? TerminalName { get; set; }
        public string? LineName { get; set; }

        public IEnumerable<string> Remarks { get; set; } = new List<string>();
        public DateTime? SentToCustomerNoticeTime { get; set; }
    }



}
