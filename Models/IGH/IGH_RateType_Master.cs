using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ptc_IGH_Sys.Models.IGH
{
    [Table("IGH_RateType_Master")]
    public class IGH_RateType_Master
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Used for manual IDs (1-9)
        public int RateType_ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Rate_Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Payment_Frequency { get; set; }
    }
}