using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ptc_IGH_Sys.Models.IGH;
using ptc_IGH_Sys.ViewModels.IGH;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;


namespace ptc_IGH.Controllers
{
    public class IGHDELController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //    // ── DataSource read action ──────────────────────────────
        //    [HttpPost]
        //    public IActionResult GetGateActivity([DataSourceRequest] DataSourceRequest request)
        //    {
        //        // Sample static data — replace with your DB query
        //        var data = new List<GateActivityModel>
        //        {
        //            new GateActivityModel { Id = "GI-001", Type = "Gate In",  Vehicle = "TRK-2241", Driver = "Ahmad Bin Ismail", Time = "08:15 AM", Status = "Completed"   },
        //            new GateActivityModel { Id = "DO-089", Type = "Delivery", Vehicle = "VAN-1102", Driver = "Tan Chee Keong",  Time = "08:42 AM", Status = "In Progress"  },
        //            new GateActivityModel { Id = "GI-002", Type = "Gate In",  Vehicle = "TRK-3389", Driver = "Ravi Subramaniam",Time = "09:05 AM", Status = "Completed"   },
        //            new GateActivityModel { Id = "GO-041", Type = "Gate Out", Vehicle = "TRK-2241", Driver = "Ahmad Bin Ismail", Time = "09:30 AM", Status = "Completed"   },
        //            new GateActivityModel { Id = "VC-011", Type = "V.Check",  Vehicle = "VAN-0987", Driver = "System Check",    Time = "10:00 AM", Status = "Pending"      },
        //        };

        //        return Json(data.ToDataSourceResult(request));
        //    }

        //    // ── If reading from DB via EF ───────────────────────────
        //    // [HttpPost]
        //    // public IActionResult GetGateActivity([DataSourceRequest] DataSourceRequest request)
        //    // {
        //    //     var data = _db.GateActivities.Select(x => new GateActivityModel
        //    //     {
        //    //         Id      = x.Id,
        //    //         Type    = x.Type,
        //    //         Vehicle = x.Vehicle,
        //    //         Driver  = x.Driver,
        //    //         Time    = x.Time,
        //    //         Status  = x.Status
        //    //     });
        //    //     return Json(data.ToDataSourceResult(request));
        //    // }
        //}

        //// ── Model ───────────────────────────────────────────────────
        //public class GateActivityModel
        //{
        //    public string Id { get; set; }
        //    public string Type { get; set; }
        //    public string Vehicle { get; set; }
        //    public string Driver { get; set; }
        //    public string Time { get; set; }
        //    public string Status { get; set; }
        //}
    }
}
