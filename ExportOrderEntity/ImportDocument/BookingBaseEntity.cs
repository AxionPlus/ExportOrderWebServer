using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportOrderEntites.ImportDocument
{
    public class BookingBaseEntity : BaseEntity
    {

        [Required]
        public string Num { get; set; } = string.Empty;

       // public List<BillOfLadingBaseEntity> BillOfLadings { get; set; } = new List<BillOfLadingBaseEntity>();
    }
}
