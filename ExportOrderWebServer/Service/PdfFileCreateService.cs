using AspNetCore.Reporting;
using System.IO.Compression;

namespace ExportOrderWebServer.Service;

public interface IPdfFileCreateService : IDisposable
{
    Task<byte[]> CreatePdfOrders(List<ExportOrderDTO> items);
    Task<byte[]> CreatePdfBills(List<ExportOrderDTO> items);
}

public class PdfFileCreateService : IPdfFileCreateService
{
    private static readonly string DirTemplate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
    private static readonly string DirTemporaryZipReady = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TempFiles", Path.GetRandomFileName());
    private static readonly string ZipName = $"{DirTemporaryZipReady}.zip";

    public PdfFileCreateService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        if (!Directory.Exists(DirTemporaryZipReady))
            Directory.CreateDirectory(DirTemporaryZipReady);
    }

    public async Task<byte[]> CreatePdfOrders(List<ExportOrderDTO> items)
    {
        if (items is null || !items.Any())
            return Array.Empty<byte>();

        try
        {
            foreach (var item in items)
            {
                /// Data sources
                var Items = new List<ExportOrderDTO>() { item };

                var dsItem = Items.Select(x => new
                {
                    x.Num,
                    x.BLNum,
                    x.Dated,
                    Vessel = string.Concat(x.VesselName, " (", x.VesselFlag, ")"),
                    x.Voyage,
                    x.DateOfLoading,
                    x.POL,
                    x.PODwithCountryRus,
                    x.Contract,
                    x.ContractDate,
                    x.Shippers,
                    x.Consignees,
                    x.Commodities,
                    x.CommodityShort,
                    x.MyCompanyName,
                    x.MyCompanyEmail,
                    x.Person
                });

                var dsRecords = item.ExportOrderRecordsDTO;

                int dSeq = 0;
                var dsDocuments = dsRecords?.GroupBy(r => r.DocumentName).Select(g => new {
                    docSeq = ++dSeq,
                    Document = g.Key,
                    Pakages = g.Sum(q => q.PackageQty),
                    CntrTare = g.Sum(tr => tr.CntrTareWt),
                    docNet = g.Sum(net => net.NetWt),
                    docGross = g.Sum(gr => gr.GrossWt)
                }).ToList();

                /// Report RDLC
                string mimeType = "application/pdf";
                int pageIndex = new Random().Next(1, 101);
                string pathTemplate = Path.Combine(DirTemplate, "ExportOrder.rdlc");

                LocalReport localReport = new(pathTemplate);

                localReport.AddDataSource("dsItem", dsItem);
                localReport.AddDataSource("dsRecords", dsRecords);
                localReport.AddDataSource("dsDocuments", dsDocuments);

                ReportResult result = localReport.Execute(RenderType.Pdf, pageIndex, null, mimeType);

                /// сохраняем Pdf-файл в папку DirTemporaryZipReady
                string pathFile = Path.Combine(DirTemporaryZipReady, $"Order_{item.Num}.pdf");
                using var fileStream = new FileStream(pathFile, FileMode.Create);
                await fileStream.WriteAsync(result.MainStream.AsMemory(0, result.MainStream.Length));
            }

            /// Create ZIP-file for entire collection
            ZipFile.CreateFromDirectory(DirTemporaryZipReady, ZipName);

            using FileStream zipFileStream = new(ZipName, FileMode.Open);
            var buffer = new byte[zipFileStream.Length];
            await zipFileStream.ReadAsync(buffer);
            zipFileStream.Close();

            return buffer;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Array.Empty<byte>();
        }
    }

    public async Task<byte[]> CreatePdfBills(List<ExportOrderDTO> items)
    {
        if (items is null || !items.Any())
            return Array.Empty<byte>();
        
        try
        {
            foreach (var item in items)
            {
                /// Data sources            
                var Items = new List<ExportOrderDTO>() { item };
                var Records = item.ExportOrderRecordsDTO.ToList();

                /// список типов контейнеров
                var CntrTypesGroup = Records.Where(r => r.CntrType is not null).GroupBy(r => r.CntrType)
                                            .Select(g => new { CntrTypes = string.Join(" ", g.ToList().Count, "*", g.Key) })
                                            .ToArray();  //ToList()

                string CntrTypes = string.Join("\n", CntrTypesGroup.Select(ctg => ctg.CntrTypes)).Trim();

                /// общие данные
                var dsItem = Items.Select(x => new
                {
                    x.BLNum,
                    x.BLDate,
                    x.BLDateOEL,
                    x.Shippers,
                    x.Consignees,
                    NotifyParties = x.Consignees,
                    CntrTypes,
                    x.Commodities,
                    POLAgent = x.CarrierNameEn,
                    x.PODAgent,
                    x.VesselName,
                    x.Voyage,
                    x.POLEn,
                    x.PODEn,
                    x.PODwithCountryEn,
                    x.TotalCntrCount,
                    x.TotalPackages,
                    x.TotalTareWeight,
                    x.TotalGrossWeight,
                    x.TotalGrossNTareWeight,
                    x.Measurement,
                }).ToArray();    //ToList()

                /// список контейнеров дополненный до 20ти записей на первом листе коносамента
                if (item.BLtemplate == "nca")
                {
                    int recCount = Records.Count;

                    int upperBound = 20;

                    if (recCount < 20)
                    {
                        foreach (var record in Records)
                        {
                            int extraRowsSeal = 0;
                            int extraRowsCommodity = 0;

                            switch (record.Seal?.Length)
                            {
                                case <= 10:
                                    break;
                                case <= 20:
                                    extraRowsSeal = 1;
                                    break;
                                case <= 30:
                                    extraRowsSeal = 2;
                                    break;
                                case <= 40:
                                    extraRowsSeal = 3;
                                    break;
                                case <= 50:
                                    extraRowsSeal = 4;
                                    break;
                                case <= 60:
                                    extraRowsSeal = 5;
                                    break;
                                case <= 70:
                                    extraRowsSeal = 6;
                                    break;
                                case <= 80:
                                    extraRowsSeal = 7;
                                    break;
                                case <= 90:
                                    extraRowsSeal = 8;
                                    break;
                                case <= 100:
                                    extraRowsSeal = 9;
                                    break;
                                case <= 110:
                                    extraRowsSeal = 10;
                                    break;
                            }

                            switch (record.RecordCommoditiesEn?.Length)
                            {
                                case <= 35:
                                    break;
                                case <= 70:
                                    extraRowsCommodity = 1;
                                    break;
                                case <= 105:
                                    extraRowsCommodity = 2;
                                    break;
                                case <= 140:
                                    extraRowsCommodity = 3;
                                    break;
                                case <= 175:
                                    extraRowsCommodity = 4;
                                    break;
                                case <= 210:
                                    extraRowsCommodity = 5;
                                    break;
                                case <= 245:
                                    extraRowsCommodity = 6;
                                    break;
                                case <= 280:
                                    extraRowsCommodity = 7;
                                    break;
                                case <= 315:
                                    extraRowsCommodity = 8;
                                    break;
                                case <= 350:
                                    extraRowsCommodity = 9;
                                    break;
                                case <= 385:
                                    extraRowsCommodity = 10;
                                    break;
                            }

                            if (extraRowsSeal >= extraRowsCommodity)
                                upperBound -= extraRowsSeal;
                            else
                                upperBound -= extraRowsCommodity;
                        }
                    }

                    if (upperBound > 0)
                        for (int i = recCount + 1; i <= upperBound; i++)
                            Records.Add(new() { Seq = (uint)i, CntrTareWt = null, GrossWt = null });
                }

                /// Report RDLC
                string mimeType = "application/pdf";
                int pageIndex = new Random().Next(1, 101);
                string pathTemplate = Path.Combine(DirTemplate, $"BL{item.BLtemplate}.rdlc");

                LocalReport localReport = new(pathTemplate);

                localReport.AddDataSource("dsBL", dsItem);
                localReport.AddDataSource("dsCntrRecords", Records);

                ReportResult result = localReport.Execute(RenderType.Pdf, pageIndex, null, mimeType);

                /// сохраняем Pdf-файл в папку DirTemporaryZipReady
                string pathFile = Path.Combine(DirTemporaryZipReady, $"BL_{item.Num}.pdf");
                using FileStream fileStream = new(pathFile, FileMode.Create);
                await fileStream.WriteAsync(result.MainStream.AsMemory(0, result.MainStream.Length));   // (result.MainStream, 0, result.MainStream.Length);
            }

            /// Create ZIP-file for entire collection
            ZipFile.CreateFromDirectory(DirTemporaryZipReady, ZipName);

            using FileStream zipFileStream = new(ZipName, FileMode.Open);
            var buffer = new byte[zipFileStream.Length];
            await zipFileStream.ReadAsync(buffer);
            zipFileStream.Close();

            return buffer;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Array.Empty<byte>();
        }        
    }    

    public void Dispose()
    {
        if (Directory.Exists(DirTemporaryZipReady))
            Directory.Delete(DirTemporaryZipReady, true);

        if (File.Exists(ZipName))
            File.Delete(ZipName);
    }
}