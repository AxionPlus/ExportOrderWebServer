

using ExportOrderEntites.Document;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ExportOrderDbContext;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public readonly object MyCompany;

    public string ConnectionString { get; set; } = "User ID=postgres;Password=Jyww3Xq2Hv7C;Host=46.173.5.112;Port=5434;Database=NeoLineMainDb;Pooling=true;";

    #region DataBASES
    public DbSet<CountryCatalog> Countries { get; set; }
    public DbSet<LocationCatalog> Locations { get; set; }
    public DbSet<TerminalCatalog> Terminals { get; set; }
    public DbSet<CommodityCatalog> Commodities { get; set; }
    public DbSet<Carrier> Carriers { get; set; }
    public DbSet<CntrEntity> Containers { get; set; }
    public DbSet<CntrTpSz> ContainerTypeSize { get; set; }
    public DbSet<CustomerCatalog> Customers { get; set; }
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
        modelBuilder.ApplyConfiguration(new ExportOrderConfiguration());
        modelBuilder.ApplyConfiguration(new ExportOrderRecordConfiguration());

        






        #region TableNames
        // modelBuilder.Entity<ContractorCommunicationChannel>(entity => { entity.ToTable(name: "Contractor_CommunicationChannels"); });
        // modelBuilder.Entity<ContractorType>(entity => { entity.ToTable(name: "Contractor_Types"); });





        #endregion

        modelBuilder.Entity<CustomerCatalog>().Navigation(s => s.Country).AutoInclude();
        //modelBuilder.Entity<DocumentEntity>().Ignore(x => x.Consignee);
        //modelBuilder.Entity<DocumentEntity>().Ignore(x => x.Shipper);


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