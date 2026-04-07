/*
 * IGHController.cs
 * ════════════════════════════════════════════════════════════════
 * Uses KendoGrid helper from Helpers/KendoGrid.cs
 * NO [DataSourceRequest] — works on .NET 8 forever
 * NO Telerik NuGet dependency for grid binding
 * ════════════════════════════════════════════════════════════════
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ptc_IGH_Sys.Data;
using ptc_IGH_Sys.Helpers;          // ← KendoGridRequest, KendoGridResult
using ptc_IGH_Sys.Models.IGH;
using ptc_IGH_Sys.ViewModels.IGH;
using System;
using System.Linq;
using System.Threading.Tasks;

/* ── REMOVED — no longer needed without [DataSourceRequest] ──
   using Kendo.Mvc;
   using Kendo.Mvc.Extensions;
   using Kendo.Mvc.UI;
   ─────────────────────────────────────────────────────────── */

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

        /* ── Views ──────────────────────────────────────────────── */
        [HttpGet] public IActionResult LeaveTransaction() => View();
        [HttpGet] public IActionResult SalaryAdjustment() => View();
        [HttpGet] public IActionResult Smart() => View();

        /* ════════════════════════════════════════════════════════════
           POST: /IGH/LeaveTransactionRead
           
           HOW IT WORKS:
           [FromForm] KendoGridRequest kendo  → receives page/pageSize/sort
           [FromForm] int? leaveMonth         → receives your custom filter
           [FromForm] int? leaveYear          → receives your custom filter
           [FromForm] string driverName       → receives your custom filter
           
           Kendo JS grid posts ALL of these together as form fields.
           parameterMap in JS merges them into one POST body.
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> LeaveTransactionRead(
            [FromForm] KendoGridRequest kendo,
            [FromForm] int? leaveMonth = null,
            [FromForm] int? leaveYear = null,
            [FromForm] string driverName = null)
        {
            _logger.LogInformation(
                "LeaveTransactionRead page={P} size={S} month={M} year={Y} driver='{D}'",
                kendo?.Page, kendo?.PageSize, leaveMonth, leaveYear, driverName);

            try
            {
                /* ── Build query with filters ── */
                var query = _db.IGH_Leave_Transactions
                    .AsNoTracking()
                    .AsQueryable();

                if (leaveYear.HasValue && leaveYear.Value > 0)
                    query = query.Where(x => x.Leave_Date.Year == leaveYear.Value);

                if (leaveMonth.HasValue && leaveMonth.Value > 0)
                    query = query.Where(x => x.Leave_Date.Month == leaveMonth.Value);

                if (!string.IsNullOrWhiteSpace(driverName))
                {
                    var term = driverName.Trim().ToUpper();
                    query = query.Where(x =>
                        x.Driver_Name != null &&
                        x.Driver_Name.ToUpper().Contains(term));
                }

                /* ── Project to ViewModel ── */
                var projected = query.Select(x => new LeaveTransactionViewModel
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

                /* ── Apply paging + sorting via global helper ── */
                /* defaultSort = "DriverName" means sort by DriverName asc   */
                /* unless grid sends its own sort instruction                 */
                var result = await kendo.ToResultAsync(
                    query,
                    x => new LeaveTransactionViewModel
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
                    },
                    defaultSort: "Driver_Name"
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LeaveTransactionRead failed");
                return Json(KendoGridResult<LeaveTransactionViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /IGH/LeaveTransactionUpdate
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> LeaveTransactionUpdate(
            [FromForm] LeaveTransactionViewModel viewModel)
        {
            try
            {
                if (viewModel == null)
                    return Json(KendoGridResult<LeaveTransactionViewModel>
                        .Error("Invalid request data."));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Any())
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                    return Json(new
                    {
                        Data = new[] { viewModel },
                        Total = 1,
                        Errors = errors
                    });
                }

                var record = await _db.IGH_Leave_Transactions
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Id);

                if (record == null)
                    return Json(KendoGridResult<LeaveTransactionViewModel>
                        .Error($"Record #{viewModel.Id} not found."));

                /* Update only editable fields */
                record.Leave_Count = viewModel.LeaveCount ?? 0;
                record.Leave_Type = viewModel.LeaveType;
                record.Remarks = viewModel.Remarks;

                await _db.SaveChangesAsync();

                /* Return updated values back to grid */
                viewModel.DriverName = record.Driver_Name;
                viewModel.CreatedAt = record.Created_At;
                viewModel.LeaveDate = record.Leave_Date;
                viewModel.LeaveMonth = record.Leave_Month;

                _logger.LogInformation(
                    "LeaveTransaction #{Id} updated by {User}",
                    viewModel.Id, User.Identity?.Name);

                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LeaveTransactionUpdate failed Id={Id}", viewModel?.Id);
                return Json(KendoGridResult<LeaveTransactionViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /IGH/LeaveTransactionCreate
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> LeaveTransactionCreate(
            [FromForm] LeaveTransactionViewModel viewModel)
        {
            try
            {
                if (viewModel == null || !ModelState.IsValid)
                    return Json(KendoGridResult<LeaveTransactionViewModel>
                        .Error("Invalid request data."));

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

                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LeaveTransactionCreate failed");
                return Json(KendoGridResult<LeaveTransactionViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /IGH/LeaveTransactionDestroy
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> LeaveTransactionDestroy(
            [FromForm] LeaveTransactionViewModel viewModel)
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

                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LeaveTransactionDestroy failed Id={Id}", viewModel?.Id);
                return Json(KendoGridResult<LeaveTransactionViewModel>.Error(ex.Message));
            }
        }




        /* ════════════════════════════════════════════════════════════
           POST: /IGH/SalaryAdjustmentRead
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> SalaryAdjustmentRead(
            [FromForm] KendoGridRequest kendo,
            [FromForm] int? adjustmentMonth = null,
            [FromForm] int? adjustmentYear = null,
            [FromForm] string driverName = null)
        {
            _logger.LogInformation(
                "SalaryAdjustmentRead page={P} size={S} month={M} year={Y} driver='{D}'",
                kendo?.Page, kendo?.PageSize, adjustmentMonth, adjustmentYear, driverName);

            try
            {
                var query = _db.IGH_Salary_Adjustment_Transaction
                    .AsNoTracking()
                    .AsQueryable();

                if (adjustmentYear.HasValue && adjustmentYear.Value > 0)
                    query = query.Where(x => x.AdjustmentDate.Year == adjustmentYear.Value);

                if (adjustmentMonth.HasValue && adjustmentMonth.Value > 0)
                    query = query.Where(x => x.AdjustmentDate.Month == adjustmentMonth.Value);

                if (!string.IsNullOrWhiteSpace(driverName))
                {
                    var term = driverName.Trim().ToUpper();
                    query = query.Where(x =>
                        x.DriverName != null &&
                        x.DriverName.ToUpper().Contains(term));
                }

                var result = await kendo.ToResultAsync(
                    query,
                    x => new SalaryAdjustmentViewModel
                    {
                        Id = x.Id,
                        DriverId = x.DriverId,
                        DriverName = x.DriverName ?? string.Empty,
                        AdjustmentDate = x.AdjustmentDate,
                        AdjustmentMonth = x.AdjustmentDate.Month,
                        AdjustmentType = x.AdjustmentType ?? string.Empty,
                        AdjustmentAmount = x.AdjustmentAmount,
                        Remarks = x.Remarks ?? string.Empty,
                        CreatedAt = x.CreatedAt
                    },
                    defaultSort: "DriverName"
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SalaryAdjustmentRead failed");
                return Json(KendoGridResult<SalaryAdjustmentViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /IGH/SalaryAdjustmentUpdate
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> SalaryAdjustmentUpdate(
            [FromForm] SalaryAdjustmentViewModel viewModel)
        {
            try
            {
                if (viewModel == null)
                    return Json(KendoGridResult<SalaryAdjustmentViewModel>
                        .Error("Invalid request data."));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Any())
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                    return Json(new
                    {
                        Data = new[] { viewModel },
                        Total = 1,
                        Errors = errors
                    });
                }

                var record = await _db.IGH_Salary_Adjustment_Transaction
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Id);

                if (record == null)
                    return Json(KendoGridResult<SalaryAdjustmentViewModel>
                        .Error($"Record #{viewModel.Id} not found."));

                // Update only editable fields
                record.AdjustmentType = viewModel.AdjustmentType;
                record.AdjustmentAmount = viewModel.AdjustmentAmount;
                record.Remarks = viewModel.Remarks;

                await _db.SaveChangesAsync();

                // Return updated values back to grid
                viewModel.DriverName = record.DriverName;
                viewModel.CreatedAt = record.CreatedAt;
                viewModel.AdjustmentDate = record.AdjustmentDate;
                viewModel.AdjustmentMonth = record.AdjustmentDate.Month;

                _logger.LogInformation(
                    "SalaryAdjustment #{Id} updated by {User}",
                    viewModel.Id, User.Identity?.Name);

                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SalaryAdjustmentUpdate failed Id={Id}", viewModel?.Id);
                return Json(KendoGridResult<SalaryAdjustmentViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /IGH/SalaryAdjustmentCreate
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> SalaryAdjustmentCreate(
            [FromForm] SalaryAdjustmentViewModel viewModel)
        {
            try
            {
                if (viewModel == null || !ModelState.IsValid)
                    return Json(KendoGridResult<SalaryAdjustmentViewModel>
                        .Error("Invalid request data."));

                var newRecord = new IGH_Salary_Adjustment_Transaction
                {
                    DriverId = viewModel.DriverId,
                    DriverName = viewModel.DriverName,
                    AdjustmentDate = viewModel.AdjustmentDate == default(DateTime)
                        ? DateTime.Today
                        : viewModel.AdjustmentDate,
                    AdjustmentType = viewModel.AdjustmentType?.ToUpper() ?? string.Empty,
                    AdjustmentAmount = viewModel.AdjustmentAmount,
                    Remarks = viewModel.Remarks,
                    CreatedAt = DateTime.Now
                };

                _db.IGH_Salary_Adjustment_Transaction.Add(newRecord);
                await _db.SaveChangesAsync();

                viewModel.Id = newRecord.Id;
                viewModel.CreatedAt = newRecord.CreatedAt;
                viewModel.AdjustmentMonth = newRecord.AdjustmentDate.Month;

                _logger.LogInformation(
                    "SalaryAdjustment created Id={Id} by {User}",
                    newRecord.Id, User.Identity?.Name);

                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SalaryAdjustmentCreate failed");
                return Json(KendoGridResult<SalaryAdjustmentViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /IGH/SalaryAdjustmentDestroy
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> SalaryAdjustmentDestroy(
            [FromForm] SalaryAdjustmentViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    var record = await _db.IGH_Salary_Adjustment_Transaction
                        .FirstOrDefaultAsync(x => x.Id == viewModel.Id);

                    if (record != null)
                    {
                        _db.IGH_Salary_Adjustment_Transaction.Remove(record);
                        await _db.SaveChangesAsync();

                        _logger.LogInformation(
                            "SalaryAdjustment #{Id} deleted by {User}",
                            viewModel.Id, User.Identity?.Name);
                    }
                }

                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SalaryAdjustmentDestroy failed Id={Id}", viewModel?.Id);
                return Json(KendoGridResult<SalaryAdjustmentViewModel>.Error(ex.Message));
            }
        }
    }
}
