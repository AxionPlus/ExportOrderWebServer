
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
    public string CaptainName { get; set; } = string.Empty;
    public string CaptainLastName { get; set; } = string.Empty;
    public DateTime? TranslateUpdateTime { get; set; }
    public Guid TerminalId { get; set; }
    public TerminalDto Terminal { get; set; }

    public DateTime? ETA { get; set; }
    public DateTime? ETS { get; set; }

    public Guid PortOfLoadingId { get; set; }
    public PortDto PortOfLoading { get; set; }
    public DateTime? PortOfLoadingDate { get; set; }

   

    public List<BillOfLadingBaseDto> BillOfLadings { get; set; } = new();

    public string FeederBlNo { get; set; } = "ALPHA";

    [NotMapped]
    public string VesselCallDisplay => $"{Vessel?.Name} - {VoyageNo} ({ETA:dd.MM.yyyy})";

    [NotMapped]
    public string FullInfo => $"{Vessel?.Name} {VoyageNo} → {Terminal?.Name} ETA: {ETA:dd.MM.yyyy HH:mm}";

    public bool IsNew => Timestamp == 0;




    public int QuantityEmpty20 => BillOfLadings.SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).Select(s => s.First()).Where(s => s.ContainerTypeId.Contains("20")).Count(s => s.FullOrEmpty != "F");
    public int QuantityEmpty40 => BillOfLadings.SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).Select(s => s.First()).Where(s => s.ContainerTypeId.Contains("40")).Count(s => s.FullOrEmpty != "F");
    public int QuantityFull20 => BillOfLadings.SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).Select(s => s.First()).Where(s => s.ContainerTypeId.Contains("20")).Count(s => s.FullOrEmpty == "F");
    public int QuantityFull40 => BillOfLadings.SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).Select(s => s.First()).Where(s => s.ContainerTypeId.Contains("40")).Count(s => s.FullOrEmpty == "F");

    public int QuantityImo20 => BillOfLadings.Where(s=>s.IsImo).SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).Select(s => s.First()).Where(s => s.ContainerTypeId.Contains("20")).Count(s => s.FullOrEmpty == "F");
    public int QuantityImo40 => BillOfLadings.Where(s=>s.IsImo).SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).Select(s => s.First()).Where(s => s.ContainerTypeId.Contains("40")).Count(s => s.FullOrEmpty == "F");



    public int QuantityContainers => BillOfLadings.SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo)
        .Select(s => s.First()).Count();


    public int TareWtEmpty20 => BillOfLadings.SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).Select(s => s.First()).Where(s => s.ContainerTypeId.Contains("20")).Where(s => s.FullOrEmpty != "F").Sum(s => s.TareWt);
    public int TareWtEmpty40 => BillOfLadings.SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).Select(s => s.First()).Where(s => s.ContainerTypeId.Contains("40")).Where(s => s.FullOrEmpty != "F").Sum(s => s.TareWt);
    public int TareWtFull20 => BillOfLadings.SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).Select(s => s.First()).Where(s =>  s.ContainerTypeId.Contains("20")).Where(s => s.FullOrEmpty == "F").Sum(s => s.TareWt);
    public int TareWtFull40 => BillOfLadings.SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).Select(s => s.First()).Where(s =>  s.ContainerTypeId.Contains("40")).Where(s => s.FullOrEmpty == "F").Sum(s => s.TareWt);

    public int TareWtImo20 => BillOfLadings.Where(s=>s.IsImo).SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).Select(s => s.First()).Where(s => s.ContainerTypeId.Contains("20")).Where(s => s.FullOrEmpty == "F").Sum(s => s.TareWt);
    public int TareWtImo40 => BillOfLadings.Where(s => s.IsImo).SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).Select(s => s.First()).Where(s => s.ContainerTypeId.Contains("40")).Where(s => s.FullOrEmpty == "F").Sum(s => s.TareWt);

    public int TareWtContainers =>   BillOfLadings.SelectMany(s => s.ContainerRecords).GroupBy(s => s.ContainerNo).Select(s => s.First()).Sum(s => s.TareWt);



    public double GrossWtFull20 => BillOfLadings.SelectMany(s => s.ContainerRecords).Where(s => s.ContainerTypeId.Contains("20")).Where(s => s.FullOrEmpty == "F").Sum(s => s.GrossWeight);
    public double GrossWtFull40 => BillOfLadings.SelectMany(s => s.ContainerRecords).Where(s => s.ContainerTypeId.Contains("40")).Where(s => s.FullOrEmpty == "F").Sum(s => s.GrossWeight);

    public double GrossWtImo20 => BillOfLadings.Where(s=>s.IsImo).SelectMany(s => s.ContainerRecords).Where(s => s.ContainerTypeId.Contains("20")).Where(s => s.FullOrEmpty == "F").Sum(s => s.GrossWeight);
    public double GrossWtImo40 => BillOfLadings.Where(s => s.IsImo).SelectMany(s => s.ContainerRecords).Where(s => s.ContainerTypeId.Contains("40")).Where(s => s.FullOrEmpty == "F").Sum(s => s.GrossWeight);

    public double GrossWtFull => BillOfLadings.SelectMany(s => s.ContainerRecords).Where(s => s.FullOrEmpty == "F").Sum(s => s.GrossWeight);



}

