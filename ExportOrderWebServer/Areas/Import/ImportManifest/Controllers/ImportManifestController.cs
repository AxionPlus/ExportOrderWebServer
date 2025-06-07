using ExportOrderWebServer.Areas.Import.BillofLadings.Provider;
using ExportOrderWebServer.Areas.Import.ImportManifest.Provider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ExportOrderWebServer.Areas.Import.ImportManifest.Controllers
{
    [AllowAnonymous]
    [Route("file/[controller]")]
    [ApiController]
    public class ImportManifestController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContext;
        private readonly IExcelFileCreateService _excelService;
        private readonly IImportManifestProvider _importManifestProvider;

        private AppObjectResponse appObjResponse = new();

        public ImportManifestController(IDbContextFactory<ApplicationDbContext> dbContext,
            IImportManifestProvider importManifestProvider,
            IExcelFileCreateService excelService)
        {
            _dbContext = dbContext;
            _excelService = excelService;
            _importManifestProvider = importManifestProvider;
        }





        [HttpPost, Route("DownLoadTemplate1C")]
        public async Task<IActionResult> DownLoadTemplate1C([FromBody] object obj)
        {

            var vesselCall = JsonConvert.DeserializeObject<VesselCallDetailDTO>(obj.ToString());


            if (vesselCall is null )
                return BadRequest("List of selected records is empty.");

            var BillOfLadings = await _importManifestProvider.GetBillOfLadingsAsync(vesselCall);

            if (BillOfLadings is null || !BillOfLadings.Any())
                return BadRequest("Records not found");

            var buffer = await _excelService.CreateExcelTemplate1C(vesselCall, BillOfLadings);

            if (buffer == Array.Empty<byte>())
                return BadRequest("Файл не записан.");
            else
                return File(buffer, "application/zip");
        }
    }
}
