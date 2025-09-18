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
        public string VesselName { get; set; }        
        public string VesselVoyage { get; set; }      
        public string VesselFlag { get; set; }        
        public string CustomsPostCode { get; set; }   
       public DateTime ETA { get; set; }

       public string DeparturePortName { get; set; }       
       public string DeparturePortCode { get; set; }       
       public string DeparturePortCountryCode { get; set; }
       public string DeparturePortCountry { get; set; }    

        public IEnumerable<BillOfLadingDto> BillofLadings { get; set; } = new List<BillOfLadingDto>();  //public IList<ExportOrderEntity>
    }
}
