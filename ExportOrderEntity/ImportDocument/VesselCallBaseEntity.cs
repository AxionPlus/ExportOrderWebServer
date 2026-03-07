
using System.ComponentModel.DataAnnotations.Schema;


namespace ExportOrderEntites.ImportDocument
{
    public class VesselCallBaseEntity : BaseEntity
    {
        public string? GoogleTableUrl { get; set; } = string.Empty;

        [ForeignKey("Vessel")]
        public Guid VesselId { get; set; }
        public VesselBaseEntity Vessel { get; set; }

        public bool IsFinalized { get; set; }
        public string VoyageNo { get; set; } = string.Empty;
        public string TerminalVoyageNo { get; set; } = string.Empty;


        public string CaptainName { get; set; } = string.Empty;
        public string CaptainLastName { get; set; } = string.Empty;
        public DateTime? TranslateUpdateTime { get; set; } 

        [ForeignKey("Terminal")]
        public Guid TerminalId { get; set; }
        public TerminalBaseEntity Terminal { get; set; }

        public DateTime ETA { get; set; }
        public DateTime ETS { get; set; }


        [ForeignKey("PortOfLoading")]
        public Guid PortOfLoadingId { get; set; }
        public PortBaseEntity PortOfLoading { get; set; }
        public DateTime? PortOfLoadingDate { get; set; }


        public List<BillOfLadingBaseEntity> BillOfLadings { get; set; } = new List<BillOfLadingBaseEntity>();


        public string FeederBlNo { get; set; } = string.Empty;
    }
}
