
using System.ComponentModel.DataAnnotations;

namespace ExportOrderEntites.ImportDocument
{
    public class TerminalBaseEntity:BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string NameRu { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CustomsPost { get; set; } = string.Empty;
        public string CustomsPostName { get; set; } = string.Empty;
        public string ContractId { get; set; } = string.Empty;
        public string ContractName { get; set; } = string.Empty;

    }
}
