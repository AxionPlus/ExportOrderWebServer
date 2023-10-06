
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

        builder.HasOne(s => s.Vessel);

        builder.Navigation(s => s.Vessel).AutoInclude();

        builder.Navigation(s => s.LoadingTerminal).AutoInclude();

        //builder.Navigation(s => s.POD).AutoInclude();
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
