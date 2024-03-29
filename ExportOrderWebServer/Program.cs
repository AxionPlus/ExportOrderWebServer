using ExportOrderWebServer;
using ExportOrderWebServer.Areas.Statistic;
using ExportOrderWebServer.Service;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString,
                        optionsBuilder => optionsBuilder.MigrationsAssembly("ExportOrderWebServer")));

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
                                                            options.UseNpgsql(connectionString), ServiceLifetime.Transient);

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//                .AddEntityFrameworkStores<ApplicationDbContext>()
//                .AddDefaultTokenProviders();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "AspNetCore.Identity.Application.ExportOrderWebServer";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);      // время простоя
    //options.IOTimeout = TimeSpan.FromMinutes(5);      // время активности сессии
});

builder.Services.AddScoped<TokenProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddAuthentication().AddCookie(cfg => cfg.SlidingExpiration = true).AddJwtBearer(x =>
{
    // options
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddHttpClient();
builder.Services.AddMudServices();

builder.Services.AddTransient<IUserProvider, UserProvider>();
builder.Services.AddTransient<ICarrierProvider, CarrierProvider>();
builder.Services.AddTransient<ICommodityProvider, CommodityProvider>();
builder.Services.AddTransient<ICountryProvider, CountryProvider>();
builder.Services.AddTransient<ICustomerProvider, CustomerProvider>();
builder.Services.AddTransient<ICustomsProvider, CustomsProvider>();
builder.Services.AddTransient<ILocationProvider, LocationProvider>();
builder.Services.AddTransient<ITerminalProvider, TerminalProvider>();
builder.Services.AddTransient<IVesselProvider, VesselProvider>();
builder.Services.AddTransient<IVesselCallProvider, VesselCallProvider>();
builder.Services.AddTransient<ICntrTypeProvider, CntrTypeProvider>();
builder.Services.AddTransient<IDocumentProvider, DocumentProvider>();
builder.Services.AddTransient<IExportOrderProvider, ExportOrderProvider>();
builder.Services.AddTransient<IMyCompanyProvider, MyCompanyProvider>();

builder.Services.AddTransient<StatisticProvider>();
builder.Services.AddTransient<CheckUploadResultService>();
builder.Services.AddTransient<CheckCntrNumService>();

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


app.UseSession();
app.UseStaticFiles();


app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
