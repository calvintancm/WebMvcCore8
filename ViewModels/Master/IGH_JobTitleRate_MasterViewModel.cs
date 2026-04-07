using System;
using System.ComponentModel.DataAnnotations;

namespace ptc_IGH_Sys.ViewModels.IGH
{
    public class IGH_JobTitleRate_MasterViewModel
    {
        public int JobRateId { get; set; }   // auto-generated

        [Required]
        [StringLength(50)]
        public string JobTitleGroup { get; set; }

        [Required]
        public int RateTypeId { get; set; }

        // For display only – shows Rate Description from lookup
        public string RateTypeDescription { get; set; }

        [Required]
        public decimal RateValue { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "SGD";

        [Required]
        public DateTime EffectiveDate { get; set; } = DateTime.Now;
    }
}