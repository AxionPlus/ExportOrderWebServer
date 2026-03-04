using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportOrderEntites.ImportDocument
{
    public class CustomerBaseEntity:BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string? NameRu { get; set; } = string.Empty;
        public string? AddressRu { get; set; } = string.Empty;
        public string? CountryRu { get; set; } = string.Empty;
    }
}
