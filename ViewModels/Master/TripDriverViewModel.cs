using System;
using System.ComponentModel.DataAnnotations;

namespace ptc_IGH_Sys.ViewModels.Master
{
    public class TripDriverViewModel
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? LicenseNumber { get; set; }
        public string? NRIC { get; set; }

        [Required]
        public string MobileNumber { get; set; }

        public bool Active { get; set; }
        public double Allowance { get; set; }
        public string? Nationality { get; set; }
        public string? Shift { get; set; }
        public string? JobTitle_Group { get; set; }

        public long? PaymentScheme_Id { get; set; }   // long? to match PaymentScheme.Id
        public long? Department_Id { get; set; }       // long? to match Department.Id

        // Display only — never validated
        public string? DepartmentName { get; set; }
        public string? PaymentSchemeName { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
    }
}