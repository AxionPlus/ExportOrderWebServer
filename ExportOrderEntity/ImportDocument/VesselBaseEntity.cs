using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportOrderEntites.ImportDocument
{
    public class VesselBaseEntity:BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string FlagRu { get; set; } = string.Empty;
        public string FlagEn { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string RolisCode { get; set; } = string.Empty;
    }
}
