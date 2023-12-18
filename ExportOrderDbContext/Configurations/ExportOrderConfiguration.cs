using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace ExportOrderDbContext.Configurations;

public class ExportOrderConfiguration : IEntityTypeConfiguration<ExportOrderEntity>
{
    public void Configure(EntityTypeBuilder<ExportOrderEntity> builder)
    {
        var converter = new ValueConverter<byte[], long>(
                       v => BitConverter.ToInt64(v, 0),
                        v => BitConverter.GetBytes(v));

        builder.Property(s => s.Version)
               .HasColumnName("xmin")
               .HasColumnType("xid")
               .HasConversion(converter);

        builder.HasMany(s=>s.Documents).WithMany(s=>s.ExportOrders).UsingEntity(j => j.ToTable("ExportOrders_Documents"));
    }
}

//public class ExportOrderRecordConfiguration : IEntityTypeConfiguration<ExportOrderRecord>
//{
//    public void Configure(EntityTypeBuilder<ExportOrderRecord> builder)
//    {
//        //var converter = new ValueConverter<byte[], long>(
//        //               v => BitConverter.ToInt64(v, 0),
//        //                v => BitConverter.GetBytes(v));


//        builder.ToTable("ExportOrder_Records");        
//    }
//}


public class ExportOrderHistoryConfiguration : IEntityTypeConfiguration<ExportOrderHistory>
{
    public void Configure(EntityTypeBuilder<ExportOrderHistory> builder)
    {
        builder.HasMany(s => s.Documents).WithMany(d => d.ExportOrders);
        builder.HasMany(s => s.Records).WithOne(r => r.ExportOrder);
    }
}

public class ExportOrderRecordHistoryConfiguration : IEntityTypeConfiguration<ExportOrderRecordHistory>
{
    public void Configure(EntityTypeBuilder<ExportOrderRecordHistory> builder)
    {
        builder
                .Property(b => b.Contents)
                .HasColumnType("jsonb")
                .HasConversion(
                     v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                     v => JsonSerializer.Deserialize<List<ContainerContentHistory>>(v, (JsonSerializerOptions?)null),
                     new ValueComparer<List<ContainerContentHistory>>(
                             (c1, c2) => c1.SequenceEqual(c2),
                             c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                             c => c.ToList()));
    }
}
