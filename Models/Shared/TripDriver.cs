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
        public virtual string Name { get; set; }

        [StringLength(20)]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Alphanumeric only!")]
        public virtual string LicenseNumber { get; set; }

        
        [StringLength(20)]
        public virtual string NRIC { get; set; }

        [StringLength(20)]
        [Required]
        public virtual string MobileNumber { get; set; }

        public virtual bool Active { get; set; }

        public virtual List<TripDriverAttribute> Attributes { get; set; }

        public virtual double Allowance { get; set; }

        /*Custom*/
        [UIHint("PaymentSchemeEditor")]
        public virtual PaymentScheme PaymentScheme { get; set; }

        [UIHint("DepartmentEditor")]
        public virtual Department Department { get; set; }
        public virtual string Nationality { get; set; }
        public virtual string Shift { get; set; }
        public virtual string JobTitle_Group { get; set; }

    }
}
