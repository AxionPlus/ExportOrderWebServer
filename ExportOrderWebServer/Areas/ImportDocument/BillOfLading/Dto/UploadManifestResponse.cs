namespace ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto
{
    public class UploadManifestResponse
    {
        public bool Success { get; set; }
        public int ProcessedFiles { get; set; }
        public int ParsedBills { get; set; }
        public int SavedBills { get; set; }
        public int SkippedBills { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<BillOfLadingBaseDto> Bills { get; set; } = new();
        public List<BillOfLadingBaseDto> ExistedBills { get; set; } = new();
    }
}
