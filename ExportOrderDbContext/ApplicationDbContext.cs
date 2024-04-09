using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ExportOrderDbContext;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    //public string ConnectionString { get; set; } = "User ID=postgres;Password=Hd6!#qcS2htQ2;Host=db.axionplus.ru;Port=5454;Database=ExportOrderAxion;Pooling=true;";

    #region DataBASES

    // Catalogues
    public DbSet<CountryCatalog> Countries { get; set; }
    public DbSet<LocationCatalog> Locations { get; set; }
    public DbSet<TerminalCatalog> Terminals { get; set; }
    public DbSet<CommodityCatalog> Commodities { get; set; }
    public DbSet<CarrierCatalog> Carriers { get; set; }
    public DbSet<CarrierTerminalDetails> CarrierDetails { get; set; }
    public DbSet<CustomerCatalog> Customers { get; set; }
    public DbSet<CustomsCatalog> CustomsOffices { get; set; }

    // Entities
    public DbSet<CntrTpSz> ContainerTypeSize { get; set; }
    public DbSet<VesselEntity> Vessels { get; set; }
    public DbSet<VesselCallEntity> VesselCalls { get; set; }
    public DbSet<DocumentEntity> Documents { get; set; }
    public DbSet<ExportOrderEntity> ExportOrders { get; set; }
    public DbSet<MyCompanyEntity> MyCompany { get; set; }
    public DbSet<PersonEntity> Persons { get; set; }    

    #endregion


    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public ApplicationDbContext()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    optionsBuilder.UseNpgsql(ConnectionString);
    //}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var converter = new ValueConverter<byte[], long>(
                        v => BitConverter.ToInt64(v, 0),
                        v => BitConverter.GetBytes(v));

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new MyCompanyConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentConfiguration());
        modelBuilder.ApplyConfiguration(new ExportOrderConfiguration());
        //modelBuilder.ApplyConfiguration(new ExportOrderRecordConfiguration());
        modelBuilder.ApplyConfiguration(new VesselConfiguration());
        modelBuilder.ApplyConfiguration(new VesselCallConfiguration());
        modelBuilder.ApplyConfiguration(new VesselCallDetailConfiguration());

        modelBuilder.ApplyConfiguration(new DocumentHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new ExportOrderHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new ExportOrderRecordHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new VesselCallHistoryConfiguration());

        #region Tables
        modelBuilder.Entity<ExportOrderRecord>(entity => { entity.ToTable(name: "ExportOrder_Records"); });

        //modelBuilder.Entity<DocumentHistory>(entity => { entity.ToTable(name: "History_Documents"); });
        //modelBuilder.Entity<ExportOrderHistory>(entity => { entity.ToTable(name: "History_ExportOrders"); });
        //modelBuilder.Entity<ExportOrderRecordHistory>(entity => { entity.ToTable(name: "History_ExportOrderRecords"); });
        //modelBuilder.Entity<VesselCallHistory>(entity => { entity.ToTable(name: "History_VesselCall"); });
        #endregion
    }
}