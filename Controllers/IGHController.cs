using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ptc_IGH_Sys.Data;
using ptc_IGH_Sys.Models.IGH;
using ptc_IGH_Sys.ViewModels.IGH;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ptc_IGH_Sys.Controllers
{
    [Authorize]
    public class IGHController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<IGHController> _logger;

        public IGHController(ApplicationDbContext db, ILogger<IGHController> logger)
        {
            _db     = db;
            _logger = logger;
        }

       
        [HttpGet]
        public IActionResult LeaveTransaction()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SalaryAdjustment()
        {
            return View();
        }

     
        public IActionResult Smart()
        {
            return View(); // This will look for Views/IGH/Smart.cshtml

        }

        [HttpPost]
        public async Task<IActionResult> LeaveTransactionRead(
            [DataSourceRequest] DataSourceRequest request,
            int?   leaveMonth  = null,
            int?   leaveYear   = null,
            string driverName  = null)
        {
            try
            {
                var query = _db.IGH_Leave_Transactions.AsNoTracking().AsQueryable();

              
                if (leaveYear.HasValue)
                {
                    query = query.Where(x => x.Leave_Date.Year == leaveYear.Value);
                }

             
                if (leaveMonth.HasValue && leaveMonth.Value > 0)
                {
                    query = query.Where(x => x.Leave_Date.Month == leaveMonth.Value);
                }

            
                if (!string.IsNullOrWhiteSpace(driverName))
                {
                    var search = driverName.Trim().ToUpper();
                    query = query.Where(x =>
                        x.Driver_Name != null &&
                        x.Driver_Name.ToUpper().Contains(search));
                }

           
                var projected = query
                    .OrderBy(x => x.Driver_Name)
                    .Select(x => new LeaveTransactionViewModel
                    {
                        Id         = x.Id,
                        DriverId   = x.Driver_Id,
                        DriverName = x.Driver_Name,
                        LeaveDate  = x.Leave_Date,
                        LeaveCount = x.Leave_Count,
                        LeaveType  = x.Leave_Type,
                        LeaveMonth = x.Leave_Month,
                        Remarks    = x.Remarks,
                        CreatedAt  = x.Created_At
                    });

                // NOTE: ToDataSourceResultAsync handles server-side paging/sorting
                var result = await projected.ToDataSourceResultAsync(request);

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LeaveTransactionRead failed");
                ModelState.AddModelError("ServerError", ex.Message);

                var empty = Array.Empty<LeaveTransactionViewModel>()
                    .AsQueryable()
                    .ToDataSourceResult(request, ModelState);

                return Json(empty);
            }
        }

        // ─────────────────────────────────────────────────────────
        // POST: /IGH/LeaveTransactionUpdate
        // Called by Kendo Grid inline edit save
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> LeaveTransactionUpdate(
            [DataSourceRequest] DataSourceRequest request,
            [FromBody] LeaveTransactionViewModel viewModel)
        {
            try
            {
                if (viewModel != null && ModelState.IsValid)
                {
                    var record = await _db.IGH_Leave_Transactions
                        .FirstOrDefaultAsync(x => x.Id == viewModel.Id);

                    if (record == null)
                    {
                        ModelState.AddModelError("", $"Leave record #{viewModel.Id} not found.");
                        return Json(new[] { viewModel }.ToDataSourceResult(request, ModelState));
                    }

                    // Only update the editable fields
                    record.Leave_Count = viewModel.LeaveCount;
                    record.Leave_Type  = viewModel.LeaveType;
                    record.Remarks     = viewModel.Remarks;
                   // record.Updated_At  = DateTime.Now;
                   // record.Updated_By  = User.Identity?.Name ?? "SYSTEM";

                    await _db.SaveChangesAsync();

                    // Return the full row (including read-only fields) back to the grid
                    viewModel.DriverName = record.Driver_Name;
                    viewModel.CreatedAt  = record.Created_At;
                    viewModel.LeaveDate  = record.Leave_Date;
                    viewModel.LeaveMonth = record.Leave_Month;

                    _logger.LogInformation(
                        "LeaveTransaction #{Id} updated by {User}",
                        viewModel.Id, User.Identity?.Name);
                }

                return Json(new[] { viewModel }.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LeaveTransactionUpdate failed for Id={Id}", viewModel?.Id);
                ModelState.AddModelError("", ex.Message);
                return Json(new[] { viewModel }.ToDataSourceResult(request, ModelState));
            }
        }

        // ─────────────────────────────────────────────────────────
        // POST: /IGH/LeaveTransactionCreate   (optional — for Add)
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> LeaveTransactionCreate(
            [DataSourceRequest] DataSourceRequest request,
            [FromBody] LeaveTransactionViewModel viewModel)
        {
            try
            {
                if (viewModel != null && ModelState.IsValid)
                {
                    var newRecord = new IGH_Leave_Transaction
                    {
                        Driver_Id   = viewModel.DriverId,
                        Driver_Name = viewModel.DriverName,
                        Leave_Date  = viewModel.LeaveDate,
                        Leave_Count = viewModel.LeaveCount,
                        Leave_Type  = viewModel.LeaveType,
                        Leave_Month = viewModel.LeaveDate.Month,
                        Remarks     = viewModel.Remarks,
                        Created_At  = DateTime.Now,
                        //Updated_By  = User.Identity?.Name ?? "SYSTEM"
                    };

                    _db.IGH_Leave_Transactions.Add(newRecord);
                    await _db.SaveChangesAsync();

                    viewModel.Id        = newRecord.Id;
                    viewModel.CreatedAt = newRecord.Created_At;
                    viewModel.LeaveMonth = newRecord.Leave_Month;

                    _logger.LogInformation(
                        "LeaveTransaction created Id={Id} by {User}",
                        newRecord.Id, User.Identity?.Name);
                }

                return Json(new[] { viewModel }.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LeaveTransactionCreate failed");
                ModelState.AddModelError("", ex.Message);
                return Json(new[] { viewModel }.ToDataSourceResult(request, ModelState));
            }
        }

        // ─────────────────────────────────────────────────────────
        // POST: /IGH/LeaveTransactionDestroy  (optional — for delete)
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> LeaveTransactionDestroy(
            [DataSourceRequest] DataSourceRequest request,
            [FromBody] LeaveTransactionViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    var record = await _db.IGH_Leave_Transactions
                        .FirstOrDefaultAsync(x => x.Id == viewModel.Id);

                    if (record != null)
                    {
                        _db.IGH_Leave_Transactions.Remove(record);
                        await _db.SaveChangesAsync();

                        _logger.LogInformation(
                            "LeaveTransaction #{Id} deleted by {User}",
                            viewModel.Id, User.Identity?.Name);
                    }
                }

                return Json(new[] { viewModel }.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LeaveTransactionDestroy failed for Id={Id}", viewModel?.Id);
                ModelState.AddModelError("", ex.Message);
                return Json(new[] { viewModel }.ToDataSourceResult(request, ModelState));
            }
        }
    }
}
