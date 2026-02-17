using GreenBowlApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;

namespace GreenBowlApp.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly string _connStr;

        public CategoriesController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("OracleDb")!;
        }

        public IActionResult Index()
        {
            return View();
        }

        // equipment controller for the categories section
        public IActionResult EquipmentControl()
        {
            var listEquip = new List<PreEquipmentModel>();

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            // creating a command in sql to obtain the data for the table
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    b.b_equip_id,
                    b.batch_ctrl_batch_id,
                    b.staff_employee_id,
                    s.full_name,
                    s.last_name,
                    b.start_time,
                    b.finish_time,
                    b.equipment_name,
                    b.condition_of_equipment,
                    b.action_taken,
                    b.gluten_test,
                    b.packaging_machine,
                    b.comments
                FROM batch_equip b
                LEFT JOIN staff s ON s.employee_id = b.staff_employee_id
                ORDER BY b.b_equip_id DESC";

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                // providing and converting the data from the database to fill the tabled
                listEquip.Add(new PreEquipmentModel
                {
                    EquipId = Convert.ToInt32(r["b_equip_id"]),
                    BatchId = r["batch_ctrl_batch_id"]?.ToString() ?? "",
                    EmployeeId = Convert.ToInt32(r["staff_employee_id"]),
                    EmployeeName = $"{r["full_name"]} {r["last_name"]}".Trim(),
                    StartTime = r["start_time"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(r["start_time"]).ToString("HH:mm"),

                                    FinishTime = r["finish_time"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(r["finish_time"]).ToString("HH:mm"),
                    EquipmentName = r["equipment_name"]?.ToString(),
                    ConditionOfEquipment = r["condition_of_equipment"]?.ToString(),
                    ActionTaken = r["action_taken"]?.ToString(),
                    GlutenTest = r["gluten_test"]?.ToString(),
                    PackagingMachine = r["packaging_machine"]?.ToString(),
                    Comments = r["comments"]?.ToString(),
                });
            }

            return View(listEquip);
        }

        
        // Packaging controller for categories

        public IActionResult PackagingControl()
            {
            var Packaginglist = new List<BatchPackagingModel>();

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    p.b_pack_id,
                    p.batch_ctrl_batch_id,
                    p.staff_employee_id,
                    s.full_name,
                    s.last_name,
                    p.production_date,
                    p.start_time_filling,
                    p.finish_time_filling,
                    p.time_checked,
                    p.hygienic_condition,
                    p.sealing_condition,
                    p.packaging_type,
                    p.qty_pouch_made,
                    p.comments
                FROM b_pack p
                LEFT JOIN staff s ON s.employee_id = p.staff_employee_id
                ORDER BY p.b_pack_id DESC";

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                Packaginglist.Add(new BatchPackagingModel
                {
                    PackId = Convert.ToInt32(r["b_pack_id"]),
                    BatchId = r["batch_ctrl_batch_id"]?.ToString() ?? "",
                    EmployeeId = Convert.ToInt32(r["staff_employee_id"]),
                    EmployeeName = $"{r["full_name"]} {r["last_name"]}".Trim(),

                    ProductionDate = r["production_date"] == DBNull.Value ? null : Convert.ToDateTime(r["production_date"]),
                    StartTimeFilling = r["start_time_filling"] == DBNull.Value ? null : Convert.ToDateTime(r["start_time_filling"]).ToString("HH:mm"),
                    FinishTimeFilling = r["finish_time_filling"] == DBNull.Value ? null : Convert.ToDateTime(r["finish_time_filling"]).ToString("HH:mm"),
                    TimeChecked = r["time_checked"] == DBNull.Value ? null : Convert.ToDateTime(r["time_checked"]).ToString("HH:mm"),

                    HygienicCondition = r["hygienic_condition"]?.ToString(),
                    SealingCondition = r["sealing_condition"]?.ToString(),
                    PackagingType = r["packaging_type"]?.ToString(),
                    QtyPouchMade = r["qty_pouch_made"] == DBNull.Value ? null : Convert.ToInt32(r["qty_pouch_made"]),
                    Comments = r["comments"]?.ToString()
                });
            }

            return View(Packaginglist);
        }

        // Processing controller for categories
        public IActionResult ProcessingControl()
        {
            var processlist = new List<BatchProcessingModel>();

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    p.bl_proces_id,
                    p.batch_ctrl_batch_id,
                    p.staff_employee_id,
                    s.full_name,
                    s.last_name,
                    p.production_date,
                    p.start_time,
                    p.finish_time,
                    p.time_checked,
                    p.alarm_autoclave,
                    p.reason_comment,
                    p.action_taken,
                    p.comments
                FROM b_proces p
                LEFT JOIN staff s ON s.employee_id = p.staff_employee_id
                ORDER BY p.bl_proces_id DESC";

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                processlist.Add(new BatchProcessingModel
                {
                    ProcessId = Convert.ToInt32(r["bl_proces_id"]),
                    BatchId = r["batch_ctrl_batch_id"]?.ToString() ?? "",

                    EmployeeId = Convert.ToInt32(r["staff_employee_id"]),
                    EmployeeName = $"{r["full_name"]} {r["last_name"]}".Trim(),

                    ProductionDate = r["production_date"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(r["production_date"]),

                    StartTime = r["start_time"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(r["start_time"]).ToString("HH:mm"),

                    FinishTime = r["finish_time"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(r["finish_time"]).ToString("HH:mm"),

                    TimeChecked = r["time_checked"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(r["time_checked"]).ToString("HH:mm"),

                    AlarmAutoclave = r["alarm_autoclave"]?.ToString(),
                    ReasonComment = r["reason_comment"]?.ToString(),
                    ActionTakenComment = r["action_taken"]?.ToString(),
                    Comments = r["comments"]?.ToString()
                });
            }

            return View(processlist);
        }

        // Xray controller for categories
        public IActionResult XRayMonitoring()
        {
            var list = new List<XRayModel>();

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    x.xray_id,
                    x.batch_ctrl_batch_id,
                    x.staff_employee_id,
                    s.full_name,
                    s.last_name,
                    x.start_time,
                    x.""date"",
                    x.end_time,
                    x.correction_action,
                    x.comments
                FROM xray_record x
                LEFT JOIN staff s ON s.employee_id = x.staff_employee_id
                ORDER BY x.xray_id DESC";

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new XRayModel
                {
                    XrayId = Convert.ToInt32(r["xray_id"]),
                    BatchId = r["batch_ctrl_batch_id"]?.ToString() ?? "",

                    EmployeeId = Convert.ToInt32(r["staff_employee_id"]),
                    EmployeeName = $"{r["full_name"]} {r["last_name"]}".Trim(),

                    Date = r["date"] == DBNull.Value ? null : Convert.ToDateTime(r["date"]),

                    StartTime = r["start_time"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(r["start_time"]).ToString("HH:mm"),

                    EndTime = r["end_time"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(r["end_time"]).ToString("HH:mm"),

                    CorrectionAction = r["correction_action"]?.ToString(),
                    Comments = r["comments"]?.ToString()
                });
            }

            return View(list);
        }

        // Batch controller for categories
        public IActionResult Batches()
        {
            var list = new List<BatchControlModel>();

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    b.batch_id,
                    b.batch_no,
                    b.production_date,
                    b.effective_date,
                    b.start_batch_time,
                    b.finish_batch_time,
                    b.batch_weight,
                    b.description,
                    b.staff_employee_id,
                    s.full_name,
                    s.last_name
                FROM batch_ctrl b
                LEFT JOIN staff s ON s.employee_id = b.staff_employee_id
                ORDER BY b.production_date DESC NULLS LAST, b.batch_id DESC";

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new BatchControlModel
                {
                    BatchId = r["batch_id"]?.ToString() ?? "",
                    BatchNo = r["batch_no"]?.ToString() ?? "",

                    ProductionDate = r["production_date"] == DBNull.Value ? null : Convert.ToDateTime(r["production_date"]),
                    EffectiveDate = r["effective_date"] == DBNull.Value ? null : Convert.ToDateTime(r["effective_date"]),

                    StartTime = r["start_batch_time"] == DBNull.Value ? null : Convert.ToDateTime(r["start_batch_time"]).ToString("HH:mm"),
                    FinishTime = r["finish_batch_time"] == DBNull.Value ? null : Convert.ToDateTime(r["finish_batch_time"]).ToString("HH:mm"),

                    BatchWeight = r["batch_weight"] == DBNull.Value ? null : Convert.ToDouble(r["batch_weight"]),
                    Description = r["description"]?.ToString(),

                    SupervisorEmployeeId = Convert.ToInt32(r["staff_employee_id"]),
                    SupervisorName = $"{r["full_name"]} {r["last_name"]}".Trim()
                });
            }

            return View(list);
        }
    }
}