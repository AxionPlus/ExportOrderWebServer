namespace ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;

    public class UploadManifestRequest
    {
        public Guid? VesselCallId { get; set; }
        public string? VoyageNumber { get; set; }
        public string? VesselName { get; set; }
        public IFormFileCollection Files { get; set; }
        public bool AutoMatchVessel { get; set; } = true;
        public bool CreateNewIfNotFound { get; set; } = false;
    }

