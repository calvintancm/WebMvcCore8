using System;
using System.ComponentModel.DataAnnotations;

namespace ptc_IGH_Sys.ViewModels.IGH
{
    public class IGH_Incentive_Rate_MasterViewModel
    {
        public int IncentiveRateId { get; set; }

        [Required]
        public int RateTypeId { get; set; }

        public string? RateTypeDescription { get; set; }

        [Required]
        [StringLength(50)]
        public string JobTitleGroup { get; set; } = string.Empty;

        [Required]
        public decimal RateValue { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "SGD";

        [StringLength(50)]
        public string? TripType { get; set; } = string.Empty;   // default to empty, not null

        [StringLength(20)]
        public string? TerminalArea { get; set; } = string.Empty;

        public int? TripThresholdMin { get; set; } = 0;          // default 0 instead of null

        [StringLength(20)]
        public string? AssignmentType { get; set; } = string.Empty;

        [Required]
        public DateTime EffectiveDate { get; set; } = DateTime.Now;

        // Constructor to ensure defaults
        public IGH_Incentive_Rate_MasterViewModel()
        {
            TripType = "";
            TerminalArea = "";
            AssignmentType = "";
            TripThresholdMin = 0;
        }
    }
}