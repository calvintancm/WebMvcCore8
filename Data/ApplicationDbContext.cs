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
        public DbSet<IGH_Leave_Transaction> IGH_Leave_Transactions { get; set; }
        public DbSet<IGH_Salary_Adjustment_Transaction> IGH_Salary_Adjustment_Transaction { get; set; }
        public DbSet<TripDriver> TripDrivers { get; set; }
        public DbSet<IGH_RateType_Master> IGH_RateType_Master { get; set; }
        public DbSet<IGH_JobTitleRate_Master> IGH_JobTitleRate_Master { get; set; }
        public DbSet<IGH_Incentive_Rate_Master> IGH_Incentive_Rate_Master { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Map to your existing table name
            builder.Entity<ApplicationUser>().ToTable("AspNetUsers");


            //
            builder.Entity<IGH_JobTitleRate_Master>()
                .Property(e => e.RateValue)
                .HasPrecision(18, 2);

            builder.Entity<IGH_Incentive_Rate_Master>()
                .Property(e => e.RateValue)
                .HasPrecision(18, 2);

            builder.Entity<IGH_Leave_Transaction>()
                .Property(e => e.Leave_Count)
                .HasPrecision(18, 2);

            builder.Entity<IGH_Salary_Adjustment_Transaction>()
                .Property(e => e.AdjustmentAmount)
                .HasPrecision(18, 2);

            // IMPORTANT: EF Core 8 uses "LockoutEnd" by default
            // but your existing LCL_UAT DB column is "LockoutEndDateUtc"
            // This line maps them correctly to avoid a column mismatch error
            builder.Entity<ApplicationUser>()
                .Property(u => u.LockoutEnd)
                .HasColumnName("LockoutEndDateUtc");
        }
    }
}
