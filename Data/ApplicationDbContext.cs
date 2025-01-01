using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoAuth.Models.Accounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DemoAuth.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions)
        : base(dbContextOptions)
        {

        }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Adding unique constraint on Organization - CompanyName
            builder.Entity<Organization>()
                .HasIndex(o => o.CompanyName)
                .IsUnique();  // Ensures that CompanyName is unique


            // if the organization is deleted Deletes all user that is attached to it
            // Configure cascade delete
            builder.Entity<AppUser>()
                .HasOne(u => u.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seeding a "Admin, Staff, Basic" role to AspNetRoles table
            builder.Entity<IdentityRole>().HasData(new IdentityRole { Id = "e2a8a2f2-d88b-4d29-b736-97d347b5e54f", Name = "SuperUser-001", NormalizedName = "SUPERUSER-001".ToUpper() });
            builder.Entity<IdentityRole>().HasData(new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Admin", NormalizedName = "ADMIN".ToUpper() });
            builder.Entity<IdentityRole>().HasData(new IdentityRole { Id = "8e445865-a24d-4543-a6c6-9443d048cdb9", Name = "Staff", NormalizedName = "STAFF".ToUpper() });
            builder.Entity<IdentityRole>().HasData(new IdentityRole { Id = "d88c5d60-b607-442d-b9c5-34b25f68e3a1", Name = "Basic", NormalizedName = "BASIC".ToUpper() });
        }
    }
}