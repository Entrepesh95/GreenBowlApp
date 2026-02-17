using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using GreenBowlApp.Models;

namespace GreenBowlApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly string _connStr;

        public LoginController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("OracleDb")!;
        }


        // Login page
        [HttpGet]
        public IActionResult Login()
        {
            return View(new Staff());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Staff vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // verification of the employee
            int employeeId = 0;
            string? fullName = null;
            string? role = null;
            string? dbPassword = null;

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT employee_id, full_name, last_name, employee_role, password
                    FROM staff
                    WHERE LOWER(email) = LOWER(:email)";

                cmd.Parameters.Add(new OracleParameter("email", vm.Email));

                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    employeeId = Convert.ToInt32(r["employee_id"]);
                    var fn = r["full_name"]?.ToString() ?? "";
                    var ln = r["last_name"]?.ToString() ?? "";
                    fullName = (fn + " " + ln).Trim();
                    role = r["employee_role"]?.ToString() ?? "Employee";
                    dbPassword = r["password"]?.ToString();
                }
            }

            if (employeeId == 0 || string.IsNullOrWhiteSpace(dbPassword))
            {
                vm.Error = "Invalid email or password.";
                return View(vm);
            }

            // 2the Bcryt was installed by Nugget to use protected password
            bool ok;
            if (dbPassword.StartsWith("$2")) 
                ok = BCrypt.Net.BCrypt.Verify(vm.Password, dbPassword);
            else
                ok = (vm.Password == dbPassword);

            if (!ok)
            {
                vm.Error = "Invalid email or password.";
                return View(vm);
            }

            // Creating cookies login
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employeeId.ToString()),
                new Claim(ClaimTypes.Name, fullName ?? vm.Email),
                new Claim(ClaimTypes.Email, vm.Email),
                new Claim(ClaimTypes.Role, role ?? "Employee")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }
    }
}