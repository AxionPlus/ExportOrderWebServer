
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        //builder.HasOne(s => s.Vessel);
        //builder.HasOne(s => s.Terminal);

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
