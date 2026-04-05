using System;
using System.ComponentModel.DataAnnotations;

namespace ptc_IGH_Sys.ViewModels.IGH
{
   
        public class SalaryAdjustmentViewModel
        {
            public long Id { get; set; }

            public long DriverId { get; set; }

            [Display(Name = "Driver Name")]
            public string DriverName { get; set; }

            [Required]
            [Display(Name = "Adjustment Date")]
            public DateTime AdjustmentDate { get; set; }

            [Display(Name = "Adjustment Month")]
            public int AdjustmentMonth { get; set; }

            [Required]
            [Display(Name = "Adjustment Type")]
            [StringLength(50)]
            public string AdjustmentType { get; set; }

            [Required]
            [Display(Name = "Adjustment Amount")]
            public decimal AdjustmentAmount { get; set; }

            [Display(Name = "Remarks")]
            public string Remarks { get; set; }

            [Display(Name = "Created At")]
            public DateTime CreatedAt { get; set; }
        
    }
}