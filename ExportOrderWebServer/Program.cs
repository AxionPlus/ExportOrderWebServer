using ExportOrderWebServer.Areas.Carrier.Provider;
using ExportOrderWebServer.Areas.Commodity.Provider;
using ExportOrderWebServer.Areas.Country.Provider;
using ExportOrderWebServer.Areas.Customer.Provider;
using ExportOrderWebServer.Areas.Identity;
using ExportOrderWebServer.Areas.Location.Provider;
using ExportOrderWebServer.Areas.Terminal.Provider;
using ExportOrderWebServer.Areas.Vessel.Provider;
using ExportOrderWebServer.Areas.VesselCall.Provider;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString,
          optionsBuilder =>
                optionsBuilder.MigrationsAssembly("ExportOrderWebServer")));

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString), ServiceLifetime.Transient);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

builder.Services.AddMudServices();

builder.Services.AddTransient<ICarrierProvider, CarrierProvider>();
builder.Services.AddTransient<ICommodityProvider, CommodityProvider>();
builder.Services.AddTransient<ICountryProvider, CountryProvider>();
builder.Services.AddTransient<ICustomerProvider, CustomerProvider>();
builder.Services.AddTransient<ILocationProvider, LocationProvider>();
builder.Services.AddTransient<ITerminalProvider, TerminalProvider>();
builder.Services.AddTransient<IVesselProvider, VesselProvider>();
builder.Services.AddTransient<IVesselCallProvider, VesselCallProvider>();


var app = builder.Build();

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

app.Run();
