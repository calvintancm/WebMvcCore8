using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ptc_IGH_Sys.Data;
//using ptc_IGH_System.Data;
using System;
using System.Data;
using System.Threading.Tasks;

namespace ptc_IGH_Sys.Controllers
{
    [Authorize]
    public class SyncController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly SHReportDbContext _shdb;
        private readonly ILogger<SyncController> _logger;

        public SyncController(ApplicationDbContext db, SHReportDbContext shdb, ILogger<SyncController> logger)
        {
            _db = db;          
            _shdb = shdb;      
            _logger = logger;
        }

        public IActionResult ImportTripData()
        {
            return View();
        }

     
        [HttpPost]
        public async Task<IActionResult> RunImportTripData(DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (fromDate > toDate)
                    return Json(new { success = false, message = "From Date cannot be after To Date." });

                var fromParam = new SqlParameter("@FromDate", fromDate);
                var toParam = new SqlParameter("@ToDate", toDate);
                var driverParam = new SqlParameter("@DriverSearch", DBNull.Value);

                await _shdb.Database.ExecuteSqlRawAsync(
                    "EXEC sp_IGH_Export_DriverTripSummaryEx @FromDate, @ToDate, @DriverSearch",
                    fromParam, toParam, driverParam);

                _logger.LogInformation("ImportTripData executed from {From} to {To} by {User}",
                    fromDate, toDate, User.Identity?.Name);

                return Json(new
                {
                    success = true,
                    message = $"Trip data successfully imported for period {fromDate:dd-MMM-yyyy} to {toDate:dd-MMM-yyyy}."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RunImportTripData failed from {From} to {To}", fromDate, toDate);
                return Json(new { success = false, message = ex.Message });
            }
        }


        #region Wages
      
        public IActionResult GenerateMonthlyWages()
        {
            return View();
        }

     
        [HttpPost]
        public async Task<IActionResult> RunGenerateMonthlyWages(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                    return Json(new { success = false, message = "Start Date cannot be after End Date." });

                // Execute stored procedure (ignore DriverID parameter)
                var startParam = new SqlParameter("@StartDate", startDate);
                var endParam = new SqlParameter("@EndDate", endDate);
                // DriverID is passed as NULL (ignored)
                var driverIdParam = new SqlParameter("@DriverID", DBNull.Value);

                await _db.Database.ExecuteSqlRawAsync(
                    "EXEC sp_IGH_CalculateMonthlySalary @StartDate, @EndDate, @DriverID",
                    startParam, endParam, driverIdParam);

                _logger.LogInformation("GenerateMonthlyWages executed from {Start} to {End} by {User}",
                    startDate, endDate, User.Identity?.Name);

                return Json(new
                {
                    success = true,
                    message = $"Monthly wages successfully generated for period {startDate:dd-MMM-yyyy} to {endDate:dd-MMM-yyyy}."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RunGenerateMonthlyWages failed from {Start} to {End}", startDate, endDate);
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion 
    }
}