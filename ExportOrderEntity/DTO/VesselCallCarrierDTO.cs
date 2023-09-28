using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExportOrderEntites.DTO;

public class VesselCallCarrierDTO
{
    //[Key]
    public long VesselCallId { get; set; }
    public long CarrierId { get; set; }
    public string? CarrierNameEn { get; set; }
    [NotMapped]
    public bool IsShowRecords { get; set; } = false;
    public IEnumerable<VesselCallRecordDTO> RecordsDTO { get; set; } = new List<VesselCallRecordDTO>();
}

public class VesselCallRecordDTO
{
    //[Key]
    public long ExportOrderId { get; set; }
    public string? ExportOrderNum { get; set; }
    public DateTime? ExportOrderDate { get; set; }

    [JsonIgnore]
    public VesselCallCarrierDTO? vesselCallCarrierDTO { get; set; }

}
