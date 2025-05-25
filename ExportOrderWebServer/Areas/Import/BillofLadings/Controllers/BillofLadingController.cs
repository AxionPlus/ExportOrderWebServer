using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExportOrderWebServer.Areas.Import.BillofLadings.Provider;
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

        private AppObjectResponse appObjResponse = new();

        public BillofLadingController(IDbContextFactory<ApplicationDbContext> dbContext,
            IBillofLadingProvider billofLadingProvider,
            IExcelFileUploadService excelService)
        {
            _dbContext = dbContext;
            _excelService = excelService;
            _billofLadingProvider = billofLadingProvider;
        }

        [HttpPost]
        [Route("UploadImportManifest")]
        public async Task UploadImportManifest([FromForm] string userName, IFormFile file) // <AppObjectResponse>
        {
            var UserName = JsonConvert.DeserializeObject<string>(userName);
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

            await _billofLadingProvider.AddRecords(records, UserName);






        }

    }
}
