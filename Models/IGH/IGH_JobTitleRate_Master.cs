using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ptc_IGH_Sys.Models.IGH
{
    [Table("IGH_JobTitleRate_Master")]
    public class IGH_JobTitleRate_Master
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("JobRate_ID")]
        public int JobRateId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Job_Title_Group")]
        public string JobTitleGroup { get; set; }

        [Required]
        [Column("RateType_ID")]
        public int RateTypeId { get; set; }

        [Required]
        [Column("Rate_Value")]
        public decimal RateValue { get; set; }


        [Required]
        [StringLength(3)]
        [Column("Currency")]
        public string Currency { get; set; } = "SGD";

        [Required]
        [Column("Effective_Date")]
        public DateTime EffectiveDate { get; set; } = DateTime.Now;

        // Navigation Property
        [ForeignKey("RateTypeId")]
        public virtual IGH_RateType_Master RateType { get; set; }
    }
}