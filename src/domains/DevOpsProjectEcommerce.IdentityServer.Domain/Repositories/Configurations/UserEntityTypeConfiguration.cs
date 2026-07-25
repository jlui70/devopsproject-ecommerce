using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using DevOpsProjectEcommerce.IdentityServer.Domain.Models;

namespace DevOpsProjectEcommerce.IdentityServer.Domain.Repositories.Configurations;

public class UserEntityTypeConfiguration: IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("User");
        builder.HasKey(user => user.Id);
        builder
            .Property(user => user.Id)
            // .HasColumnType("serial")
            .ValueGeneratedOnAdd();
        
        builder
            .Property(user => user.Email)
            .HasColumnType("text")
            .IsRequired();
        
        builder
            .Property(user => user.Password)
            .HasColumnType("text")
            .IsRequired();
    }
}
