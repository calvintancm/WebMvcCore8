using System;
using System.ComponentModel.DataAnnotations;

namespace ptc_IGH_Sys.ViewModels.Master
{
    public class TripDriverViewModel
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? LicenseNumber { get; set; }
        public string? NRIC { get; set; }

        [Required]
        public string MobileNumber { get; set; } = string.Empty;

        public bool Active { get; set; }
        public double Allowance { get; set; }

        // These are now NOT NULL in DB, so make them non-nullable with defaults
        public string Nationality { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public string JobTitle_Group { get; set; } = string.Empty;
        public long? PaymentScheme_Id { get; set; }
        public long? Department_Id { get; set; }

        // Display-only fields
        public string? DepartmentName { get; set; }
        public string? PaymentSchemeName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
