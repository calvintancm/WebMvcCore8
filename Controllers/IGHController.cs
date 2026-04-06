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
            _db = db;
            _logger = logger;
        }

        // ── Views ────────────────────────────────────────────────
        [HttpGet]
        public IActionResult LeaveTransaction() => View();

        [HttpGet]
        public IActionResult SalaryAdjustment() => View();

        [HttpGet]
        public IActionResult Smart() => View();

        // ─────────────────────────────────────────────────────────
        // POST: /IGH/LeaveTransactionRead
        //
        // WHY NO [FromBody] or [FromForm]:
        //   type: 'aspnetmvc-ajax' in JS causes Kendo to POST as
        //   application/x-www-form-urlencoded.
        //   ASP.NET Core MVC binds form fields to simple parameters
        //   (int?, string) automatically — no attribute needed.
        //
        // WHY [DataSourceRequest] works here:
        //   It reads paging/sorting/filtering from the form body,
        //   which Kendo posts alongside our custom leaveMonth etc.
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> LeaveTransactionRead(
            [DataSourceRequest] DataSourceRequest request,
            int? leaveMonth = null,
            int? leaveYear = null,
            string driverName = null)
        {
            _logger.LogInformation(
                "LeaveTransactionRead → month={M} year={Y} driver='{D}'",
                leaveMonth, leaveYear, driverName);

            try
            {
                var query = _db.IGH_Leave_Transactions
                    .AsNoTracking()
                    .AsQueryable();

                // ── Year filter ──
                if (leaveYear.HasValue && leaveYear.Value > 0)
                    query = query.Where(x => x.Leave_Date.Year == leaveYear.Value);

                // ── Month filter ──
                if (leaveMonth.HasValue && leaveMonth.Value > 0)
                    query = query.Where(x => x.Leave_Date.Month == leaveMonth.Value);

                // ── Driver name filter (case-insensitive) ──
                if (!string.IsNullOrWhiteSpace(driverName))
                {
                    var term = driverName.Trim().ToUpper();
                    query = query.Where(x =>
                        x.Driver_Name != null &&
                        x.Driver_Name.ToUpper().Contains(term));
                }

                var projected = query
                    .OrderBy(x => x.Driver_Name)
                    .Select(x => new LeaveTransactionViewModel
                    {
                        Id = x.Id,
                        DriverId = x.Driver_Id,
                        DriverName = x.Driver_Name ?? string.Empty,
                        LeaveDate = x.Leave_Date,
                        LeaveCount = x.Leave_Count,
                        LeaveType = x.Leave_Type ?? string.Empty,
                        LeaveMonth = x.Leave_Month,
                        Remarks = x.Remarks ?? string.Empty,
                        CreatedAt = x.Created_At
                    });

                var result = await projected.ToDataSourceResultAsync(request);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LeaveTransactionRead failed");
                ModelState.AddModelError("ServerError", ex.Message);
                return Json(Array.Empty<LeaveTransactionViewModel>()
                    .AsQueryable()
                    .ToDataSourceResult(request, ModelState));
            }
        }

        // ─────────────────────────────────────────────────────────
        // POST: /IGH/LeaveTransactionUpdate
        //
        // NO [FromBody] — Kendo posts update as form-urlencoded.
        // [FromBody] would break [DataSourceRequest] binding.
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> LeaveTransactionUpdate(
            [DataSourceRequest] DataSourceRequest request,
            LeaveTransactionViewModel viewModel)       // ← NO [FromBody]
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

                    record.Leave_Count = viewModel.LeaveCount ?? 0;
                    record.Leave_Type = viewModel.LeaveType;
                    record.Remarks = viewModel.Remarks;

                    await _db.SaveChangesAsync();

                    viewModel.DriverName = record.Driver_Name;
                    viewModel.CreatedAt = record.Created_At;
                    viewModel.LeaveDate = record.Leave_Date;
                    viewModel.LeaveMonth = record.Leave_Month;

                    _logger.LogInformation(
                        "LeaveTransaction #{Id} updated by {User}",
                        viewModel.Id, User.Identity?.Name);
                }

                return Json(new[] { viewModel }.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LeaveTransactionUpdate failed Id={Id}", viewModel?.Id);
                ModelState.AddModelError("", ex.Message);
                return Json(new[] { viewModel }.ToDataSourceResult(request, ModelState));
            }
        }

        // ─────────────────────────────────────────────────────────
        // POST: /IGH/LeaveTransactionCreate
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> LeaveTransactionCreate(
            [DataSourceRequest] DataSourceRequest request,
            LeaveTransactionViewModel viewModel)       // ← NO [FromBody]
        {
            try
            {
                if (viewModel != null && ModelState.IsValid)
                {
                    var newRecord = new IGH_Leave_Transaction
                    {
                        Driver_Id = viewModel.DriverId,
                        Driver_Name = viewModel.DriverName,
                        Leave_Date = viewModel.LeaveDate ?? DateTime.Today,
                        Leave_Count = viewModel.LeaveCount ?? 0,
                        Leave_Type = viewModel.LeaveType?.ToUpper() ?? string.Empty,
                        Leave_Month = viewModel.LeaveDate?.Month ?? DateTime.Today.Month,
                        Remarks = viewModel.Remarks,
                        Created_At = DateTime.Now
                    };

                    _db.IGH_Leave_Transactions.Add(newRecord);
                    await _db.SaveChangesAsync();

                    viewModel.Id = newRecord.Id;
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
        // POST: /IGH/LeaveTransactionDestroy
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> LeaveTransactionDestroy(
            [DataSourceRequest] DataSourceRequest request,
            LeaveTransactionViewModel viewModel)       // ← NO [FromBody]
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
                _logger.LogError(ex, "LeaveTransactionDestroy failed Id={Id}", viewModel?.Id);
                ModelState.AddModelError("", ex.Message);
                return Json(new[] { viewModel }.ToDataSourceResult(request, ModelState));
            }
        }
    }
}
