using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActorUserName).HasMaxLength(150);
        builder.Property(x => x.Action).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Module).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.IpAddress).HasMaxLength(64);

        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_AuditLogs_CreatedAt");
        builder.HasIndex(x => x.ActorUserId).HasDatabaseName("IX_AuditLogs_ActorUserId");
        builder.HasIndex(x => new { x.Module, x.EntityType }).HasDatabaseName("IX_AuditLogs_Module_Entity");
    }
}
