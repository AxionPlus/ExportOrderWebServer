    using ExportOrderEntites.AuditLog;
using ExportOrderEntites.BillofLading;
using ExportOrderEntites.EmailLogRecords;
using ExportOrderEntites.ImportVesselCall;
using ExportOrderEntites.ReleaseRecord;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace ExportOrderDbContext;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    //public string ConnectionString { get; set; } = "User ID=postgres;Password=Hd6!#qcS2htQ2;Host=db.axionplus.ru;Port=5454;Database=ExportOrderAxion;Pooling=true;";

    #region DataBASES

    public DbSet<AuditEntry> AuditLogs { get; set; }

    // Catalogues
    public DbSet<CountryCatalog> Countries { get; set; }
    public DbSet<LocationCatalog> Locations { get; set; }
    public DbSet<TerminalCatalog> Terminals { get; set; }
    public DbSet<CommodityCatalog> Commodities { get; set; }
    public DbSet<CarrierCatalog> Carriers { get; set; }
    public DbSet<CarrierTerminalDetails> CarrierDetails { get; set; }
    public DbSet<CustomerCatalog> Customers { get; set; }
    public DbSet<CustomsCatalog> CustomsOffices { get; set; }
    public DbSet<SupplementaryUnitCatalog> SupplementaryUnits { get; set; }

    // Entities
    public DbSet<CntrTpSz> ContainerTypeSize { get; set; }
    public DbSet<VesselEntity> Vessels { get; set; }
    public DbSet<VesselCallEntity> VesselCalls { get; set; }
    public DbSet<ImportVesselCallEntity> ImportVesselCalls { get; set; }
    public DbSet<DocumentEntity> Documents { get; set; }
    public DbSet<ExportOrderEntity> ExportOrders { get; set; }
    public DbSet<MyCompanyEntity> MyCompany { get; set; }
    public DbSet<PersonEntity> Persons { get; set; }

    #endregion

    private readonly IHttpContextAccessor _httpContextAccessor;




    [ActivatorUtilitiesConstructor]
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    // Этот конструктор будет использоваться EF Core для миграций
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public ApplicationDbContext()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditEntry>().Property(e => e.OldValues)
            .HasColumnType("jsonb");
        modelBuilder.Entity<AuditEntry>().Property(e => e.NewValues)
            .HasColumnType("jsonb");


        var converter = new ValueConverter<byte[], long>(
                        v => BitConverter.ToInt64(v, 0),
                        v => BitConverter.GetBytes(v));

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new MyCompanyConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentConfiguration());
        modelBuilder.ApplyConfiguration(new ExportOrderConfiguration());
        modelBuilder.ApplyConfiguration(new VesselConfiguration());
        modelBuilder.ApplyConfiguration(new VesselCallConfiguration());
        modelBuilder.ApplyConfiguration(new VesselCallDetailConfiguration());
        modelBuilder.ApplyConfiguration(new ImportVesselCallConfiguration());
        modelBuilder.ApplyConfiguration(new ImportVesselCallDetailConfiguration());

        modelBuilder.ApplyConfiguration(new DocumentHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new ExportOrderHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new ExportOrderRecordHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new VesselCallHistoryConfiguration());

        #region Tables
        modelBuilder.Entity<ExportOrderRecord>(entity => { entity.ToTable(name: "ExportOrder_Records"); });

        modelBuilder.Entity<BillofLadingEntity>(entity => { entity.ToTable(name: "BillofLadings"); });
        modelBuilder.Entity<BillofLadingContainerRecord>(entity => { entity.ToTable(name: "BillofLading_ContainerRecords"); });
        modelBuilder.Entity<ReleaseImportContainerRecordEntity>(entity => { entity.ToTable(name: "ReleaseContainerRecords"); });
        modelBuilder.Entity<EmailLogRecord>(entity => { entity.ToTable(name: "EmailLogRecords"); });
        modelBuilder.Entity<ReleaseRemark>(entity => { entity.ToTable(name: "ReleaseRemarks"); });

        //modelBuilder.Entity<DocumentHistory>(entity => { entity.ToTable(name: "History_Documents"); });
        //modelBuilder.Entity<ExportOrderHistory>(entity => { entity.ToTable(name: "History_ExportOrders"); });
        //modelBuilder.Entity<ExportOrderRecordHistory>(entity => { entity.ToTable(name: "History_ExportOrderRecords"); });
        //modelBuilder.Entity<VesselCallHistory>(entity => { entity.ToTable(name: "History_VesselCall"); });
        #endregion
    }


    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = base.SaveChanges(acceptAllChangesOnSuccess);
        OnAfterSaveChanges(auditEntries);
        return result;
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        await OnAfterSaveChanges(auditEntries);
        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditEntry || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry
            {
                EntityName = entry.Entity.GetType().Name,
                Action = entry.State.ToString(),
                Timestamp = DateTime.UtcNow,
                UserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier),
                UserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name,
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
            };

            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary || property.Metadata.IsShadowProperty())
                    continue;

                string propertyName = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.EntityId = property.CurrentValue switch
                    {
                        long l => l.ToString(),
                        int i => i.ToString(),
                        short s => s.ToString(),
                        _ => property.CurrentValue?.ToString()
                    };
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }
        }

        return auditEntries;
    }

    private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return Task.CompletedTask;

        // Сериализация словарей в JSON для хранения в PostgreSQL jsonb
        foreach (var auditEntry in auditEntries)
        {
            AuditLogs.Add(new AuditEntry
            {
                EntityName = auditEntry.EntityName,
                Action = auditEntry.Action,
                EntityId = auditEntry.EntityId,
                OldValues = auditEntry.OldValues,
                NewValues = auditEntry.NewValues,
                Timestamp = auditEntry.Timestamp,
                UserId = auditEntry.UserId,
                UserName = auditEntry.UserName,
                IpAddress = auditEntry.IpAddress
            });
        }

        return SaveChangesAsync();
    }
}