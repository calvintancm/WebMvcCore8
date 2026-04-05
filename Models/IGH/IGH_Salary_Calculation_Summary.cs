using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ptc_IGH_Sys.Models.IGH
{
    [Table("IGH_Salary_Calculation_Summary")]
    public class IGH_Salary_Calculation_Summary
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Summary_ID")]
        public int SummaryId { get; set; }

        [Required]
        [Column("Driver_ID")]
        public long DriverId { get; set; }

        [Required]
        [Column("Pay_Period_Start")]
        public DateTime PayPeriodStart { get; set; }

        [Required]
        [Column("Pay_Period_End")]
        public DateTime PayPeriodEnd { get; set; }

        [Column("Processing_Date")]
        public DateTime? ProcessingDate { get; set; }

        // --- Identity Fields ---
        [Column("Driver_Name")] // DRIVER NAME
        public string DriverName { get; set; }

        [Column("Nationality")] // Nationality
        public string Nationality { get; set; }

        [Column("Master_Shift")] // MASTER SHIFT
        public string MasterShift { get; set; }

        // --- Shift & Day Count Fields ---
        [Column("Actual_Night_Shifts")] // ACTUAL NIGHT SHIFTS
        public int ActualNightShifts { get; set; }

        [Column("Tot_OffDay")] // TOT_OFFDAY
        public int TotOffDay { get; set; }

        [Column("Tot_WorkDays")] // TOT WORKDAYS
        public int TotWorkDays { get; set; }

        [Column("Leave_Days_JV")] // LEAVE DAYS (JV)
        public decimal LeaveDaysJV { get; set; }

        [Column("Paid_Leave_Days")] // PAID LEAVE DAYS
        public decimal PaidLeaveDays { get; set; }

        [Column("PH_Sunday_Count")] // PH / SUNDAY Count
        public int PHSundayCount { get; set; }

        // --- Earnings & Allowance Fields ---
        [Column("Basic_Salary")] // Basic Salary
        public decimal BasicSalary { get; set; }

        [Column("Night_Allowance")] // NIGHT ALLOWANCE
        public decimal NightAllowance { get; set; }

        [Column("MoreThan23Days")] // MORE>23Days
        public decimal MoreThan23Days { get; set; }

        [Column("Leave_Pay_KC")] // LEAVE PAY (KC)
        public decimal LeavePayKC { get; set; }

        [Column("PH_Allowance")] // PH ALLOWANCE
        public decimal PHAllowance { get; set; }

        [Column("Tot_Adjustment")] // TOT ADJUSTMENT
        public decimal TotAdjustment { get; set; }

        // --- PSA Trip Verification Fields ---
        [Column("PSA_SM20_Cnt")] // PSA SM20 CNT
        public int PSASM20Cnt { get; set; }

        [Column("PSA_SM20_Amt")] // PSA SM20 AMT
        public decimal PSASM20Amt { get; set; }

        [Column("PSA_DM20_Cnt")] // PSA DM20 CNT
        public int PSADM20Cnt { get; set; }

        [Column("PSA_DM20_Amt")] // PSA DM20 AMT
        public decimal PSADM20Amt { get; set; }

        [Column("PSA_40FT_Cnt")] // PSA 40FT CNT
        public int PSA40FTCnt { get; set; }

        [Column("PSA_40FT_Amt")] // PSA 40FT AMT
        public decimal PSA40FTAmt { get; set; }

        // --- TUAS Trip Verification Fields ---
        [Column("TUAS_SM20_Cnt")] // TUAS SM20 CNT
        public int TUASSM20Cnt { get; set; }

        [Column("TUAS_SM20_Amt")] // TUAS SM20 AMT
        public decimal TUASSM20Amt { get; set; }

        [Column("TUAS_DM20_Cnt")] // TUAS DM20 CNT
        public int TUASDM20Cnt { get; set; }

        [Column("TUAS_DM20_Amt")] // TUAS DM20 AMT
        public decimal TUASDM20Amt { get; set; }

        [Column("TUAS_40FT_Cnt")] // TUAS 40FT CNT
        public int TUAS40FTCnt { get; set; }

        [Column("TUAS_40FT_Amt")] // TUAS 40FT AMT
        public decimal TUAS40FTAmt { get; set; }

        // --- Grand Totals ---
        [Column("Incentive_Total")] // INCENTIVE TOTAL
        public decimal IncentiveTotal { get; set; }

        [Column("Tot_G_Wages")] // TOT G.WAGES
        public decimal TotGWages { get; set; }
    }
}