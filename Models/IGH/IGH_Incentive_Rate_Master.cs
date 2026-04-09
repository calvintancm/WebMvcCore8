using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ptc_IGH_Sys.Models.IGH
{
    [Table("IGH_Incentive_Rate_Master")]
    public class IGH_Incentive_Rate_Master
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("IncentiveRate_ID")]
        public int IncentiveRateId { get; set; }

        [Required]
        [Column("RateType_ID")]
        public int RateTypeId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Job_Title_Group")]
        public string JobTitleGroup { get; set; }

        [Required]
        [Column("Rate_Value")]
        public decimal RateValue { get; set; }

        [Required]
        [StringLength(3)]
        [Column("Currency")]
        public string Currency { get; set; } = "SGD";

        [StringLength(50)]
        [Column("Trip_Type")]
        public string? TripType { get; set; }

        [StringLength(20)]
        [Column("Terminal_Area")]
        public string? TerminalArea { get; set; }

        [Column("Trip_Threshold_Min")]
        public int? TripThresholdMin { get; set; }

        [StringLength(20)]
        [Column("Assignment_Type")]
        public string? AssignmentType { get; set; }

        [Required]
        [Column("Effective_Date")]
        public DateTime? EffectiveDate { get; set; } = DateTime.Now;

       
        [ForeignKey("RateTypeId")]
        public virtual IGH_RateType_Master RateType { get; set; }
    }
}