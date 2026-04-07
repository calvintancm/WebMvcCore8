using System.ComponentModel.DataAnnotations;

namespace ptc_IGH_Sys.ViewModels.IGH
{
    public class IGH_RateType_MasterViewModel
    {
        public int RateType_ID { get; set; }   // manual ID (1-9)

        [Required]
        [StringLength(50)]
        public string Rate_Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Payment_Frequency { get; set; }
    }
}