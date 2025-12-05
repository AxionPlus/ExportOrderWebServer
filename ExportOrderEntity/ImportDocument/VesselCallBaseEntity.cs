
using System.ComponentModel.DataAnnotations.Schema;


namespace ExportOrderEntites.ImportDocument
{
    public class VesselCallBaseEntity : BaseEntity
    {


        [ForeignKey("Vessel")]
        public Guid VesselId { get; set; }
        public VesselBaseEntity Vessel { get; set; }

        public string VoyageNo { get; set; } = string.Empty;
        public string TerminalVoyageNo { get; set; } = string.Empty;



        [ForeignKey("Terminal")]
        public Guid TerminalId { get; set; }
        public TerminalBaseEntity Terminal { get; set; }

        public DateTime ETA { get; set; }
        public DateTime ETS { get; set; }


        [ForeignKey("PortOfLoading")]
        public Guid PortOfLoadingId { get; set; }
        public PortBaseEntity PortOfLoading { get; set; }


        public List<BillOfLadingBaseEntity> BillOfLadings { get; set; } = new List<BillOfLadingBaseEntity>();


        public string FeederBlNo { get; set; } = string.Empty;
    }
}
