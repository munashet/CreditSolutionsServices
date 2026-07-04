using CreditSolutions.Domain.Promises;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditSolutions.Infrastructure.Persistence.Configurations;

public sealed class PromiseConfiguration : IEntityTypeConfiguration<Promise>
{
    public void Configure(EntityTypeBuilder<Promise> builder)
    {
        builder.ToTable("Promises");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.PromiseDate).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(p => new { p.Status, p.PromiseDate });
    }
}
