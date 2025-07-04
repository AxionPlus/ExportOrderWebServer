using ExportOrderEntites.BillofLading;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExportOrderEntites.BillofLading.Dto;

namespace ExportOrderEntites.ImportVesselCall.Dto
{
    public class ImportVesselCallDto
    {
        public string VesselName { get; set; }        // поле связи
        public string VesselVoyage { get; set; }        // поле связи
        public string VesselFlag { get; set; }        // поле связи
        public string CustomsPostCode { get; set; }        // поле связи
       public DateTime ETA { get; set; }

       public string DeparturePortName { get; set; }        // поле связи
       public string DeparturePortCode { get; set; }        // поле связи
       public string DeparturePortCountryCode { get; set; }        // поле связи

        public IEnumerable<BillOfLadingDto> BillofLadings { get; set; } = new List<BillOfLadingDto>();  //public IList<ExportOrderEntity>
    }
}
