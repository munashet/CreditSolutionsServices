using CreditSolutions.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditSolutions.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.AccountNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(c => c.AccountNumber).IsUnique();
        builder.Property(c => c.FullName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.MobileNumber).HasMaxLength(30).IsRequired();
        builder.Property(c => c.Balance).HasPrecision(18, 2);

        builder.HasMany(c => c.Promises)
            .WithOne()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Payments)
            .WithOne()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(c => c.Promises).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(c => c.Payments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
