using System;

namespace ptc_IGH_Sys.ViewModels.Master
{
    public class TripDriverViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string LicenseNumber { get; set; }
        public string NRIC { get; set; }
        public string MobileNumber { get; set; }
        public bool Active { get; set; }
        public double Allowance { get; set; }
        public string Nationality { get; set; }
        public string Shift { get; set; }
        public string JobTitle_Group { get; set; }

        // For display only (read‑only in grid)
        public string DepartmentName { get; set; }
        public string PaymentSchemeName { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}