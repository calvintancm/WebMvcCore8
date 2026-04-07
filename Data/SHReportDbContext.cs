using Microsoft.EntityFrameworkCore;

namespace ptc_IGH_Sys.Data
{
   
    public class SHReportDbContext : DbContext
    {
        public SHReportDbContext(DbContextOptions<SHReportDbContext> options)
            : base(options)
        {
        }

        // No DbSets required because we only execute stored procedures.
        // If you need to query tables from SHReportPortal, add DbSet<T> here.
    }
}
