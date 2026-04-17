using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Infrastructure.Data.Configurations;

namespace ProjectTemplate.Infrastructure.Data.Configurations;

public sealed class RoleConfiguration : BaseEntityConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);

        builder.ToTable("roles");

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Permissions)
            .HasColumnName("permissions")
            .HasColumnType("text[]")
            .IsRequired();

        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("ix_roles_name");
    }
}
