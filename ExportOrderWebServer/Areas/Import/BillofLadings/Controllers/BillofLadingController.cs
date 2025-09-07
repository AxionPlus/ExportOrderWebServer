using ExportOrderWebServer.Areas.Import.BillofLadings.Provider;
using ExportOrderWebServer.Areas.Import.ImportManifest.Provider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ExportOrderWebServer.Areas.Import.BillofLadings.Controllers
{
    [AllowAnonymous]
    [Route("file/[controller]")]
    [ApiController]
    public class BillofLadingController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContext;
        private readonly IExcelFileUploadService _excelService;
        private readonly IBillofLadingProvider _billofLadingProvider;
        private readonly IImportManifestProvider _importManifestProvider;

        private AppObjectResponse appObjResponse = new();

        public BillofLadingController(IDbContextFactory<ApplicationDbContext> dbContext,
            IBillofLadingProvider billofLadingProvider,
            IExcelFileUploadService excelService, IImportManifestProvider importManifestProvider )
        {
            _dbContext = dbContext;
            _excelService = excelService;
            _billofLadingProvider = billofLadingProvider;
            _importManifestProvider = importManifestProvider;
        }

        [HttpPost]
        [Route("UploadImportManifest")]
        public async Task UploadImportManifest([FromForm] string userName, [FromForm] string vesselCallDetailId, IFormFile file) // <AppObjectResponse>
        {
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "TempFiles");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            string savePath = Path.Combine(dirPath, fileName);


            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                Console.WriteLine("file.CopyTo(stream)");
                file.CopyTo(stream);
            }

            var records = _excelService.ReadExcelFileImportManifest(savePath);
            if (records.Count() == 0) return;

            await _billofLadingProvider.AddRecords(records, vesselCallDetailId, userName);

        }

        [HttpPost]
        [Route("UploadXlsFile")]
        public async Task UploadXlsFile([FromForm] string userName, IFormFile file) // <AppObjectResponse>
        {
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "TempFiles");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            string savePath = Path.Combine(dirPath, fileName);


            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                Console.WriteLine("file.CopyTo(stream)");
                file.CopyTo(stream);
            }

            var records = _excelService.ReadExcelFile(savePath);
            if (records.Count() == 0) return;

            await _billofLadingProvider.UpdateRecordsFromXls(records,  userName);

        }


        [HttpGet, Route("ManifestXmlNle/{id}")]     // file/BillofLading/ManifestXmlNle
        public async Task<IActionResult> ManifestXmlNle(string id)
        {
         
            var item = await _importManifestProvider.GetVesselCallData(id);   //var item = await _exportOrderProvider.GetExportOrderDTOAsync(Id);

            if (item == null) return BadRequest("Данные не получены.");


            using IXmlFileCreateService _xmlFileService = new XmlFileCreateService();
            var buffer = await _xmlFileService.CreateXMLfileManifestNle(item);

            if (buffer == Array.Empty<byte>())
                return BadRequest("Файл не записан.");
            else
                return File(buffer, "application/xml", $"{item.VesselName}_{item.VesselVoyage}.xml");
        }

    }
}
