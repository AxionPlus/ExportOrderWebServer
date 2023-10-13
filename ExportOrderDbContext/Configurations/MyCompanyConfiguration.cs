using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


namespace ExportOrderDbContext.Configurations;

public class MyCompanyConfiguration : IEntityTypeConfiguration<MyCompanyEntity>
{
    public void Configure(EntityTypeBuilder<MyCompanyEntity> builder)
    {
        var converter = new ValueConverter<byte[], long>(
                v => BitConverter.ToInt64(v, 0),
                v => BitConverter.GetBytes(v));

        builder
                 .Property(s => s.Version)
                 .HasColumnName("xmin")
                 .HasColumnType("xid")
                 .HasConversion(converter);
    }
}
