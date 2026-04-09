using Microsoft.EntityFrameworkCore;
using ptc_IGH_Sys.Models.Custom;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ptc_IGH_Sys.Models.Shared
{
    [Table("UniTripDrivers")]
    [Index(nameof(Name), nameof(NRIC), IsUnique = true, Name = "NameAndDept")]
    public class TripDriver : Auditable
    {
        public virtual long Id { get; set; }

        [Required]
        [StringLength(50)]
        public virtual string Name { get; set; } = string.Empty;   // NOT NULL in DB

        [StringLength(20)]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Alphanumeric only!")]
        public virtual string? LicenseNumber { get; set; }         // nullable in DB

        [StringLength(20)]
        public virtual string? NRIC { get; set; }                  // nullable in DB

        [Required]
        [StringLength(20)]
        public virtual string MobileNumber { get; set; } = string.Empty; // NOT NULL in DB
        public virtual bool Active { get; set; }                   // NOT NULL in DB
        public virtual double Allowance { get; set; }              // NOT NULL in DB
        public long? PaymentScheme_Id { get; set; }                // nullable in DB
        public long? Department_Id { get; set; }                   // nullable in DB
        public virtual string Nationality { get; set; } = string.Empty;   // now NOT NULL
        public virtual string Shift { get; set; } = string.Empty;         // now NOT NULL
        public string? JobTitle_Group { get; set; } // now NOT NULL
    }
}

