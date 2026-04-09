using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ptc_IGH_Sys.Data;
using ptc_IGH_Sys.Helpers;          // KendoGridRequest, KendoGridResult
using ptc_IGH_Sys.Models;            // TripDriver, Department, PaymentScheme
using ptc_IGH_Sys.Models.IGH;
using ptc_IGH_Sys.Models.Shared;
using ptc_IGH_Sys.ViewModels.IGH;
using ptc_IGH_Sys.ViewModels.Master;
using System;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        #region Driver Master
        // GET: /Master/DriverMaster
        public IActionResult DriverMaster() => View();


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
                    //.Include(x => x.Department)
                    //.Include(x => x.PaymentScheme)
                    .AsNoTracking()
                    .AsQueryable();

                // Always filter DepartmentId = 5 and active = true by default, as per original code
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
                        CreatedAt = x.CreatedAt ?? DateTime.Now,
                        UpdatedAt = x.UpdatedAt ?? DateTime.Now

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
        public async Task<IActionResult> DriverMasterUpdate([FromForm] TripDriverViewModel viewModel)
        {
            _logger.LogInformation("=== DriverMasterUpdate START ===");

            try
            {
                // 1. Log all form values
                _logger.LogInformation("=== FORM VALUES RECEIVED ===");
                foreach (var key in Request.Form.Keys)
                {
                    _logger.LogInformation("Form[{Key}] = {Value}", key, Request.Form[key]);
                }

                // 2. Log ViewModel values
                _logger.LogInformation("=== VIEWMODEL VALUES ===");
                _logger.LogInformation(
                    "Id={Id}, Name={Name}, Mobile={Mobile}, Active={Active}, Allowance={Allowance}, " +
                    "License={License}, NRIC={NRIC}, Nationality={Nationality}, Shift={Shift}, JobTitle={JobTitle}",
                    viewModel.Id, viewModel.Name, viewModel.MobileNumber, viewModel.Active, viewModel.Allowance,
                    viewModel.LicenseNumber, viewModel.NRIC, viewModel.Nationality, viewModel.Shift, viewModel.JobTitle_Group
                );

                // 3. Clean ModelState (remove display-only fields)
                ModelState.Remove(nameof(TripDriverViewModel.DepartmentName));
                ModelState.Remove(nameof(TripDriverViewModel.PaymentSchemeName));
                ModelState.Remove(nameof(TripDriverViewModel.CreatedAt));
                ModelState.Remove(nameof(TripDriverViewModel.UpdatedAt));
                ModelState.Remove(nameof(TripDriverViewModel.Department_Id));
                ModelState.Remove(nameof(TripDriverViewModel.PaymentScheme_Id));

                // 4. Log ModelState errors
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("=== MODELSTATE ERRORS ===");
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Any())
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    foreach (var kvp in errors)
                    {
                        _logger.LogWarning("Field={Field}, Errors={Errors}", kvp.Key, string.Join("; ", kvp.Value));
                    }

                    return Json(new { Data = new[] { viewModel }, Total = 1, Errors = errors });
                }

                var debugRecord = await _db.TripDrivers
                .Where(x => x.Id == viewModel.Id)
                .Select(x => new { x.Id, x.JobTitle_Group, x.PaymentScheme_Id, x.Department_Id })
                .FirstOrDefaultAsync();

                _logger.LogInformation("DebugRecord: Id={Id}, JobTitle={Job}, Dept={Dept}, Scheme={Scheme}",
                    debugRecord?.Id, debugRecord?.JobTitle_Group ?? "<NULL>",
                    debugRecord?.Department_Id?.ToString() ?? "<NULL>",
                    debugRecord?.PaymentScheme_Id?.ToString() ?? "<NULL>");


                // 5. Lookup record
                var record = await _db.TripDrivers.FirstOrDefaultAsync(x => x.Id == viewModel.Id);
                if (record == null)
                {
                    _logger.LogWarning("Record #{Id} not found.", viewModel.Id);
                    return Json(KendoGridResult<TripDriverViewModel>.Error($"Record #{viewModel.Id} not found."));
                }

                _logger.LogInformation("=== RECORD BEFORE UPDATE ===");
                foreach (var prop in record.GetType().GetProperties())
                {
                    var val = prop.GetValue(record);
                    _logger.LogInformation("{Prop} = {Val}", prop.Name, val ?? "<NULL>");
                }

                // 6. Apply updates safely
                record.Name = viewModel.Name ?? string.Empty;
                record.LicenseNumber = viewModel.LicenseNumber;
                record.NRIC = viewModel.NRIC;
                record.MobileNumber = viewModel.MobileNumber ?? string.Empty;
                record.Active = viewModel.Active;
                record.Allowance = viewModel.Allowance;
                record.Nationality = viewModel.Nationality ?? string.Empty;
                record.Shift = viewModel.Shift ?? string.Empty;
                record.JobTitle_Group = viewModel.JobTitle_Group ?? string.Empty;

                // Hardcoded values
                record.Department_Id = 5L;
                record.PaymentScheme_Id = 10L;

                // Audit fields
                record.CreatedAt = viewModel.CreatedAt ?? DateTime.Now;
                record.UpdatedAt = DateTime.Now;

                // 7. Save changes
                _logger.LogInformation("=== SAVING CHANGES ===");
                await _db.SaveChangesAsync();

                // 8. Update ViewModel for return
                viewModel.UpdatedAt = record.UpdatedAt;

                _logger.LogInformation("Driver #{Id} updated successfully by {User}", viewModel.Id, User.Identity?.Name);
                _logger.LogInformation("=== DriverMasterUpdate END ===");

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
                viewModel.CreatedAt = newRecord.CreatedAt ?? DateTime.Now;
                viewModel.UpdatedAt = newRecord.UpdatedAt ?? DateTime.Now;

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

        #endregion


        #region RateTypeMaster
        /* ════════════════════════════════════════════════════════════
   GET: /Master/RateTypeMaster (view)
════════════════════════════════════════════════════════════ */
        public IActionResult RateTypeMaster() => View();

        /* ════════════════════════════════════════════════════════════
           POST: /Master/RateTypeMasterRead
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> RateTypeMasterRead(
            [FromForm] KendoGridRequest kendo,
            [FromForm] string rateDescription = null)
        {
            _logger.LogInformation("RateTypeMasterRead page={P} size={S} desc='{D}'",
                kendo?.Page, kendo?.PageSize, rateDescription);

            try
            {
                var query = _db.IGH_RateType_Master
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(rateDescription))
                {
                    var term = rateDescription.Trim().ToUpper();
                    query = query.Where(x => x.Rate_Description.ToUpper().Contains(term));
                }

                var result = await kendo.ToResultAsync(
                    query,
                    x => new IGH_RateType_MasterViewModel
                    {
                        RateType_ID = x.RateType_ID,
                        Rate_Description = x.Rate_Description,
                        Payment_Frequency = x.Payment_Frequency
                    },
                    defaultSort: "RateType_ID"
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RateTypeMasterRead failed");
                return Json(KendoGridResult<IGH_RateType_MasterViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /Master/RateTypeMasterUpdate
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> RateTypeMasterUpdate(
            [FromForm] IGH_RateType_MasterViewModel viewModel)
        {
            try
            {
                if (viewModel == null)
                    return Json(KendoGridResult<IGH_RateType_MasterViewModel>.Error("Invalid request."));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Any())
                        .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                    return Json(new { Data = new[] { viewModel }, Total = 1, Errors = errors });
                }

                var record = await _db.IGH_RateType_Master
                    .FirstOrDefaultAsync(x => x.RateType_ID == viewModel.RateType_ID);

                if (record == null)
                    return Json(KendoGridResult<IGH_RateType_MasterViewModel>.Error($"RateType_ID #{viewModel.RateType_ID} not found."));

                // Update fields
                record.Rate_Description = viewModel.Rate_Description;
                record.Payment_Frequency = viewModel.Payment_Frequency;

                await _db.SaveChangesAsync();

                _logger.LogInformation("RateType #{Id} updated by {User}", viewModel.RateType_ID, User.Identity?.Name);
                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RateTypeMasterUpdate failed Id={Id}", viewModel?.RateType_ID);
                return Json(KendoGridResult<IGH_RateType_MasterViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /Master/RateTypeMasterCreate
           NOTE: RateType_ID is provided manually by the user (1-9)
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> RateTypeMasterCreate(
            [FromForm] IGH_RateType_MasterViewModel viewModel)
        {
            try
            {
                if (viewModel == null || !ModelState.IsValid)
                    return Json(KendoGridResult<IGH_RateType_MasterViewModel>.Error("Invalid request data."));

                // Check if ID already exists
                var exists = await _db.IGH_RateType_Master.AnyAsync(x => x.RateType_ID == viewModel.RateType_ID);
                if (exists)
                    return Json(KendoGridResult<IGH_RateType_MasterViewModel>.Error($"RateType_ID {viewModel.RateType_ID} already exists. Please use a unique ID (1-9)."));

                var newRecord = new IGH_RateType_Master
                {
                    RateType_ID = viewModel.RateType_ID,
                    Rate_Description = viewModel.Rate_Description,
                    Payment_Frequency = viewModel.Payment_Frequency
                };

                _db.IGH_RateType_Master.Add(newRecord);
                await _db.SaveChangesAsync();

                _logger.LogInformation("RateType created Id={Id} by {User}", newRecord.RateType_ID, User.Identity?.Name);
                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RateTypeMasterCreate failed");
                return Json(KendoGridResult<IGH_RateType_MasterViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /Master/RateTypeMasterDestroy
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> RateTypeMasterDestroy(
            [FromForm] IGH_RateType_MasterViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    var record = await _db.IGH_RateType_Master
                        .FirstOrDefaultAsync(x => x.RateType_ID == viewModel.RateType_ID);
                    if (record != null)
                    {
                        _db.IGH_RateType_Master.Remove(record);
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("RateType #{Id} deleted by {User}", viewModel.RateType_ID, User.Identity?.Name);
                    }
                }
                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RateTypeMasterDestroy failed Id={Id}", viewModel?.RateType_ID);
                return Json(KendoGridResult<IGH_RateType_MasterViewModel>.Error(ex.Message));
            }
        }


        #endregion


        #region JobTitleRateMaster
        /* ════════════════════════════════════════════════════════════
   GET: /Master/JobTitleRateMaster
════════════════════════════════════════════════════════════ */
        public IActionResult JobTitleRateMaster() => View();

        /* ════════════════════════════════════════════════════════════
           POST: /Master/JobTitleRateMasterRead
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> JobTitleRateMasterRead(
            [FromForm] KendoGridRequest kendo,
            [FromForm] string jobTitleGroup = null,
            [FromForm] int? rateTypeId = null)
        {
            _logger.LogInformation("JobTitleRateMasterRead page={P} size={S} jobGroup='{J}' rateType={R}",
                kendo?.Page, kendo?.PageSize, jobTitleGroup, rateTypeId);

            try
            {
                var query = _db.IGH_JobTitleRate_Master
                    .Include(x => x.RateType)
                    .AsNoTracking()
                    .AsQueryable();


                foreach (var key in Request.Form.Keys)
                {
                    _logger.LogInformation("Form[{Key}] = {Value}", key, Request.Form[key]);
                }

                // DEBUG — print all ModelState errors
                _logger.LogInformation("=== MODELSTATE ERRORS ===");
                foreach (var kvp in ModelState)
                {
                    if (kvp.Value.Errors.Any())
                    {
                        foreach (var err in kvp.Value.Errors)
                        {
                            _logger.LogWarning("ModelState Field={Field} Error={Error} AttemptedValue={Val}",
                                kvp.Key, err.ErrorMessage, kvp.Value.AttemptedValue);
                        }
                    }
                }


                if (!string.IsNullOrWhiteSpace(jobTitleGroup))
                {
                    var term = jobTitleGroup.Trim().ToUpper();
                    query = query.Where(x => x.JobTitleGroup.ToUpper().Contains(term));
                }

                if (rateTypeId.HasValue && rateTypeId.Value > 0)
                    query = query.Where(x => x.RateTypeId == rateTypeId.Value);

                var result = await kendo.ToResultAsync(
                    query,
                    x => new IGH_JobTitleRate_MasterViewModel
                    {
                        JobRateId = x.JobRateId,
                        JobTitleGroup = x.JobTitleGroup,
                        RateTypeId = x.RateTypeId,
                        RateTypeDescription = x.RateType != null ? x.RateType.Rate_Description : string.Empty,
                        RateValue = x.RateValue,
                        Currency = x.Currency,
                        EffectiveDate = x.EffectiveDate
                    }
                    // defaultSort: "JobTitleGroup"
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobTitleRateMasterRead failed");
                // Return a more descriptive error to the client (only in development)
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                return Json(KendoGridResult<IGH_JobTitleRate_MasterViewModel>.Error(errorMsg));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /Master/JobTitleRateMasterUpdate
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> JobTitleRateMasterUpdate(
            [FromForm] IGH_JobTitleRate_MasterViewModel viewModel)
        {
            try
            {
                if (viewModel == null)
                    return Json(KendoGridResult<IGH_JobTitleRate_MasterViewModel>.Error("Invalid request."));

                foreach (var key in Request.Form.Keys)
                {
                    _logger.LogInformation("Form[{Key}] = {Value}", key, Request.Form[key]);
                }

                // DEBUG — print all ModelState errors
                _logger.LogInformation("=== MODELSTATE ERRORS ===");
                foreach (var kvp in ModelState)
                {
                    if (kvp.Value.Errors.Any())
                    {
                        foreach (var err in kvp.Value.Errors)
                        {
                            _logger.LogWarning("ModelState Field={Field} Error={Error} AttemptedValue={Val}",
                                kvp.Key, err.ErrorMessage, kvp.Value.AttemptedValue);
                        }
                    }
                }


                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Any())
                        .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                    return Json(new { Data = new[] { viewModel }, Total = 1, Errors = errors });
                }



                var record = await _db.IGH_JobTitleRate_Master
                    .FirstOrDefaultAsync(x => x.JobRateId == viewModel.JobRateId);

                if (record == null)
                    return Json(KendoGridResult<IGH_JobTitleRate_MasterViewModel>.Error($"Record #{viewModel.JobRateId} not found."));

                // Update fields
                record.JobTitleGroup = viewModel.JobTitleGroup;
                record.RateTypeId = viewModel.RateTypeId;
                record.RateValue = viewModel.RateValue;
                record.Currency = viewModel.Currency;
                record.EffectiveDate = viewModel.EffectiveDate;

                await _db.SaveChangesAsync();

                // Reload RateType description for response
                var rateType = await _db.IGH_RateType_Master.FindAsync(viewModel.RateTypeId);
                viewModel.RateTypeDescription = rateType?.Rate_Description ?? string.Empty;

                _logger.LogInformation("JobTitleRate #{Id} updated by {User}", viewModel.JobRateId, User.Identity?.Name);
                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobTitleRateMasterRead failed");
                var errorDetail = ex.InnerException?.Message ?? ex.Message;
                return Json(KendoGridResult<IGH_JobTitleRate_MasterViewModel>.Error(errorDetail));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /Master/JobTitleRateMasterCreate
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> JobTitleRateMasterCreate(
            [FromForm] IGH_JobTitleRate_MasterViewModel viewModel)
        {
            try
            {
                if (viewModel == null || !ModelState.IsValid)
                    return Json(KendoGridResult<IGH_JobTitleRate_MasterViewModel>.Error("Invalid request data."));

                var newRecord = new IGH_JobTitleRate_Master
                {
                    JobTitleGroup = viewModel.JobTitleGroup,
                    RateTypeId = viewModel.RateTypeId,
                    RateValue = viewModel.RateValue,
                    Currency = viewModel.Currency,
                    EffectiveDate = viewModel.EffectiveDate
                };

                _db.IGH_JobTitleRate_Master.Add(newRecord);
                await _db.SaveChangesAsync();

                viewModel.JobRateId = newRecord.JobRateId;
                var rateType = await _db.IGH_RateType_Master.FindAsync(viewModel.RateTypeId);
                viewModel.RateTypeDescription = rateType?.Rate_Description ?? string.Empty;

                _logger.LogInformation("JobTitleRate created Id={Id} by {User}", newRecord.JobRateId, User.Identity?.Name);
                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobTitleRateMasterCreate failed");
                return Json(KendoGridResult<IGH_JobTitleRate_MasterViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /Master/JobTitleRateMasterDestroy
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> JobTitleRateMasterDestroy(
            [FromForm] IGH_JobTitleRate_MasterViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    var record = await _db.IGH_JobTitleRate_Master
                        .FirstOrDefaultAsync(x => x.JobRateId == viewModel.JobRateId);
                    if (record != null)
                    {
                        _db.IGH_JobTitleRate_Master.Remove(record);
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("JobTitleRate #{Id} deleted by {User}", viewModel.JobRateId, User.Identity?.Name);
                    }
                }
                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobTitleRateMasterDestroy failed Id={Id}", viewModel?.JobRateId);
                return Json(KendoGridResult<IGH_JobTitleRate_MasterViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /Master/GetRateTypesForDropdown
           Used by Kendo DropDownLists to populate Rate Types
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> GetRateTypesForDropdown()
        {
            var rateTypes = await _db.IGH_RateType_Master
                .OrderBy(x => x.RateType_ID)
                .Select(x => new { RateType_ID = x.RateType_ID, Rate_Description = x.Rate_Description })
                .ToListAsync();
            return Json(new { Data = rateTypes });
        }

        #endregion

        #region IncentiveRateMaster
        /* ════════════════════════════════════════════════════════════
   GET: /Master/IncentiveRateMaster
════════════════════════════════════════════════════════════ */
        public IActionResult IncentiveRateMaster() => View();

        /* ════════════════════════════════════════════════════════════
           POST: /Master/IncentiveRateMasterRead
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> IncentiveRateMasterRead(
            [FromForm] KendoGridRequest kendo,
            [FromForm] string jobTitleGroup = null,
            [FromForm] int? rateTypeId = null,
            [FromForm] string tripType = null)
        {
            _logger.LogInformation("IncentiveRateMasterRead page={P} size={S} jobGroup='{J}' rateType={R} tripType='{T}'",
                kendo?.Page, kendo?.PageSize, jobTitleGroup, rateTypeId, tripType);

            try
            {
                var query = _db.IGH_Incentive_Rate_Master
                    .Include(x => x.RateType)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(jobTitleGroup))
                {
                    var term = jobTitleGroup.Trim().ToUpper();
                    query = query.Where(x => x.JobTitleGroup.ToUpper().Contains(term));
                }

                if (rateTypeId.HasValue && rateTypeId.Value > 0)
                    query = query.Where(x => x.RateTypeId == rateTypeId.Value);

                if (!string.IsNullOrWhiteSpace(tripType))
                {
                    var term = tripType.Trim().ToUpper();
                    query = query.Where(x => x.TripType != null && x.TripType.ToUpper().Contains(term));
                }

                var result = await kendo.ToResultAsync(
                    query,
                    x => new IGH_Incentive_Rate_MasterViewModel
                    {
                        IncentiveRateId = x.IncentiveRateId,
                        RateTypeId = x.RateTypeId,
                        RateTypeDescription = x.RateType != null ? x.RateType.Rate_Description : string.Empty,
                        JobTitleGroup = x.JobTitleGroup,
                        RateValue = x.RateValue,
                        Currency = x.Currency,
                        TripType = x.TripType ?? string.Empty,
                        TerminalArea = x.TerminalArea ?? string.Empty,
                        TripThresholdMin = x.TripThresholdMin ?? 0,
                        AssignmentType = x.AssignmentType ?? string.Empty,
                        EffectiveDate = x.EffectiveDate
                    },
                    defaultSort: "JobTitleGroup"
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IncentiveRateMasterRead failed");
                return Json(KendoGridResult<IGH_Incentive_Rate_MasterViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /Master/IncentiveRateMasterUpdate
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> IncentiveRateMasterUpdate(
            [FromForm] IGH_Incentive_Rate_MasterViewModel viewModel)
        {
            try
            {
                if (viewModel == null)
                    return Json(KendoGridResult<IGH_Incentive_Rate_MasterViewModel>.Error("Invalid request."));

                foreach (var key in Request.Form.Keys)
                {
                    _logger.LogInformation("Form[{Key}] = {Value}", key, Request.Form[key]);
                }


                _logger.LogInformation("=== IncentiveRateMaster MODELSTATE ERRORS ===");
                foreach (var kvp in ModelState)
                {
                    if (kvp.Value.Errors.Any())
                    {
                        foreach (var err in kvp.Value.Errors)
                        {
                            _logger.LogWarning("ModelState Field={Field} Error={Error} AttemptedValue={Val}",
                                kvp.Key, err.ErrorMessage, kvp.Value.AttemptedValue);
                        }
                    }
                }


                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Any())
                        .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                    return Json(new { Data = new[] { viewModel }, Total = 1, Errors = errors });
                }

                var record = await _db.IGH_Incentive_Rate_Master
                    .FirstOrDefaultAsync(x => x.IncentiveRateId == viewModel.IncentiveRateId);

                if (record == null)
                    return Json(KendoGridResult<IGH_Incentive_Rate_MasterViewModel>.Error($"Record #{viewModel.IncentiveRateId} not found."));

                // Update fields
                record.RateTypeId = viewModel.RateTypeId;
                record.JobTitleGroup = viewModel.JobTitleGroup;
                record.RateValue = viewModel.RateValue;
                record.Currency = viewModel.Currency;
                record.TripType = viewModel.TripType ?? "";
                record.TerminalArea = viewModel.TerminalArea ?? "";
                record.TripThresholdMin = viewModel.TripThresholdMin ?? 0;
                record.AssignmentType = viewModel.AssignmentType ?? "";
                record.EffectiveDate = viewModel.EffectiveDate ?? DateTime.Now;

                await _db.SaveChangesAsync();

                // Reload RateType description for response
                var rateType = await _db.IGH_RateType_Master.FindAsync(viewModel.RateTypeId);
                viewModel.RateTypeDescription = rateType?.Rate_Description ?? string.Empty;

                _logger.LogInformation("IncentiveRate #{Id} updated by {User}", viewModel.IncentiveRateId, User.Identity?.Name);
                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IncentiveRateMasterUpdate failed Id={Id}", viewModel?.IncentiveRateId);
                return Json(KendoGridResult<IGH_Incentive_Rate_MasterViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /Master/IncentiveRateMasterCreate
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> IncentiveRateMasterCreate(
            [FromForm] IGH_Incentive_Rate_MasterViewModel viewModel)
        {
            try
            {
                if (viewModel == null || !ModelState.IsValid)
                    return Json(KendoGridResult<IGH_Incentive_Rate_MasterViewModel>.Error("Invalid request data."));

                var newRecord = new IGH_Incentive_Rate_Master
                {
                    RateTypeId = viewModel.RateTypeId,
                    JobTitleGroup = viewModel.JobTitleGroup,
                    RateValue = viewModel.RateValue,
                    Currency = viewModel.Currency,
                    TripType = viewModel.TripType ?? "",
                    TerminalArea = viewModel.TerminalArea ?? "",
                    TripThresholdMin = viewModel.TripThresholdMin ?? 0,
                    AssignmentType = viewModel.AssignmentType ?? "",
                    EffectiveDate = viewModel.EffectiveDate ?? DateTime.Now

                };

                _db.IGH_Incentive_Rate_Master.Add(newRecord);
                await _db.SaveChangesAsync();

                viewModel.IncentiveRateId = newRecord.IncentiveRateId;
                var rateType = await _db.IGH_RateType_Master.FindAsync(viewModel.RateTypeId);
                viewModel.RateTypeDescription = rateType?.Rate_Description ?? string.Empty;

                _logger.LogInformation("IncentiveRate created Id={Id} by {User}", newRecord.IncentiveRateId, User.Identity?.Name);
                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IncentiveRateMasterCreate failed");
                return Json(KendoGridResult<IGH_Incentive_Rate_MasterViewModel>.Error(ex.Message));
            }
        }

        /* ════════════════════════════════════════════════════════════
           POST: /Master/IncentiveRateMasterDestroy
        ════════════════════════════════════════════════════════════ */
        [HttpPost]
        public async Task<IActionResult> IncentiveRateMasterDestroy(
            [FromForm] IGH_Incentive_Rate_MasterViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    var record = await _db.IGH_Incentive_Rate_Master
                        .FirstOrDefaultAsync(x => x.IncentiveRateId == viewModel.IncentiveRateId);
                    if (record != null)
                    {
                        _db.IGH_Incentive_Rate_Master.Remove(record);
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("IncentiveRate #{Id} deleted by {User}", viewModel.IncentiveRateId, User.Identity?.Name);
                    }
                }
                return Json(new { Data = new[] { viewModel }, Total = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IncentiveRateMasterDestroy failed Id={Id}", viewModel?.IncentiveRateId);
                return Json(KendoGridResult<IGH_Incentive_Rate_MasterViewModel>.Error(ex.Message));
            }
        }
        #endregion



    }
}