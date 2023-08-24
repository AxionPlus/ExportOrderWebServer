
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ExportOrderDbContext;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    //public readonly object MyCompany;

    public string ConnectionString { get; set; } = "User ID=postgres;Password=Jyww3Xq2Hv7C;Host=46.173.5.112;Port=5434;Database=AxionExportOrder;Pooling=true;";

    #region DataBASES
    // Catalogues
    public DbSet<CountryCatalog> Countries { get; set; }
    public DbSet<LocationCatalog> Locations { get; set; }
    public DbSet<TerminalCatalog> Terminals { get; set; }
    public DbSet<CommodityCatalog> Commodities { get; set; }
    public DbSet<CarrierCatalog> Carriers { get; set; }
    public DbSet<CustomerCatalog> Customers { get; set; }

    // Entities
    public DbSet<CntrEntity> Containers { get; set; }
    public DbSet<CntrTpSz> ContainerTypeSize { get; set; }
    public DbSet<VesselEntity> Vessels { get; set; }
    public DbSet<VesselCallEntity> VesselCalls { get; set; }
    public DbSet<DocumentEntity> Documents { get; set; }
    public DbSet<ExportOrderEntity> ExportOrders { get; set; }

    

    #endregion

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        //templateData = new TemplateData();
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public ApplicationDbContext()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var converter = new ValueConverter<byte[], long>(
                        v => BitConverter.ToInt64(v, 0),
                        v => BitConverter.GetBytes(v));

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CntrConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentConfiguration());
        modelBuilder.ApplyConfiguration(new ExportOrderConfiguration());
        modelBuilder.ApplyConfiguration(new ExportOrderRecordConfiguration());
        modelBuilder.ApplyConfiguration(new VesselCallConfiguration());
        modelBuilder.ApplyConfiguration(new VesselConfiguration());

        #region TableNames        
        // modelBuilder.Entity<ContractorType>(entity => { entity.ToTable(name: "Contractor_Types"); });

        #endregion

        modelBuilder.Entity<CustomerCatalog>().Navigation(s => s.Country).AutoInclude();
        modelBuilder.Entity<LocationCatalog>().Navigation(s => s.Country).AutoInclude();
        modelBuilder.Entity<TerminalCatalog>().Navigation(s => s.Location).AutoInclude();
        modelBuilder.Entity<CntrEntity>().Navigation(s => s.TpSz).AutoInclude();
        //modelBuilder.Entity<CntrEntity>().Navigation(s => s.Carrier).AutoInclude();
        modelBuilder.Entity<VesselEntity>().Navigation(s => s.Flag).AutoInclude();

        modelBuilder.Entity<DocumentEntity>().Navigation(s => s.CreateUser).AutoInclude();  // ???

        //var type = new CntrTpSz()
        //{
        //    ISO = "22GP",
        //    Size = CntrSize._20,
        //    Height = CntrHeight.DC,
        //    Type = CntrType.DC,
        //    Normolize = "20DC"
        //};

        //modelBuilder.Entity<CntrTpSz>().HasData(type);


        //var carrier = new CarrierEntity()
        //{

        //    FullName = "LAM",
        //    ShortName = "LAM"

        //};

        //modelBuilder.Entity<CarrierEntity>().HasData(carrier);

        //var cntEvent = new CntrEventType()
        //{
        //    Id = "TEST",
        //    Name = "TEST",
        //    NameEn = "TEST",
        //    Finance = false,
        //    Mode = CntrEventTypeMode.Carrier,
        //    Notes = "TEST"


        //};
        //modelBuilder.Entity<CntrEventType>().HasData(cntEvent);


        //var cont = new ContractorEntity() {
        //FullName="Test",
        //ShortName="Test",
        //    TaxNo= "TaxNo"
        //};

        //modelBuilder.Entity<ContractorEntity>().HasData(cont);
        //modelBuilder.Entity<CntrEntity>().HasData(new CntrEntity() { Carrier= carrier, Num = "ABCU1234567", TpSz = type, TareWt = 2200, MaxPayLoad = 25000, Volume = 54, IsSOC = false });


    }
}