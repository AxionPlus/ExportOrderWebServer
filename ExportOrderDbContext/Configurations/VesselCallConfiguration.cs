
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace ExportOrderDbContext.Configurations;

public class VesselCallConfiguration : IEntityTypeConfiguration<VesselCallEntity>
{
    public void Configure(EntityTypeBuilder<VesselCallEntity> builder)
    {
        var converter = new ValueConverter<byte[], long>(
                       v => BitConverter.ToInt64(v, 0),
                        v => BitConverter.GetBytes(v));

        builder
             .Property(s => s.Version)
             .HasColumnName("xmin")
             .HasColumnType("xid")
             .HasConversion(converter);

        builder.HasMany(s => s.Details);
    }
}

public class VesselCallDetailConfiguration : IEntityTypeConfiguration<VesselCallDetail>
{
    public void Configure(EntityTypeBuilder<VesselCallDetail> builder)
    {
        var converter = new ValueConverter<byte[], long>(
                 v => BitConverter.ToInt64(v, 0),
                 v => BitConverter.GetBytes(v));

        builder
            .Property(s => s.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        builder.HasMany(s => s.ExportOrders);

        builder.ToTable("VesselCall_Details");
    }
}

public class VesselConfiguration : IEntityTypeConfiguration<VesselEntity>
{
    public void Configure(EntityTypeBuilder<VesselEntity> builder)
    {
        var converter = new ValueConverter<byte[], long>(
               v => BitConverter.ToInt64(v, 0),
                v => BitConverter.GetBytes(v));

        builder
         .Property(s => s.Version)
         .HasColumnName("xmin")
         .HasColumnType("xid")
         .HasConversion(converter);

        builder.Navigation(s => s.Flag).AutoInclude();
    }
}

public class VesselCallHistoryConfiguration : IEntityTypeConfiguration<VesselCallHistory>
{
    public void Configure(EntityTypeBuilder<VesselCallHistory> builder)
    {
        builder
               .Property(b => b.Details)
               .HasColumnType("jsonb")
               .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<VesselCallDetailsHistory>>(v, (JsonSerializerOptions?)null),
                    new ValueComparer<List<VesselCallDetailsHistory>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
    }
}