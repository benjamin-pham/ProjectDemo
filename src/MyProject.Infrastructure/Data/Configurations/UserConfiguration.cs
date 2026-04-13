using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyProject.Domain.Entities;

namespace MyProject.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("users");

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(u => u.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);

        builder.Property(u => u.Birthday)
            .HasColumnName("birthday");

        builder.Property(u => u.HashedRefreshToken)
            .HasColumnName("hashed_refresh_token")
            .HasMaxLength(512);

        builder.Property(u => u.RefreshTokenExpiresAt)
            .HasColumnName("refresh_token_expires_at");

        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("ix_users_username")
            .HasFilter("is_deleted = false");

        builder.HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity("user_roles",
                l => l.HasOne(typeof(Role)).WithMany().HasForeignKey("role_id")
                      .HasConstraintName("fk_user_roles_role_id").OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne(typeof(User)).WithMany().HasForeignKey("user_id")
                      .HasConstraintName("fk_user_roles_user_id").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("user_roles");
                    j.HasKey("user_id", "role_id");
                });
    }
}
