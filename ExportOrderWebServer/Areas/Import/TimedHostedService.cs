using ExportOrderWebServer.Areas.Import.ReleaseRecords.Services;

namespace ExportOrderWebServer.Areas.Import;
public class TimedHostedService : IHostedService, IDisposable
{
    private TimeSpan loopTime = TimeSpan.FromSeconds(20);
    private TimeSpan loopTime_2 = TimeSpan.FromSeconds(30);
    private TimeSpan loopTime_3 = TimeSpan.FromSeconds(250);
    private TimeSpan loopTime_4 = TimeSpan.FromSeconds(300);
    private int executionCount = 0;
    private readonly ILogger<TimedHostedService> _logger;
    private readonly IReleaseService _releaseImportService;
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer? _timer = null;
    private Timer? _timer_2 = null;
    private Timer? _timer_3 = null;
    private Timer? _timer_4 = null;

    public TimedHostedService(ILogger<TimedHostedService> logger, IReleaseService releaseImportService, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _releaseImportService = releaseImportService;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Timed Hosted Service running. {DateTime.Now}");

         _timer = new Timer(CheckEmailBoxReleaseImport, null, TimeSpan.FromSeconds(5), loopTime);
        _timer_2 = new Timer(SendEmailReleaseImportXml, null, TimeSpan.FromSeconds(10), loopTime_2);
        //  _timer_3 = new Timer(SendEmailReleaseImportConfirmationToCustomer, null, TimeSpan.FromSeconds(15), loopTime_3);

        return Task.CompletedTask;
    }

    private async void CheckEmailBoxReleaseImport(object? state)
    {
        while (StaticParam.IMAP_BUSY)
            await Task.Delay(TimeSpan.FromSeconds(5));
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            StaticParam.IMAP_BUSY = true;
            // Use dbContext here
            await _releaseImportService.CheckEmailBoxReleaseImportAsync(dbContext);
            _logger.LogInformation($"complete execute CheckEmailBoxReleaseImportAsync {DateTime.Now}");
            Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false).GetAwaiter().GetResult();
            await _releaseImportService.CheckReleaseImportResponseAsync(dbContext);
            _logger.LogInformation($"complete execute CheckReleaseImportResponseAsync {DateTime.Now}");
            StaticParam.IMAP_BUSY = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    private async void SendEmailReleaseImportXml(object? state)
    {

        while (StaticParam.SMTP_BUSY)
            await Task.Delay(TimeSpan.FromSeconds(5));
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Use dbContext here

            StaticParam.SMTP_BUSY = true;
            await _releaseImportService.SendEmailReleaseImportXmlAsync(dbContext);
            _logger.LogInformation($"complete execute SendEmailReleaseImportXmlAsync {DateTime.Now}");
            StaticParam.SMTP_BUSY = false;
        }
        catch (Exception ex)
        {
            StaticParam.SMTP_BUSY = false;
            Console.WriteLine(ex.Message);
        }
    }
    private async void SendEmailReleaseImportConfirmationToCustomer(object? state)
    {

        while (StaticParam.SMTP_BUSY)
            await Task.Delay(TimeSpan.FromSeconds(5));


        StaticParam.SMTP_BUSY = true;
        await _releaseImportService.SendEmailReleaseImportConfirmationToCustomerAsync();
        _logger.LogInformation($"complete execute SendEmailReleaseImportConfirmationToCustomerAsync {DateTime.Now}");
        StaticParam.SMTP_BUSY = false;

    }


    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Timed Hosted Service is stopping. {DateTime.Now}");

        _timer?.Change(Timeout.Infinite, 0);
        _timer_2?.Change(Timeout.Infinite, 0);
        _timer_3?.Change(Timeout.Infinite, 0);
        _timer_4?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _timer_2?.Dispose();
        _timer_3?.Dispose();
        _timer_4?.Dispose();
    }
}
public static class StaticParam
{
    public static bool SMTP_BUSY { get; set; } = false;
    public static bool IMAP_BUSY { get; set; } = false;

}