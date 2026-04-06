//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;

//namespace ptc_IGH_Sys.Data
//{
//    public class ApplicationDbContext : IdentityDbContext
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//            : base(options)
//        {
//        }
//    }
//}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ptc_IGH_Sys.Models;
using ptc_IGH_Sys.Models.IGH;
using ptc_IGH_Sys.Models.Shared;
using System.Reflection.Emit;

namespace ptc_IGH_Sys.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>  // ← changed: added <ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ✅ ADD HERE
        public DbSet<IGH_Leave_Transaction> IGH_Leave_Transactions { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Map to your existing table name
            builder.Entity<ApplicationUser>().ToTable("AspNetUsers");

          
            // IMPORTANT: EF Core 8 uses "LockoutEnd" by default
            // but your existing LCL_UAT DB column is "LockoutEndDateUtc"
            // This line maps them correctly to avoid a column mismatch error
            builder.Entity<ApplicationUser>()
                .Property(u => u.LockoutEnd)
                .HasColumnName("LockoutEndDateUtc");
        }
    }
}
