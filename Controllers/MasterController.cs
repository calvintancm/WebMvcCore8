using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ptc_IGH_Sys.Data;
using ptc_IGH_Sys.Helpers;          // KendoGridRequest, KendoGridResult
using ptc_IGH_Sys.Models;            // TripDriver, Department, PaymentScheme
using ptc_IGH_Sys.Models.Shared;
using ptc_IGH_Sys.ViewModels.Master;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ptc_IGH_Sys.Controllers
{
    [Authorize]
    public class MasterController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<MasterController> _logger;

        public MasterController(ApplicationDbContext db, ILogger<MasterController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: /Master/DriverMaster
        public IActionResult DriverMaster() => View();

        /* ════════════════════════════════════════════════════════════
           POST: /Master/DriverMasterRead
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> DriverMasterRead(
            [FromForm] KendoGridRequest kendo,
            [FromForm] string driverName = null,
            [FromForm] bool? activeOnly = null)
        {
            _logger.LogInformation("DriverMasterRead page={P} size={S} name='{N}' activeOnly={A}",
                kendo?.Page, kendo?.PageSize, driverName, activeOnly);

            try
            {
                var query = _db.TripDrivers
                    .Include(x => x.Department)
                    .Include(x => x.PaymentScheme)
                    .AsNoTracking()
                    .AsQueryable();

                // Always filter DepartmentId = 5
                query = query.Where(x => x.Department_Id == 5 && x.Active == true);


                if (!string.IsNullOrWhiteSpace(driverName))
                {
                    var term = driverName.Trim().ToUpper();
                    query = query.Where(x => x.Name.ToUpper().Contains(term));
                }

                if (activeOnly.HasValue && activeOnly.Value)
                    query = query.Where(x => x.Active == true);

                var result = await kendo.ToResultAsync(
                    query,
                    x => new TripDriverViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        LicenseNumber = x.LicenseNumber ?? string.Empty,
                        NRIC = x.NRIC ?? string.Empty,
                        MobileNumber = x.MobileNumber,
                        Active = x.Active,
                        Allowance = x.Allowance,
                        Nationality = x.Nationality ?? string.Empty,
                        Shift = x.Shift ?? string.Empty,
                        JobTitle_Group = x.JobTitle_Group ?? string.Empty,
                        //DepartmentName = x.Department != null ? x.Department.Description : string.Empty,
                        //PaymentSchemeName = x.PaymentScheme != null ? x.PaymentScheme.Description : string.Empty,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt
                    },
                    defaultSort: "Name"
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DriverMasterRead failed");
                return Json(KendoGridResult<TripDriverViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /Master/DriverMasterUpdate
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> DriverMasterUpdate(
            [FromForm] TripDriverViewModel viewModel)
        {
            try
            {
                if (viewModel == null)
                    return Json(KendoGridResult<TripDriverViewModel>.Error("Invalid request data."));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Any())
                        .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                    return Json(new { Data = new[] { viewModel }, Total = 1, Errors = errors });
                }

                var record = await _db.TripDrivers.FirstOrDefaultAsync(x => x.Id == viewModel.Id);
                if (record == null)
                    return Json(KendoGridResult<TripDriverViewModel>.Error($"Record #{viewModel.Id} not found."));

                // Update editable fields
                record.Name = viewModel.Name;
                record.LicenseNumber = viewModel.LicenseNumber;
                record.NRIC = viewModel.NRIC;
                record.MobileNumber = viewModel.MobileNumber;
                record.Active = viewModel.Active;
                record.Allowance = viewModel.Allowance;
                record.Nationality = viewModel.Nationality;
                record.Shift = viewModel.Shift;
                record.JobTitle_Group = viewModel.JobTitle_Group;
                // Note: Department and PaymentScheme are not updated here – use separate dropdowns if needed

                await _db.SaveChangesAsync();

                _logger.LogInformation("Driver #{Id} updated by {User}", viewModel.Id, User.Identity?.Name);
                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DriverMasterUpdate failed Id={Id}", viewModel?.Id);
                return Json(KendoGridResult<TripDriverViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /Master/DriverMasterCreate
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> DriverMasterCreate(
            [FromForm] TripDriverViewModel viewModel)
        {
            try
            {
                if (viewModel == null || !ModelState.IsValid)
                    return Json(KendoGridResult<TripDriverViewModel>.Error("Invalid request data."));

                var newRecord = new TripDriver
                {
                    Name = viewModel.Name,
                    LicenseNumber = viewModel.LicenseNumber,
                    NRIC = viewModel.NRIC,
                    MobileNumber = viewModel.MobileNumber,
                    Active = viewModel.Active,
                    Allowance = viewModel.Allowance,
                    Nationality = viewModel.Nationality,
                    Shift = viewModel.Shift,
                    JobTitle_Group = viewModel.JobTitle_Group,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                   // Department, PaymentScheme can be set later if needed
                };

                _db.TripDrivers.Add(newRecord);
                await _db.SaveChangesAsync();

                viewModel.Id = newRecord.Id;
                viewModel.CreatedAt = newRecord.CreatedAt;
                viewModel.UpdatedAt = newRecord.UpdatedAt;

                _logger.LogInformation("Driver created Id={Id} by {User}", newRecord.Id, User.Identity?.Name);
                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DriverMasterCreate failed");
                return Json(KendoGridResult<TripDriverViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /Master/DriverMasterDestroy
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> DriverMasterDestroy(
            [FromForm] TripDriverViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    var record = await _db.TripDrivers.FirstOrDefaultAsync(x => x.Id == viewModel.Id);
                    if (record != null)
                    {
                        _db.TripDrivers.Remove(record);
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("Driver #{Id} deleted by {User}", viewModel.Id, User.Identity?.Name);
                    }
                }
                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DriverMasterDestroy failed Id={Id}", viewModel?.Id);
                return Json(KendoGridResult<TripDriverViewModel>.Error(ex.Message));
            }
        }
    }
}