using System;
using System.Collections.Generic;
using IcddWebApp.Services.Models;
using IcddWebApp.Services.Models.Authentication;
using IcddWebApp.Services.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IcddWebApp.Data
{
    public class DatabaseContext : IdentityDbContext
    {
        public DatabaseContext (DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<ContainerMetadata> ContainerMetadata { get; set; }
        public DbSet<ContentMetadata> ContentMetadata { get; set; }
        public DbSet<LinksetMetadata> LinksetMetadata { get; set; }
        public DbSet<User> ContextUsers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<VersionApi> Versions { get; set; }
        public DbSet<AdditionalParameter> AdditionalParameters { get; set; }
        public DbSet<SparqlQuery> SparqlQueries { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ContainerMetadata>().HasKey(i => i.InternalId);
            modelBuilder.Entity<ContentMetadata>().HasKey(i => new { i.Id, i.ContainerInternalId });
            modelBuilder.Entity<LinksetMetadata>().HasKey(i => new { i.Id, i.ContainerInternalId });
            modelBuilder.Entity<ContainerMetadata>().HasMany(i => i.Content).WithOne().HasPrincipalKey("InternalId").OnDelete(DeleteBehavior.ClientCascade);
            modelBuilder.Entity<ContainerMetadata>().HasMany(i => i.Linkset).WithOne().HasPrincipalKey("InternalId").OnDelete(DeleteBehavior.ClientCascade);
            modelBuilder.Entity<Project>().HasMany(i => i.Containers).WithOne().HasPrincipalKey("Id");
            modelBuilder.Entity<User>().HasMany(i => i.Projects).WithMany(i => i.Users);
            modelBuilder.Entity<ContainerMetadata>().HasMany(i => i.Recipients);
            modelBuilder.Entity<ContainerMetadata>().HasMany(i => i.AdditionalParameters).WithOne().HasPrincipalKey("InternalId").OnDelete(DeleteBehavior.ClientCascade);
            modelBuilder.Entity<AdditionalParameter>().Property<string>("Id").HasColumnType("nvarchar(450)").ValueGeneratedOnAdd();
            modelBuilder.Entity<ContainerMetadata>().Property(x => x.Type).HasConversion(x => x.ToString(), x => (ContainerType)Enum.Parse(typeof(ContainerType), x));
            modelBuilder.Entity<ContainerMetadata>().HasMany(i => i.SparqlQueries).WithOne().HasPrincipalKey("InternalId").OnDelete(DeleteBehavior.ClientCascade);

            IdentityRole adminRole = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Admin",
                NormalizedName = "ADMIN"
            };
            modelBuilder.Entity<IdentityRole>().HasData(new List<IdentityRole>
            {
                adminRole,
                new IdentityRole {
                    Id = Guid.NewGuid().ToString(),
                    Name = "User",
                    NormalizedName = "USER"
                },
            });
            var hasher = new PasswordHasher<User>();
            User admin = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                PasswordHash = hasher.HashPassword(null, "admin"),
                EmailConfirmed = true,
                Email = "philipp.hagedorn-n6v@rub.de"
            };
            
            modelBuilder.Entity<User>().HasData(
                admin
            );

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    
                    RoleId = adminRole.Id, // for admin username
                    UserId = admin.Id  // for admin role
                }
            );

            modelBuilder.Entity<VersionApi>().HasData(
                new VersionApi
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "1"
                }
            );
        }
    }
}
