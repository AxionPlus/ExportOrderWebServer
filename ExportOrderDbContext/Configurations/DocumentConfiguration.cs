using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace ExportOrderDbContext.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<DocumentEntity>
{
    public void Configure(EntityTypeBuilder<DocumentEntity> builder)
    {
        var converter = new ValueConverter<byte[], long>(
                        v => BitConverter.ToInt64(v, 0),
                        v => BitConverter.GetBytes(v));

        //var valueComparer = new ValueComparer<byte[]>(
        //    (c1, c2) => c1.SequenceEqual(c2),
        //c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), c => c);


        builder
                 .Property(s => s.Version)
                 .HasColumnName("xmin")
                 .HasColumnType("xid")
                 .HasConversion(converter);


        builder
               .Property(b => b.Shipper)
               .HasColumnType("jsonb")
               .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<DocumentCustomer>(v, (JsonSerializerOptions?)null));

        builder
               .Property(b => b.Consignee)
               .HasColumnType("jsonb")
               .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<DocumentCustomer>(v, (JsonSerializerOptions?)null));


        builder.Navigation(s => s.CreateUser).AutoInclude();  // ???
    }
}
