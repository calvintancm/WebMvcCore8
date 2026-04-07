using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ptc_IGH_Sys.Models.Shared;

namespace ptc_IGH_Sys.Models.IGH
{
    [Table("IGH_Salary_Adjustment_Transaction")]
    public class IGH_Salary_Adjustment_Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [Column("Driver_ID")]
        public long DriverId { get; set; }

        /// <summary>
        /// Denormalized driver name for quick binding/search in Kendo UI grids.
        /// Still keep DriverId + navigation property for integrity.
        /// </summary>
        [StringLength(200)]
        [Column("Driver_Name")]
        public string DriverName { get; set; }

        [Required]
        [Column("Adjustment_Date")]
        public DateTime AdjustmentDate { get; set; }

        /// <summary>
        /// Convenience field for grouping/filtering by month (e.g. "2026-01").
        /// Can be populated in SaveChanges or via SQL computed column.
        /// </summary>
       
        public int AdjustmentMonth { get; set; }

        [Required]
        [StringLength(50)]
        public string AdjustmentType { get; set; }

        [Required]
        public decimal AdjustmentAmount { get; set; }

        [Column("Remarks")]
        public string Remarks { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Property
        [ForeignKey("DriverId")]
        public virtual TripDriver Driver { get; set; }
    }
}
