using System;
using System.ComponentModel.DataAnnotations;

namespace ptc_IGH_Sys.ViewModels.IGH
{
    public class LeaveTransactionViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Driver is required")]
        public long DriverId { get; set; }

        public string DriverName { get; set; }

        [Required(ErrorMessage = "Leave Date is required")]
        public DateTime LeaveDate { get; set; }

        [Required(ErrorMessage = "Leave Count is required")]
        [Range(0.5, 365, ErrorMessage = "Leave Count must be between 0.5 and 365")]
        public decimal LeaveCount { get; set; }

        [Required(ErrorMessage = "Leave Type is required")]
        [StringLength(50)]
        public string LeaveType { get; set; }


        public int LeaveMonth { get; set; }

        [StringLength(500)]
        public string Remarks { get; set; }

        public DateTime? CreatedAt { get; set; }

        public void Update(ptc_IGH_Sys.Models.IGH.IGH_Leave_Transaction target)
        {
            target.Driver_Id = this.DriverId;
            target.Leave_Date = this.LeaveDate;
            target.Leave_Count = this.LeaveCount;
            target.Leave_Type = this.LeaveType?.ToUpper();
            target.Leave_Month = this.LeaveDate.Month;
            target.Remarks = this.Remarks;
        }
    }
}