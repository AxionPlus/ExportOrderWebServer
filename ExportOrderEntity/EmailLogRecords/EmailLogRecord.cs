using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.EmailLogRecords
{
    public class EmailLogRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EmailSubject { get; set; }
        public string From { get; set; }
        public DateTime DateReciept { get; set; }   // DateReceipt
        public string FileName { get; set; }
        public string FileContent { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public bool IsHandle { get; set; } = false;

        public EmailLogRecordType Type { get; set; }

    }
    public enum EmailLogRecordType
    {
        ContainerEvent,
        Invoice,
        ResponseTerminal //RespomseNLE
    }
}
