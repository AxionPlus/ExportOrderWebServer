using AspNetCore.Reporting;

namespace ExportOrderWebServer.Service;

public class SavePdfFileService : IDisposable
{
    private readonly IWebHostEnvironment webHostEnvironment;
    private string DirPath { get; set; }
    private readonly IEnumerable<VoyageExportOrderDTO> Items;

    public SavePdfFileService(IWebHostEnvironment _webHostEnvironment, string _DirPath, IEnumerable<VoyageExportOrderDTO> _Items)
    {
        webHostEnvironment = _webHostEnvironment;
        DirPath = _DirPath;
        Items = _Items;
    }

    public async Task<Dictionary<string, byte[]>> CreatePDFfile()
    {
        Dictionary<string, byte[]> Files = new Dictionary<string, byte[]>();

        //var tempFiles = new List<string>();

        foreach (var item in Items)
        {
            string mimeType = "";
            int pageIndex = new Random().Next(1, 101);
            string pathReport = Path.Combine(webHostEnvironment.ContentRootPath, "Reports", "ExportOrder.rdlc");
            string pathFile = Path.Combine(DirPath, $"{item.Num}.pdf");

            LocalReport localReport = new LocalReport(pathReport);
            localReport.AddDataSource("dsExportOrders", item);

            ReportResult result = localReport.Execute(RenderType.Pdf, pageIndex, null, mimeType);

            //// сохраняем файл в папку DirPath
            //using (var fileStream = new FileStream(pathFile, FileMode.Create))
            //{

            //    var file = File.WriteAllBytesAsync(pathFile, result.MainStream);
                
            //    await file.CopyToAsync(fileStream);
            //}

            await File.WriteAllBytesAsync(pathFile, result.MainStream);

            Files.Add(pathFile, result.MainStream);
        }

        return Files;
        /// Read whole folder and stream it ot zip file


    }


    public void Dispose()
    {
        try
        {
            if (File.Exists(DirPath))
                File.Delete(DirPath);
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }
}
