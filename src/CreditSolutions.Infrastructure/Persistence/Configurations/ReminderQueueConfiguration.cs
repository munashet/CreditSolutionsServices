using CreditSolutions.Domain.Reminders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditSolutions.Infrastructure.Persistence.Configurations;

public sealed class ReminderQueueConfiguration : IEntityTypeConfiguration<ReminderQueueItem>
{
    public void Configure(EntityTypeBuilder<ReminderQueueItem> builder)
    {
        builder.ToTable("ReminderQueue");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.MobileNumber).HasMaxLength(30).IsRequired();
        builder.Property(r => r.Message).HasMaxLength(500).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Outcome).HasMaxLength(500);
        builder.HasIndex(r => new { r.Status, r.CreatedAt });
    }
}
