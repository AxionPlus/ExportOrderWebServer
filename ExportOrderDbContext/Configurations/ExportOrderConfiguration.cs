using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ExportOrderDbContext.Configurations;

public class ExportOrderConfiguration : IEntityTypeConfiguration<ExportOrderEntity>
{
    public void Configure(EntityTypeBuilder<ExportOrderEntity> builder)
    {
        var converter = new ValueConverter<byte[], long>(
                       v => BitConverter.ToInt64(v, 0),
                        v => BitConverter.GetBytes(v));

        //var valueComparer = new ValueComparer<byte[]>(
        //    (c1, c2) => c1.SequenceEqual(c2),
        //c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), c => c);


        builder.Property(s => s.Version)
               .HasColumnName("xmin")
               .HasColumnType("xid")
               .HasConversion(converter);

        //builder.Navigation(s => s.Records).AutoInclude();
        
        //builder.Navigation(s => s.Carrier).AutoInclude();

        //builder.Navigation(s => s.VesselCall).AutoInclude();


        builder.HasMany(s=>s.Documents).WithMany(s=>s.ExportOrders).UsingEntity(j => j.ToTable("ExportOrders_Documents"));
    }
}


public class ExportOrderRecordConfiguration : IEntityTypeConfiguration<ExportOrderRecord>
{
    public void Configure(EntityTypeBuilder<ExportOrderRecord> builder)
    {
        var converter = new ValueConverter<byte[], long>(
                       v => BitConverter.ToInt64(v, 0),
                        v => BitConverter.GetBytes(v));

        //var valueComparer = new ValueComparer<byte[]>(
        //    (c1, c2) => c1.SequenceEqual(c2),
        //c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), c => c);


        builder.ToTable("ExportOrder_Records");

        builder.Navigation(s => s.CntrType).AutoInclude();

        builder.Navigation(s => s.Contents).AutoInclude();
    }
}
