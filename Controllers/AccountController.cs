using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ptc_IGH_Sys.Models;
using System.Security.Claims;
using System.Text;

namespace ptc_IGH_Sys.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        // ← No more UserManager / SignInManager needed
        public AccountController(
            IHttpClientFactory httpClientFactory,
            ILogger<AccountController> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // -------------------------------------------------------
                // STEP 1: Call external API — this is the ONLY validation
                // -------------------------------------------------------
                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri("https://lcunity.ptclogistics.com.sg");

                var payload = new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        Username = model.UserName,
                        Password = model.Password
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("/ptcapi/api/common/authenticateUser", payload);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API auth failed. Status: {Code}, User: {User}",
                        response.StatusCode, model.UserName);
                    TempData["ErrorMessage"] = "Authentication service unavailable. Please try again later.";
                    return View(model);
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    _logger.LogError("Empty API response for user: {User}", model.UserName);
                    TempData["ErrorMessage"] = "Authentication failed. Please try again.";
                    return View(model);
                }

                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                if (result == null || result.success == null)
                {
                    _logger.LogError("Invalid API response structure for user: {User}", model.UserName);
                    TempData["ErrorMessage"] = "Authentication error. Please contact support.";
                    return View(model);
                }

                // -------------------------------------------------------
                // STEP 2: API says NO — show error
                // -------------------------------------------------------
                if (!(bool)result.success)
                {
                    string errorMessage = result.message != null
                        ? (string)result.message
                        : "Invalid username or password.";

                    _logger.LogWarning("Failed login for user: {User}. Reason: {Msg}",
                        model.UserName, errorMessage);

                    TempData["ErrorMessage"] = errorMessage;
                    return View(model);
                }

                // -------------------------------------------------------
                // STEP 3: API says YES — create cookie directly
                // No database, no AspNetUsers, no UserManager needed
                // -------------------------------------------------------
                int sessionMinutes = model.RememberMe ? 480 : 60;

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name,           model.UserName),
                    new(ClaimTypes.NameIdentifier, model.UserName),
                    new("LoginTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(sessionMinutes)
                    });

                // -------------------------------------------------------
                // STEP 4: Set session
                // -------------------------------------------------------
                HttpContext.Session.SetString("CurrentUser", model.UserName.ToUpper());
                HttpContext.Session.SetString("LoginTime",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                // -------------------------------------------------------
                // STEP 5: Write login log
                // -------------------------------------------------------
                try
                {
                    string logFolder = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                    if (!Directory.Exists(logFolder))
                        Directory.CreateDirectory(logFolder);

                    string logFilePath = Path.Combine(logFolder, "login-log.txt");
                    string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - " +
                                        $"{model.UserName} logged in. " +
                                        $"Session: {sessionMinutes} min.{Environment.NewLine}";
                    await System.IO.File.AppendAllTextAsync(logFilePath, logMessage);
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning("Failed to write login log for {User}: {Msg}",
                        model.UserName, logEx.Message);
                }

                _logger.LogInformation("User {User} logged in via API. Session: {Min} min.",
                    model.UserName, sessionMinutes);

                return RedirectToLocal(returnUrl);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error during auth for {User}", model.UserName);
                TempData["ErrorMessage"] = "Unable to connect to authentication service. Please try again later.";
                return View(model);
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(timeoutEx, "Auth timeout for {User}", model.UserName);
                TempData["ErrorMessage"] = "Authentication request timed out. Please try again.";
                return View(model);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON parse error for {User}", model.UserName);
                TempData["ErrorMessage"] = "Authentication error. Please contact support.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for {User}", model.UserName);
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return View(model);
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            _logger.LogInformation("User {User} logged out.", userName);
            return RedirectToAction("Login", "Account");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}