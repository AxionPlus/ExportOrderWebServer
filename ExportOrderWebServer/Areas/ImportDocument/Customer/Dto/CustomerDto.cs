using ExportOrderEntites.ImportDocument;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExportOrderWebServer.Areas.ImportDocument.Customer.Dto
{
    public class CustomerDto : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? NameRu { get; set; }
        public string? AddressRu { get; set; }

        [NotMapped]
        public string DisplayName => !string.IsNullOrWhiteSpace(NameRu) ? NameRu : Name;

        [NotMapped]
        public string DisplayAddress => !string.IsNullOrWhiteSpace(AddressRu) ? AddressRu : Address;

        [NotMapped]
        public string FullDisplay => $"{DisplayName} ({Code}) - {DisplayAddress}";

        public bool IsNew => Timestamp == 0;
    }
}
