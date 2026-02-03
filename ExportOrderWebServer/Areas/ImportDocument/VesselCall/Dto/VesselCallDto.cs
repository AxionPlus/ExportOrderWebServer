
using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Terminal.Dto;
using global::ExportOrderWebServer.Areas.ImportDocument.Port.Dto;
using global::ExportOrderWebServer.Areas.ImportDocument.Vessel.Dto;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;

public class VesselCallDto : BaseEntity
    {
        public string? GoogleTableUrl { get; set; } = string.Empty;
    public Guid VesselId { get; set; }
        public VesselDto Vessel { get; set; }

        public string VoyageNo { get; set; } = string.Empty;
        public string TerminalVoyageNo { get; set; } = string.Empty;

        public Guid TerminalId { get; set; }
        public TerminalDto Terminal { get; set; }

        public DateTime? ETA { get; set; }
        public DateTime? ETS { get; set; }

        public Guid PortOfLoadingId { get; set; }
        public PortDto PortOfLoading { get; set; }

        public List<BillOfLadingBaseDto> BillOfLadings { get; set; } = new ();

        public string FeederBlNo { get; set; } = string.Empty;

        [NotMapped]
        public string VesselCallDisplay => $"{Vessel?.Name} - {VoyageNo} ({ETA:dd.MM.yyyy})";

        [NotMapped]
        public string FullInfo => $"{Vessel?.Name} {VoyageNo} → {Terminal?.Name} ETA: {ETA:dd.MM.yyyy HH:mm}";

        public bool IsNew => Timestamp == 0;
    }

