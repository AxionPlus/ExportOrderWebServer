using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Services;
using ExportOrderWebServer.Areas.ImportDocument.Port.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Services;
using ExportOrderWebServer.Areas.ImportDocument.Terminal.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Vessel.Dto;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Controller;

[ApiController]
[Route("api/[controller]")]
public class ManifestController : ControllerBase
{
    private readonly IXMLParserService _parserService;
    private readonly IBillOfLadingService _billOfLadingService;
    private readonly IVesselCallService _vesselCallService;
    private readonly ILogger<ManifestController> _logger;

    public ManifestController(
        IXMLParserService parserService,
        IBillOfLadingService billOfLadingService,
        IVesselCallService vesselCallService,
        ILogger<ManifestController> logger)
    {
        _parserService = parserService;
        _billOfLadingService = billOfLadingService;
        _vesselCallService = vesselCallService;
        _logger = logger;
    }

    [HttpPost("parse")]
    public async Task<IActionResult> ParseManifest(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Success = false, Error = "No file uploaded" });

            using var reader = new StreamReader(file.OpenReadStream());
            var xmlContent = await reader.ReadToEndAsync();

            var bills = _parserService.ParseManifest(xmlContent);

            return Ok(new
            {
                Success = true,
                BillsCount = bills.Count,
                ContainersCount = bills.Sum(b => b.TotalContainers),
                Bills = bills
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing manifest");
            return StatusCode(500, new { Success = false, Error = $"Error parsing manifest: {ex.Message}" });
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadManifest([FromForm] UploadManifestRequest request)
    {
        var response = new UploadManifestResponse();

        try
        {
            if (request.Files == null || request.Files.Count == 0)
            {
                response.Errors.Add("No files uploaded");
                return BadRequest(response);
            }

            foreach (var file in request.Files)
            {
                if (file.Length == 0)
                {
                    response.Warnings.Add($"File '{file.FileName}' is empty");
                    continue;
                }

                try
                {
                    response.ProcessedFiles++;

                    using var reader = new StreamReader(file.OpenReadStream());
                    var xmlContent = await reader.ReadToEndAsync();

                    // Парсим XML
                    var parsedBills = _parserService.ParseManifest(xmlContent);
                    response.ParsedBills += parsedBills.Count;

                    // Обрабатываем каждый коносамент
                    foreach (var billDto in parsedBills)
                    {
                        try
                        {
                            // Устанавливаем VesselCallId если он не указан
                            if (billDto.VesselCallId == Guid.Empty && request.VesselCallId.HasValue)
                            {
                                billDto.VesselCallId = request.VesselCallId.Value;
                            }

                            // Проверяем уникальность номера BL
                            var exists = await _billOfLadingService.CheckBillNumberExistsAsync(billDto.Num);
                            if (exists)
                            {
                                response.SkippedBills++;
                                response.Warnings.Add($"Bill of lading '{billDto.Num}' already exists - skipped");
                                continue;
                            }

                            // Создаем коносамент
                            var createdBill = await _billOfLadingService.CreateAsync(billDto);
                            response.SavedBills++;
                            response.Bills.Add(createdBill);

                            _logger.LogInformation($"Successfully saved bill of lading: {billDto.Num}");
                        }
                        catch (Exception ex)
                        {
                            response.Errors.Add($"Error processing bill '{billDto.Num}': {ex.Message}");
                            _logger.LogError(ex, $"Error processing bill of lading: {billDto.Num}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.Errors.Add($"Error parsing file '{file.FileName}': {ex.Message}");
                    _logger.LogError(ex, $"Error parsing manifest file: {file.FileName}");
                }
            }

            response.Success = response.Errors.Count == 0;
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UploadManifest");
            response.Errors.Add($"System error: {ex.Message}");
            return StatusCode(500, response);
        }
    }

    [HttpPost("upload-bulk")]
    public async Task<IActionResult> UploadBulkManifests([Required] Guid vesselCallId, List<IFormFile> files)
    {
        var response = new UploadManifestResponse();

        try
        {
            if (files == null || !files.Any())
                return BadRequest(new { Success = false, Error = "No files uploaded" });

            // Проверяем существование судозахода
            try
            {
                await _vesselCallService.GetByIdAsync(vesselCallId);
            }
            catch (KeyNotFoundException)
            {
                return BadRequest(new { Success = false, Error = $"Vessel call with ID {vesselCallId} not found" });
            }

            foreach (var file in files)
            {
                try
                {
                    response.ProcessedFiles++;

                    using var reader = new StreamReader(file.OpenReadStream());
                    var xmlContent = await reader.ReadToEndAsync();

                    var parsedBills = _parserService.ParseManifest(xmlContent);
                    response.ParsedBills += parsedBills.Count;

                    foreach (var billDto in parsedBills)
                    {
                        billDto.VesselCallId = vesselCallId;

                        try
                        {
                            var exists = await _billOfLadingService.CheckBillNumberExistsAsync(billDto.Num);
                            if (exists)
                            {
                                response.SkippedBills++;
                                continue;
                            }

                            var createdBill = await _billOfLadingService.CreateAsync(billDto);
                            response.SavedBills++;
                            response.Bills.Add(createdBill);
                        }
                        catch (Exception ex)
                        {
                            response.Errors.Add($"Error saving bill '{billDto.Num}': {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.Errors.Add($"Error processing file '{file.FileName}': {ex.Message}");
                }
            }

            response.Success = response.Errors.Count == 0;
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UploadBulkManifests");
            return StatusCode(500, new { Success = false, Error = ex.Message });
        }
    }

    [HttpPost("parse-directory")]
    public IActionResult ParseDirectory([FromBody] ParseDirectoryRequest request)
    {
        try
        {
            var bills = _parserService.ParseAllManifestsInDirectory(request.DirectoryPath);
            var stats = _parserService.GetParserStatistics();

            return Ok(new
            {
                Success = true,
                Directory = request.DirectoryPath,
                Statistics = stats,
                Bills = bills
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing directory");
            return StatusCode(500, $"Error parsing directory: {ex.Message}");
        }
    }

    [HttpGet("statistics")]
    public IActionResult GetStatistics()
    {
        var stats = _parserService.GetParserStatistics();
        return Ok(stats);
    }

}

public class ParseDirectoryRequest
{
    public string DirectoryPath { get; set; }
}