using ExportOrderWebServer;
using ExportOrderWebServer.Areas.Catalog;
using ExportOrderWebServer.Areas.Import;
using ExportOrderWebServer.Areas.Import.BillofLadings.Provider;
using ExportOrderWebServer.Areas.Import.ImportManifest.Provider;
using ExportOrderWebServer.Areas.Import.ReleaseRecords.Provider;
using ExportOrderWebServer.Areas.Import.ReleaseRecords.Services;
using ExportOrderWebServer.Areas.Import.VesselCall.Provider;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);
IServiceCollection services = builder.Services;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add services to the container.
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString,
                    optionsBuilder => optionsBuilder.MigrationsAssembly("ExportOrderWebServer"));
},
ServiceLifetime.Transient);

services.AddDbContextFactory<ApplicationDbContext>(options =>
{ 
    options.UseNpgsql(connectionString);
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

services.AddEndpointsApiExplorer();

services.AddScoped<TokenProvider>();
services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
services.AddAuthentication().AddCookie(cfg => cfg.SlidingExpiration = true).AddJwtBearer(x =>
{
    // options
});
//builder.Services.AddAuthentication("tris.Identity").AddCookie();
//services.AddHostedService<TimedHostedService>();
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
services.AddTransient<IExportOrderProvider, ExportOrderProvider>();
services.AddTransient<IMyCompanyProvider, MyCompanyProvider>();
services.AddTransient<ISupplementaryUnitProvider, SupplementaryUnitProvider>();
services.AddTransient<IBillofLadingProvider, BillofLadingProvider>();
services.AddTransient<IReleaseImportProvider, ReleaseImportProvider>();
services.AddTransient<ICatalogProvider, CatalogProvider>();
services.AddTransient<IStatisticProvider, StatisticProvider>();
services.AddTransient<IReleaseService, ReleaseService>();
services.AddTransient<IImportManifestProvider, ImportManifestProvider>();

/// SERVICES
services.AddTransient<IHttpCustomMethods, HttpCustomMethods>();
services.AddTransient<IUploadResultService, UploadResultService>();
services.AddTransient<IValidationService, ValidationService>();

/// FILE SERVICES
services.AddTransient<IExcelFileCreateService, ExcelFileCreateService>(); 
services.AddTransient<IExcelFileUploadService, ExcelFileUploadService>();
services.AddTransient<IXmlFileCreateService, XmlFileCreateService>();
services.AddTransient<IXmlFileReadService, XmlFileReadService>();
services.AddTransient<IPdfFileCreateService, PdfFileCreateService>();
services.AddTransient<IReleaseService, ReleaseService>();

#if DEBUG

#endif
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