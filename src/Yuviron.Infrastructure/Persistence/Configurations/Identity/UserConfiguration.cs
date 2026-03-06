using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).IsRequired().HasMaxLength(320);
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(256);
        builder.Property(x => x.LoginCodeHash).HasMaxLength(256); // Для кодов авторизации

        builder.Property(x => x.AcceptMarketing).HasDefaultValue(false);
        builder.Property(x => x.AcceptTerms).IsRequired();
        builder.Property(x => x.AccountState).IsRequired(); 
        
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}