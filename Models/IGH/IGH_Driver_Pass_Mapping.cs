using Microsoft.EntityFrameworkCore;
using ptc_IGH_Sys.Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
//using ptc_IGH_Sys.Models.Shared;

namespace ptc_IGH_Sys.Models.IGH
{
    [Table("IGH_Driver_Pass_Mapping")]
    [Index("UQ_PassNo", IsUnique = true)]

    public class IGH_Driver_Pass_Mapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Mapping_ID")]
        public int MappingId { get; set; }

        [Required]
        [Column("Driver_ID")]
        public long DriverId { get; set; }

        [Required]
        [StringLength(20)]
        [Column("PassNo")]
        
        public string PassNo { get; set; }

        [Column("Is_Primary")]
        public bool IsPrimary { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Property
        [ForeignKey("DriverId")]
        public virtual TripDriver Driver { get; set; }
    }
}