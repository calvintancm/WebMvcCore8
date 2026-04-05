using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
//using ptc_IGH_Sys.Models.Shared;

namespace ptc_IGH_Sys.Models.IGH
{
    [Table("IGH_Driver_Trip_Transaction")]
    public class IGH_Driver_Trip_Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Trip_ID { get; set; }

        [Required]
        [StringLength(20)]
        public string PassNo { get; set; } // Identifies driver via Pass Number from SHReportPortal

        [StringLength(100)]
        public string DriverName { get; set; } // Driver Name for verification/display

        [Required]
        public DateTime Work_Date { get; set; }

        [Required]
        [StringLength(20)]
        public string Terminal_Area { get; set; } // 'Within PSA' or 'Mega Port'

        public int SM20_Count { get; set; } = 0;

        public int DM20_Count { get; set; } = 0;

        public int Forty_Footer_Count { get; set; } = 0;
        public decimal Daily_Work_Hours { get; set; } = 0;
        public bool Is_WorkDay { get; set; } = true;

        public bool Is_PH { get; set; } = false; // Flag for Public Holiday/Sunday

        public string ShiftType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}