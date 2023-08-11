using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ExportOrderDbContext.Configurations;

public class CntrConfiguration : IEntityTypeConfiguration<CntrEntity>
{
    public void Configure(EntityTypeBuilder<CntrEntity> builder)
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

        builder.HasOne(s => s.TpSz);

        builder.Navigation(s => s.TpSz)
               .AutoInclude();

        builder.Navigation(s => s.Carrier)
       .AutoInclude();
    }
}
