namespace ExportOrderWebServer.Areas.ExportOrder.Dto;

public class ManifestFileDto
{
    public long VesselCallId { get; set; }
    public string? DateExplanation { get; set; }
    public string? VesselName { get; set; }
    public string? VesselFlagVoyage { get; set; }
    public string? CustomsOfficeName { get; set; }
    public string? CustomsOfficeNameShort { get; set; }
    public string? CustomsDapartment { get; set; }
    public string? PersonFamily { get; set; }
    public string? PersonNameSurname { get; set; }
    public string? PersonPass { get; set; }
    public string? PersonBirthYear { get; set; }
    public string? PersonBirthPlace { get; set; }
    public string? PersonAddress { get; set; }
    public string? PersonCompany { get; set; }
    public string? PersonSign { get; set; }
    public List<ExportOrderFileDto> ExportOrders { get; set; } = new();
}
