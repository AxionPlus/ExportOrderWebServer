using Blazored.LocalStorage;
using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer;
using ExportOrderWebServer.Areas.Catalog;
using ExportOrderWebServer.Areas.Import.BillofLadings.Provider;
using ExportOrderWebServer.Areas.Import.ImportManifest.Provider;
using ExportOrderWebServer.Areas.Import.ReleaseRecords.Provider;
using ExportOrderWebServer.Areas.Import.ReleaseRecords.Services;
using ExportOrderWebServer.Areas.Import.VesselCall.Provider;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Provider;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Services;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Validator;
using ExportOrderWebServer.Areas.ImportDocument.Customer.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Customer.Provider;
using ExportOrderWebServer.Areas.ImportDocument.Customer.Services;
using ExportOrderWebServer.Areas.ImportDocument.Customer.Validator;
using ExportOrderWebServer.Areas.ImportDocument.Extensions;
using ExportOrderWebServer.Areas.ImportDocument.Port.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Port.Provider;
using ExportOrderWebServer.Areas.ImportDocument.Port.Services;
using ExportOrderWebServer.Areas.ImportDocument.Port.Validator;
using ExportOrderWebServer.Areas.ImportDocument.Provider;
using ExportOrderWebServer.Areas.ImportDocument.Repositories;
using ExportOrderWebServer.Areas.ImportDocument.Services;
using ExportOrderWebServer.Areas.ImportDocument.Terminal.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Terminal.Provider;
using ExportOrderWebServer.Areas.ImportDocument.Terminal.Services;
using ExportOrderWebServer.Areas.ImportDocument.Terminal.Validator;
using ExportOrderWebServer.Areas.ImportDocument.Vessel.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Vessel.Provider;
using ExportOrderWebServer.Areas.ImportDocument.Vessel.Services;
using ExportOrderWebServer.Areas.ImportDocument.Vessel.Validator;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Provider;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Services;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Validator;
using ExportOrderWebServer.Service.FileService;
using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
// Регистрируем провайдер кодировок (добавьте в самое начало)
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
IServiceCollection services = builder.Services;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();


// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dataSource, optionsBuilder =>
        optionsBuilder.MigrationsAssembly("ExportOrderWebServer")
    ));

services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseNpgsql(dataSource);
},
ServiceLifetime.Transient);

services.AddDatabaseDeveloperPageExceptionFilter();

services.AddIdentity<ApplicationUser, ApplicationRole>(options => options.SignIn.RequireConfirmedAccount = false)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddDefaultUI();

services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
});

services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "AspNetCore.Identity.Application.ExportOrderWebServer";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});
services.AddBlazoredLocalStorage();
services.AddEndpointsApiExplorer();
services.AddHttpContextAccessor();

services.AddScoped<TokenProvider>();
services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
services.AddAuthentication().AddCookie(cfg => cfg.SlidingExpiration = true).AddJwtBearer(x =>
{
    // options
});
#if !DEBUG
services.AddHostedService<TimedHostedService>();
#endif
services.AddRazorPages();
services.AddServerSideBlazor();
services.AddDatabaseDeveloperPageExceptionFilter();
services.AddHttpClient();
services.AddMudServices();
services.AddControllers();

/// PROVIDERS
services.AddTransient<IUserProvider, UserProvider>();
services.AddTransient<ICarrierProvider, CarrierProvider>();
services.AddTransient<ICommodityProvider, CommodityProvider>();
services.AddTransient<ICountryProvider, CountryProvider>();
services.AddTransient<ICustomerProvider, CustomerProvider>();
services.AddTransient<ICustomsProvider, CustomsProvider>();
services.AddTransient<ILocationProvider, LocationProvider>();
services.AddTransient<ITerminalProvider, TerminalProvider>();
services.AddTransient<IVesselProvider, VesselProvider>();
services.AddTransient<IImportVesselCallProvider, ImportVesselCallProvider>();
services.AddTransient<IVesselCallProvider, VesselCallProvider>();
services.AddTransient<ICntrTypeProvider, CntrTypeProvider>();
services.AddTransient<IDocumentProvider, DocumentProvider>();
services.AddTransient<IExportOrderProvider, ExportOrderProvider>();         // DELETE
services.AddTransient<IExportOrderProvider_New, ExportOrderProvider_New>();
services.AddTransient<IMyCompanyProvider, MyCompanyProvider>();
services.AddTransient<ISupplementaryUnitProvider, SupplementaryUnitProvider>();
services.AddTransient<IBillofLadingProvider, BillofLadingProvider>();
services.AddTransient<IReleaseImportProvider, ReleaseImportProvider>();
services.AddTransient<ICatalogProvider, CatalogProvider>();
services.AddTransient<IStatisticProvider, StatisticProvider>();
services.AddTransient<IReleaseService, ReleaseService>();
services.AddTransient<IImportManifestProvider, ImportManifestProvider>();

// SERVICES
services.AddTransient<IHttpCustomMethods, HttpCustomMethods>();
services.AddTransient<IUploadResultService, UploadResultService>();         // DELETE
services.AddTransient<IUploadFileService, UploadFileService>();
services.AddTransient<IValidationService, ValidationService>();             // DELETE
services.AddTransient<IValidationService_New, ValidationService_New>();


// FILE SERVICES
services.AddTransient<IExcelFileCreateService, ExcelFileCreateService>();
services.AddTransient<IExcelFileUploadService, ExcelFileUploadService>();
services.AddTransient<IXmlFileCreateService, XmlFileCreateService>();
services.AddTransient<IXmlFileReadService, XmlFileReadService>();
services.AddTransient<IPdfFileCreateService, PdfFileCreateService>();
services.AddTransient<IReleaseService, ReleaseService>();

//Import Document
// Регистрация репозиториев
builder.Services.AddTransient(typeof(IRepository<>), typeof(GenericRepository<>));
//builder.Services.AddScoped<ICrudProvider<BookingBaseEntity>, BookingCrudProvider>();
builder.Services.AddTransient<ICrudProvider<VesselBaseEntity>, VesselCrudProvider>();
builder.Services.AddTransient<ICrudProvider<PortBaseEntity>, PortCrudProvider>();
builder.Services.AddTransient<ICrudProvider<CustomerBaseEntity>, CustomerCrudProvider>();
builder.Services.AddTransient<ICrudProvider<VesselCallBaseEntity>, VesselCallCrudProvider>();
builder.Services.AddTransient<ICrudProvider<BillOfLadingBaseEntity>, BillOfLadingCrudProvider>();
//builder.Services.AddScoped<ICrudProvider<BillOfLadingContainerRecordBaseEntity>, ContainerRecordCrudProvider>();

builder.Services.AddTransient<IVesselCallService, VesselCallService>();
builder.Services.AddTransient<IValidator<VesselCallDto>, VesselCallDtoValidator>();
builder.Services.AddTransient<ICrudProvider<VesselCallBaseEntity>, VesselCallCrudProvider>();

builder.Services.AddTransient<IVesselService, VesselService>();
builder.Services.AddTransient<IValidator<VesselDto>, VesselDtoValidator>();
builder.Services.AddTransient<ICrudProvider<VesselBaseEntity>, VesselCrudProvider>();

builder.Services.AddTransient<ITerminalService, TerminalService>();
builder.Services.AddTransient<IValidator<TerminalDto>, TerminalDtoValidator>();
builder.Services.AddTransient<ICrudProvider<TerminalBaseEntity>, TerminalCrudProvider>();

builder.Services.AddTransient<IPortService, PortService>();
builder.Services.AddTransient<IValidator<PortDto>, PortDtoValidator>();
builder.Services.AddTransient<ICrudProvider<PortBaseEntity>, PortCrudProvider>();

builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IValidator<CustomerDto>, CustomerDtoValidator>();
builder.Services.AddTransient<ICrudProvider<CustomerBaseEntity>, CustomerCrudProvider>();


builder.Services.AddTransient<IBillOfLadingService, BillOfLadingService>();
builder.Services.AddTransient<IVesselCallService, VesselCallService>();

// Добавить валидаторы
builder.Services.AddTransient<IValidator<BillOfLadingBaseDto>, BillOfLadingDtoValidator>();
builder.Services.AddTransient<IValidator<BillOfLadingContainerRecordBaseDto>, BillOfLadingContainerRecordDtoValidator>();


builder.Services.AddScoped<IGoogleSheetsParser, GoogleSheetsParser>();
builder.Services.AddScoped<IExcelExportService, ClosedXmlExcelExportService>();
builder.Services.AddScoped<IArrivalNoticeExportService, ArrivalNoticeExportService>();
builder.Services.AddScoped<IManifestExportService, ManifestExportService>();
builder.Services.AddScoped<IManifestHazardousExportService, ManifestHazardousExportService>();
builder.Services.AddScoped<IObligationExcelService, ObligationExcelService>();
builder.Services.AddScoped<IFeederBlExcelService, FeederBlExcelService>();
builder.Services.AddScoped<IFillBillExcelService, FillBillExcelService>();

builder.Services.AddScoped<IManifestXmlGenerator, ManifestXmlGenerator>();

// Добавление сервиса с настройками
builder.Services.AddXMLParserService(config =>
{
    config.IgnoreParseErrors = true;
    config.LogToConsole = true;
    config.LogToFile = true;
    config.LogFilePath = "logs/parser.log";
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls(new[] { "http://0.0.0.0:7070" });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin  
                .AllowCredentials());               // allow credentials 

app.Run();