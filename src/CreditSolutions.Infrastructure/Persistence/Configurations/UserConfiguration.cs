using CreditSolutions.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditSolutions.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.DisplayName).HasMaxLength(150).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(250).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
