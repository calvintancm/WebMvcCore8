using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ptc_IGH_Sys.Models.Shared;

namespace ptc_IGH_Sys.Models.IGH
{
    [Table("IGH_Leave_Transaction")]
    public class IGH_Leave_Transaction
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long Driver_Id { get; set; }

        [Required]
        public DateTime Leave_Date { get; set; }

        /// <summary>
        /// Convenience column for grouping/filtering by month (e.g. "2026-01").
        /// You can populate this in SaveChanges or via a computed column in SQL.
        /// </summary>
      
        public int Leave_Month { get; set; }

        /// <summary>
        /// 1.0 for Full Day, 0.5 for Half Day
        /// </summary>
        [Required]
        public decimal Leave_Count { get; set; }

        /// <summary>
        /// AL (Annual), MC (Medical), UL (Unpaid), etc.
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Leave_Type { get; set; }

        [StringLength(255)]
        public string Remarks { get; set; }

        public DateTime Created_At { get; set; } = DateTime.Now;

        // Navigation Property
        [ForeignKey("Driver_Id")]
        public virtual TripDriver Driver { get; set; }

        // Optional denormalized field:
        public string Driver_Name { get; set; }
    }
}
