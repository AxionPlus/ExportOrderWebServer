using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Web;
using YandexDisk.Client;
using YandexDisk.Client.Clients;
using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;

namespace ExportOrderWebServer.Areas.ImportDocument.Services;

public class YandexDiskSettings
{
    public string ClientId { get; set; } = "ae152a73b2b141b68603fc6e6ac96af2";
    public string ClientSecret { get; set; } = "0cf848a139ff4fa2854f3eccb5848f43";
    public string RefreshToken { get; set; } = string.Empty; // Для серверного доступа

    //oauth.yandex.ru/authorize?response_type=token&client_id=ae152a73b2b141b68603fc6e6ac96af2

    //y0__xCu1O6OARiyoT4gx4XOxxZk3QWkkr0JDjd1eIKho7U1cJ4uQQ
}

public class YandexFileUploadDto
{
    public string VesselName { get; set; } = string.Empty;
    public string VoyageNo { get; set; } = string.Empty;
    public string FileName { get; set; } = "IFillBill.xlsx";
    // Базовая папка, например: "Документы Импорт"
    public string BaseFolderPath { get; set; } = "Документы Импорт";
}
public interface IYandexDiskService
{
    Task<string> UploadFileAsync(byte[] fileContent, YandexFileUploadDto uploadDto, CancellationToken ct = default);
    Task<string> UploadFileV1Async(byte[] fileContent, YandexFileUploadDto uploadDto, CancellationToken ct = default);

}

public class YandexDiskService : IYandexDiskService
{
    private readonly HttpClient _httpClient;
    private readonly YandexDiskSettings _settings;
    private readonly ILogger<YandexDiskService> _logger;
    private string _accessToken = "y0__xCu1O6OARiyoT4gx4XOxxZk3QWkkr0JDjd1eIKho7U1cJ4uQQ";
    private const string BaseFolderPath = "Документы Импорт";
    private const string TokenUrl = "https://oauth.yandex.ru/token";
    private const string DiskApiUrl = "https://cloud-api.yandex.net/v1/disk/resources";

    public YandexDiskService(
        HttpClient httpClient,
        IOptions<YandexDiskSettings> settings,
        ILogger<YandexDiskService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        //  _accessToken = string.Empty;
    }


    private static string SanitizePathSegment(string segment)
    {
        // Удаляем символы, недопустимые в путях Яндекс.Диска
        var invalid = new string(Path.GetInvalidFileNameChars());
        foreach (var c in invalid)
            segment = segment.Replace(c.ToString(), "_");
        return segment.Trim('_', ' ', '/');
    }

    public async Task<string> UploadFileV1Async(byte[] fileContent, YandexFileUploadDto uploadDto, CancellationToken ct = default)
    {
        // Санитизация имени для пути
        var safeVessel = SanitizePathSegment(uploadDto.VesselName);
        var safeVoyage = SanitizePathSegment(uploadDto.VoyageNo);

        // Формируем путь: /Документы Импорт/VesselName_VoyageNo/IFillBill.xlsx
        var folderPath = $"/{BaseFolderPath}/{safeVessel}_{safeVoyage}";
        var fullPath = $"{folderPath}/{uploadDto.FileName}";

        string? tempFile = null;
        try
        {
            IDiskApi diskApi = new DiskHttpApi(_accessToken);
            // 1. Создаём временный файл из byte[]
            tempFile = Path.Combine(Path.GetTempPath(), $"yandex_upload_{Guid.NewGuid():N}.xlsx");
            await File.WriteAllBytesAsync(tempFile, fileContent, ct);

            // 2. Создаём папку на Яндекс.Диске, если не существует

            Resource fooResourceDescription = await diskApi.MetaInfo.GetInfoAsync(new ResourceRequest
            {
                Limit = 10000,
                Path = $"/{BaseFolderPath}", //Folder on Yandex Disk
            }, CancellationToken.None);

            if (fooResourceDescription.Embedded.Items.All(s => s.Name != $"{safeVessel}_{safeVoyage}"))
                await diskApi.Commands.CreateDictionaryAsync(folderPath, ct);

            // 3. Загружаем файл (overwrite=true)

            await diskApi.Files.UploadFileAsync(
                path: fullPath,
                overwrite: true,
                localFile: tempFile,
                cancellationToken: ct);

            _logger.LogInformation("Файл загружен на Яндекс.Диск: {Path}", fullPath);
            return fullPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки файла на Яндекс.Диск. Путь: {Path}", fullPath);
            throw;
        }
        finally
        {
            // 4. Удаляем временный файл
            if (tempFile != null && File.Exists(tempFile))
            {
                try { File.Delete(tempFile); }
                catch { /* Игнорируем ошибки удаления */ }
            }
        }
    }
    public async Task<string> UploadFileAsync(byte[] fileContent, YandexFileUploadDto uploadDto, CancellationToken ct = default)
    {
        await EnsureAuthenticatedAsync(ct);

        // Формируем полный путь: Документы Импорт/VesselName_VoyageNo/IFillBill.xlsx
        var folderPath = $"{uploadDto.BaseFolderPath}/{SanitizeFileName(uploadDto.VesselName)}_{uploadDto.VoyageNo}";
        var fullPath = $"{folderPath}/{uploadDto.FileName}";

        try
        {
            // 1. Создаем папки, если не существуют
            await CreateDirectoryIfNotExistsAsync(folderPath, ct);

            // 2. Получаем ссылку на загрузку
            var uploadLink = await GetUploadLinkAsync(fullPath, ct);

            // 3. Загружаем файл
            using var content = new ByteArrayContent(fileContent);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            var response = await _httpClient.PutAsync(uploadLink, content, ct);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Файл успешно загружен на Яндекс.Диск: {Path}", fullPath);
            return fullPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке файла на Яндекс.Диск. Путь: {Path}", fullPath);
            throw;
        }
    }

    private async Task EnsureAuthenticatedAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            await RefreshAccessTokenAsync(ct);
        }
    }

    private async Task RefreshAccessTokenAsync(CancellationToken ct)
    {
        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", _settings.RefreshToken },
            { "client_id", _settings.ClientId },
            { "client_secret", _settings.ClientSecret }
        };

        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(TokenUrl, content, ct);
        var responseString = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Ошибка авторизации Яндекс: {responseString}");
        }

        // Парсим JSON вручную или через System.Text.Json
        using var doc = System.Text.Json.JsonDocument.Parse(responseString);
        _accessToken = doc.RootElement.GetProperty("access_token").GetString()
            ?? throw new Exception("Не удалось получить access_token");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", _accessToken);
    }

    private async Task CreateDirectoryIfNotExistsAsync(string path, CancellationToken ct)
    {
        var checkResponse = await _httpClient.GetAsync($"{DiskApiUrl}?path={HttpUtility.UrlEncode(path)}", ct);

        if (checkResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            var createContent = new StringContent("{\"force_async\": false}", Encoding.UTF8, "application/json");
            var putResponse = await _httpClient.PutAsync($"{DiskApiUrl}?path={HttpUtility.UrlEncode(path)}", createContent, ct);
            putResponse.EnsureSuccessStatusCode();
        }
        else
        {
            checkResponse.EnsureSuccessStatusCode();
        }
    }

    private async Task<string> GetUploadLinkAsync(string path, CancellationToken ct)
    {
        var url = $"{DiskApiUrl}/upload?path={HttpUtility.UrlEncode(path)}&overwrite=true";
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = System.Text.Json.JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("href").GetString()
            ?? throw new Exception("Не удалось получить ссылку для загрузки");
    }

    private string SanitizeFileName(string name)
    {
        // Удаляем недопустимые символы для имен файлов/папок
        var invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        foreach (var c in invalid)
        {
            name = name.Replace(c.ToString(), "_");
        }
        return name.Trim();
    }
}