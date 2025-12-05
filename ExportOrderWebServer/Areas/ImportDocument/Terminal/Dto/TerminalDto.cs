using ExportOrderEntites.ImportDocument;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExportOrderWebServer.Areas.ImportDocument.Terminal.Dto
{
    public class TerminalDto : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CustomsPost { get; set; } = string.Empty;
        public string CustomsPostName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => !string.IsNullOrWhiteSpace(CustomsPostName)
            ? $"{Name} ({CustomsPostName})"
            : Name;

        [NotMapped]
        public string TerminalInfo => $"Терминал: {Name}, Таможенный пост: {CustomsPostName} ({CustomsPost})";

        public bool IsNew => Timestamp == 0;
    }
}