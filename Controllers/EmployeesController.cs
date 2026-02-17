using GreenBowlApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;



namespace GreenBowlApp.Controllers
{
    [Authorize(Roles = "Manager")]
    public class EmployeesController : Controller
    {
        private readonly string _connStr;

        public EmployeesController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("OracleDb")!;
        }

        // GET: /Employees
        public IActionResult Index()
        {
            var list = new List<Staff>();
            int total = 0;

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            // getting employees from database
            using (var countCmd = conn.CreateCommand())
            {
                countCmd.CommandText = "SELECT COUNT(*) FROM staff";
                total = Convert.ToInt32(countCmd.ExecuteScalar());
            }

            // employee list
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT employee_id, full_name, last_name, email, password, employee_role
                    FROM staff
                    ORDER BY employee_id";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Staff
                    {
                        EmployeeId = Convert.ToInt32(reader["employee_id"]),
                        FullName = Convert.ToString(reader["full_name"]) ?? "",
                        LastName = Convert.ToString(reader["last_name"]) ?? "",
                        Email = reader["email"] == DBNull.Value ? null : Convert.ToString(reader["email"]),
                        Password = reader["password"] == DBNull.Value ? null : Convert.ToString(reader["password"]),
                        EmployeeRole = reader["employee_role"] == DBNull.Value ? null : Convert.ToString(reader["employee_role"]),
                    });
                }
            }

            ViewBag.TotalEmployees = total;
            return View(list);
        }

        // creating employees
        public IActionResult Create()
        {
            return View(new Staff());
        }

        // posting new employees
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Staff staff)
        {
            if (string.IsNullOrWhiteSpace(staff.FullName))
                ModelState.AddModelError("FullName", "First name is required.");

            if (string.IsNullOrWhiteSpace(staff.LastName))
                ModelState.AddModelError("LastName", "Last name is required.");

            if (!ModelState.IsValid)
                return View(staff);

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO staff (employee_id, full_name, last_name, email, password, employee_role)
                VALUES (:id, :fn, :ln, :em, :pw, :role)";

            cmd.Parameters.Add(new OracleParameter("id", staff.EmployeeId));
            cmd.Parameters.Add(new OracleParameter("fn", staff.FullName));
            cmd.Parameters.Add(new OracleParameter("ln", staff.LastName));
            cmd.Parameters.Add(new OracleParameter("em", (object?)staff.Email ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("pw", (object?)staff.Password ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("role", (object?)staff.EmployeeRole ?? DBNull.Value));

            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }

        // edting page 
        public IActionResult Edit(int id)
        {
            var staff = GetStaffById(id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        // postingthe employees form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Staff staff)
        {
            if (string.IsNullOrWhiteSpace(staff.FullName))
                ModelState.AddModelError("FullName", "First name is required.");

            if (string.IsNullOrWhiteSpace(staff.LastName))
                ModelState.AddModelError("LastName", "Last name is required.");

            if (!ModelState.IsValid)
                return View(staff);

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE staff
                SET full_name = :fn,
                    last_name = :ln,
                    email = :em,
                    password = :pw,
                    employee_role = :role
                WHERE employee_id = :id";

            cmd.Parameters.Add(new OracleParameter("fn", staff.FullName));
            cmd.Parameters.Add(new OracleParameter("ln", staff.LastName));
            cmd.Parameters.Add(new OracleParameter("em", (object?)staff.Email ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("pw", (object?)staff.Password ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("role", (object?)staff.EmployeeRole ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("id", staff.EmployeeId));

            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }

        // deleting the employee form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM staff WHERE employee_id = :id";
            cmd.Parameters.Add(new OracleParameter("id", id));

            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }

        // ---- helper method ----
        private Staff? GetStaffById(int id)
        {
            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT employee_id, full_name, last_name, email, password, employee_role
                FROM staff
                WHERE employee_id = :id";
            cmd.Parameters.Add(new OracleParameter("id", id));

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new Staff
            {
                EmployeeId = Convert.ToInt32(reader["employee_id"]),
                FullName = Convert.ToString(reader["full_name"]) ?? "",
                LastName = Convert.ToString(reader["last_name"]) ?? "",
                Email = reader["email"] == DBNull.Value ? null : Convert.ToString(reader["email"]),
                Password = reader["password"] == DBNull.Value ? null : Convert.ToString(reader["password"]),
                EmployeeRole = reader["employee_role"] == DBNull.Value ? null : Convert.ToString(reader["employee_role"]),
            };
        }
    }
}