using GreenBowlApp.Models;
using GreenBowlApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Text.Json;


namespace GreenBowlApp.Controllers
{
    [Authorize]
    public class AddController : Controller
    {
        private readonly string _connStr;

        public AddController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("OracleDb")!;
        }

        // main index page
        public IActionResult Index(int? selectedId)
        {
            var products = new List<Product>();

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT product_id, product_name FROM product ORDER BY product_name";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    ProductId = Convert.ToInt32(reader["product_id"]),
                    ProductName = Convert.ToString(reader["product_name"]) ?? ""
                });
            }

            ViewBag.SelectedId = selectedId ?? 0;
            return View(products);
        }

        // page for adding a new batch
        public IActionResult NewBatch(int productId)
        {
            var batchModel = new BatchControlModel();
            batchModel.ProductId = productId;

            using var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(_connStr);
            conn.Open();

            // slecting a product using sql query
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT product_name FROM product WHERE product_id = :id";
                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", productId));
                batchModel.ProductName = (cmd.ExecuteScalar()?.ToString()) ?? "";
            }

            // selecting employees using sql query
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT employee_id, full_name, last_name FROM staff ORDER BY employee_id";
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    batchModel.Supervisors.Add(new Staff
                    {
                        EmployeeId = System.Convert.ToInt32(r["employee_id"]),
                        Name = $"{r["full_name"]} {r["last_name"]}"
                    });
                }
            }

            // obataining ingredients for a product from databse
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT i.ingredient_id, i.item_name, i.lot_number, i.expiry_date
                    FROM relation_3 r
                    JOIN ingredients i ON i.ingredient_id = r.ingredients_ingredient_id
                    WHERE r.product_product_id = :pid
                    ORDER BY i.item_name";

                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pid", productId));

                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    //placing values to add the ingredients
                    batchModel.Ingredients.Add(new IngredientBatchModel
                    {
                        IngredientId = System.Convert.ToInt32(r["ingredient_id"]),
                        IngredientName = r["item_name"]?.ToString() ?? "",
                        LotNumber = r["lot_number"] == DBNull.Value ? null : r["lot_number"].ToString(),
                        ExpiryDate = r["expiry_date"] == DBNull.Value ? null : System.Convert.ToDateTime(r["expiry_date"]),
                        AmountPerBatchKg = 0
                    });
                }
            }

            
            batchModel.ProductionDate = DateTime.Today;
            batchModel.EffectiveDate = DateTime.Today;

            return View(batchModel);
        }

        // posting the new batch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewBatch(BatchControlModel postBatchModel)
        {
            
            if (string.IsNullOrWhiteSpace(postBatchModel.BatchNo))
                ModelState.AddModelError("BatchNo", "Batch No is required.");

            if (postBatchModel.SupervisorEmployeeId <= 0)
                ModelState.AddModelError("SupervisorEmployeeId", "Select a production supervisor.");

            if (!ModelState.IsValid)
            {
                
                return NewBatch(postBatchModel.ProductId);
            }

            using var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(_connStr);
            conn.Open();

            

            string batchId;

            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT NVL(MAX(TO_NUMBER(SUBSTR(batch_id, 2))), 0) + 1
                    FROM batch_ctrl";

                var nextNumber = Convert.ToInt32(cmd.ExecuteScalar());
                batchId = "B" + nextNumber;
            }

            
            DateTime? startDateTime = CombineDateAndTime(postBatchModel.ProductionDate, postBatchModel.StartBatchTime);
            DateTime? finishDateTime = CombineDateAndTime(postBatchModel.ProductionDate, postBatchModel.FinishBatchTime);

            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO batch_ctrl
                    (batch_id, batch_no, start_batch_time, finish_batch_time, production_date,
                     description, effective_date, batch_weight, staff_employee_id)
                    VALUES
                    (:batch_id, :batch_no, :start_t, :finish_t, :prod_date,
                     :descr, :eff_date, :weight, :staff_id)";

                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("batch_id", batchId));
                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("batch_no", postBatchModel.BatchNo));
                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("start_t", (object?)startDateTime ?? DBNull.Value));
                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("finish_t", (object?)finishDateTime ?? DBNull.Value));
                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("prod_date", (object?)postBatchModel.ProductionDate ?? DBNull.Value));
                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("descr", (object?)postBatchModel.Description ?? DBNull.Value));
                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("eff_date", (object?)postBatchModel.EffectiveDate ?? DBNull.Value));
                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("weight", (object?)postBatchModel.BatchWeight ?? DBNull.Value));
                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("staff_id", postBatchModel.SupervisorEmployeeId));

                cmd.ExecuteNonQuery();
            }

            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE product SET batch_ctrl_batch_id = :bid WHERE product_id = :pid";
                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("bid", batchId));
                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pid", postBatchModel.ProductId));
                cmd.ExecuteNonQuery();
            }

            
            TempData["BatchIngredients"] = System.Text.Json.JsonSerializer.Serialize(postBatchModel.Ingredients);
            return RedirectToAction("Packaging", new { batchId = batchId, productId = postBatchModel.ProductId });
        }

        // packaging page to display form
        public IActionResult Packaging(string batchId, int productId)
        {
            var vm = new BatchPackagingModel
            {
                BatchId = batchId,
                ProductId = productId,
                ProductionDate = DateTime.Today
            };

            using var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(_connStr);
            conn.Open();

            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT product_name FROM product WHERE product_id = :id";
                cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", productId));
                vm.ProductName = (cmd.ExecuteScalar()?.ToString()) ?? "";
            }

            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT employee_id, full_name, last_name FROM staff ORDER BY employee_id";
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    vm.Employees.Add(new Staff
                    {
                        EmployeeId = Convert.ToInt32(r["employee_id"]),
                        Name = $"{r["full_name"]} {r["last_name"]}"
                    });
                }
            }

            return View(vm);
        }

        // posting packaging form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Packaging(BatchPackagingModel postPackagModel)
        {
            
            if (postPackagModel.EmployeeId <= 0)
                ModelState.AddModelError("EmployeeId", "Select employee.");

            if (!ModelState.IsValid)
                return Packaging(postPackagModel.BatchId, postPackagModel.ProductId);

            
            TempData["PackagingVM"] = JsonSerializer.Serialize(postPackagModel);
            return RedirectToAction("Processing", new { batchId = postPackagModel.BatchId, productId = postPackagModel.ProductId });
        }

        // processing page and placing values into the page using sql query
        public IActionResult Processing(string batchId, int productId)
        {
            var processingModel = new BatchProcessingModel
            {
                BatchId = batchId,
                ProductId = productId,
                ProductionDate = DateTime.Today
            };

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT product_name FROM product WHERE product_id = :id";
                cmd.Parameters.Add(new OracleParameter("id", productId));
                processingModel.ProductName = (cmd.ExecuteScalar()?.ToString()) ?? "";
            }

            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT employee_id, full_name, last_name FROM staff ORDER BY employee_id";
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    processingModel.Employees.Add(new Staff
                    {
                        EmployeeId = Convert.ToInt32(r["employee_id"]),
                        Name = $"{r["full_name"]} {r["last_name"]}"
                    });
                }
            }

            return View(processingModel);
        }

        // posting processing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Processing(BatchProcessingModel vm)
        {
            if (vm.EmployeeId <= 0)
                ModelState.AddModelError("EmployeeId", "Select employee.");

            if (!ModelState.IsValid)
                return Processing(vm.BatchId, vm.ProductId);

            // temp data to save at the end of the processes form.
            TempData["ProcessingVM"] = JsonSerializer.Serialize(vm);
            return RedirectToAction("XRay", new { batchId = vm.BatchId, productId = vm.ProductId });
        }


       


        // pre equipment page
        public IActionResult PreEquipment(string? batchId)
            {
                var equipModel = new GreenBowlApp.Models.ViewModels.PreEquipmentModel();

                using var conn = new OracleConnection(_connStr);
                conn.Open();

                
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT batch_id, batch_no FROM batch_ctrl ORDER BY production_date DESC NULLS LAST";
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        var bid = r["batch_id"]?.ToString() ?? "";
                        var bno = r["batch_no"]?.ToString() ?? "";
                    equipModel.Batches.Add(new GreenBowlApp.Models.ViewModels.BatchOption
                        {
                            BatchId = bid,
                            Label = $"{bid} - {bno}"
                        });
                    }
                }

                
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT employee_id, full_name, last_name FROM staff ORDER BY employee_id";
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                    equipModel.Employees.Add(new Staff
                        {
                            EmployeeId = Convert.ToInt32(r["employee_id"]),
                            Name = $"{r["full_name"]} {r["last_name"]}"
                        });
                    }
                }

                
                if (!string.IsNullOrWhiteSpace(batchId))
                equipModel.BatchId = batchId;

            equipModel.Date = DateTime.Today;
                return View(equipModel);
            }


            // posting equipment values
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult PreEquipment(GreenBowlApp.Models.ViewModels.PreEquipmentModel vm)
            {
                if (string.IsNullOrWhiteSpace(vm.BatchId))
                    ModelState.AddModelError("BatchId", "Select a batch.");

                if (vm.EmployeeId <= 0)
                    ModelState.AddModelError("EmployeeId", "Select an employee.");

                if (string.IsNullOrWhiteSpace(vm.GlutenTest))
                    ModelState.AddModelError("GlutenTest", "Gluten test is required.");

                if (!ModelState.IsValid)
                    return PreEquipment(vm.BatchId);

                using var conn = new OracleConnection(_connStr);
                conn.Open();

                
                DateTime? startDateTime = CombineDateAndTime(vm.Date, vm.StartTime);
                DateTime? finishDateTime = CombineDateAndTime(vm.Date, vm.FinishTime);

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO batch_equip
                        (b_equip_id, start_time, finish_time, equipment_name, condition_of_equipment,
                         action_taken, gluten_test, packaging_machine, comments,
                         batch_ctrl_batch_id, staff_employee_id)
                        VALUES
                        (:id, :startt, :finisht, :ename, :cond,
                         :action, :gluten, :pmachine, :comments,
                         :batch, :staff)";

                    cmd.Parameters.Add(new OracleParameter("id", GetNextNumber(conn, "batch_equip", "b_equip_id")));
                    cmd.Parameters.Add(new OracleParameter("startt", (object?)startDateTime ?? DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("finisht", (object?)finishDateTime ?? DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("ename", (object?)vm.EquipmentName ?? DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("cond", (object?)vm.ConditionOfEquipment ?? DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("action", (object?)vm.ActionTaken ?? DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("gluten", vm.GlutenTest)); // NOT NULL
                    cmd.Parameters.Add(new OracleParameter("pmachine", (object?)vm.PackagingMachine ?? DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("comments", (object?)vm.Comments ?? DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("batch", vm.BatchId));
                    cmd.Parameters.Add(new OracleParameter("staff", vm.EmployeeId));

                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Pre-Op equipment form saved!";
                return RedirectToAction("Index", "Add"); 
            }

        // received ingredients page
        public IActionResult ReceivedIngredients()
        {
            var IngredientList = new List<GreenBowlApp.Models.ViewModels.ReceivedIngredientModel>();

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT r.received_id, r.ingredient_name, r.supplier_name, r.expiry_date,
                           r.quantity, r.unit_kg, r.damanged_bags, r.shipping_company,
                           r.staff_employee_id, r.date_received
                    FROM received_item r
                    ORDER BY r.received_id DESC";

                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    IngredientList.Add(new GreenBowlApp.Models.ViewModels.ReceivedIngredientModel
                    {
                        ReceivedId = Convert.ToInt32(r["received_id"]),
                        IngredientName = r["ingredient_name"]?.ToString(),
                        SupplierName = r["supplier_name"]?.ToString(),
                        ExpiryDate = r["expiry_date"] == DBNull.Value ? null : Convert.ToDateTime(r["expiry_date"]),
                        Quantity = r["quantity"] == DBNull.Value ? null : Convert.ToInt32(r["quantity"]),
                        UnitKg = r["unit_kg"] == DBNull.Value ? null : Convert.ToDouble(r["unit_kg"]),
                        DamagedBags = r["damanged_bags"] == DBNull.Value ? null : Convert.ToInt32(r["damanged_bags"]),
                        ShippingCompany = r["shipping_company"]?.ToString(),
                        StaffEmployeeId = Convert.ToInt32(r["staff_employee_id"]),
                        DateReceived = r["date_received"] == DBNull.Value ? null : Convert.ToDateTime(r["date_received"])
                    });
                }
            }

            return View(IngredientList);
        }


        // received ingredient create page
        public IActionResult ReceivedIngredientCreateForm()
        {
            var ingredientFormModel = new GreenBowlApp.Models.ViewModels.ReceivedIngredientModel();

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT employee_id, full_name, last_name FROM staff ORDER BY employee_id";
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    ingredientFormModel.Employees.Add(new Staff
                    {
                        EmployeeId = Convert.ToInt32(r["employee_id"]),
                        Name = $"{r["full_name"]} {r["last_name"]}"
                    });
                }
            }

            ingredientFormModel.DateReceived = DateTime.Today;
            return View(ingredientFormModel);
        }


        // posting the received ingredient from the create form page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReceivedIngredientCreateForm(GreenBowlApp.Models.ViewModels.ReceivedIngredientModel receivedIngrPost)
        {
            if (string.IsNullOrWhiteSpace(receivedIngrPost.IngredientName))
                ModelState.AddModelError("IngredientName", "Ingredient name is required.");

            if (receivedIngrPost.StaffEmployeeId <= 0)
                ModelState.AddModelError("StaffEmployeeId", "Select who received it.");

            if (!ModelState.IsValid)
                return ReceivedIngredientCreateForm();

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO received_item
                    (received_id, ingredient_name, supplier_name, expiry_date, quantity,
                     unit_kg, damanged_bags, shipping_company, staff_employee_id, date_received)
                    VALUES
                    (:id, :iname, :sname, :exp, :qty,
                     :unitkg, :damaged, :ship, :staff, :drec)";

                cmd.Parameters.Add(new OracleParameter("id", GetNextNumber(conn, "received_item", "received_id")));
                cmd.Parameters.Add(new OracleParameter("iname", receivedIngrPost.IngredientName));
                cmd.Parameters.Add(new OracleParameter("sname", (object?)receivedIngrPost.SupplierName ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("exp", (object?)receivedIngrPost.ExpiryDate ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("qty", (object?)receivedIngrPost.Quantity ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("unitkg", (object?)receivedIngrPost.UnitKg ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("damaged", (object?)receivedIngrPost.DamagedBags ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("ship", (object?)receivedIngrPost.ShippingCompany ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("staff", receivedIngrPost.StaffEmployeeId));
                cmd.Parameters.Add(new OracleParameter("drec", (object?)receivedIngrPost.DateReceived ?? DBNull.Value));

                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Received ingredient saved!";
            return RedirectToAction("ReceivedIngredients");
        }

        // equipment control page form
        public IActionResult EquipmentControlCreate()
        {
            var vm = new PreEquipmentModel
            {
                Date = DateTime.Today
            };

            return View(vm);
        }

        

        // Xray page 
        public IActionResult XRay(string batchId, int productId)
        {
            var XrayModel = new XRayModel
            {
                BatchId = batchId,
                ProductId = productId,
                Date = DateTime.Today
            };

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT product_name FROM product WHERE product_id = :id";
                cmd.Parameters.Add(new OracleParameter("id", productId));
                XrayModel.ProductName = (cmd.ExecuteScalar()?.ToString()) ?? "";
            }

            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT employee_id, full_name, last_name FROM staff ORDER BY employee_id";
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    XrayModel.Employees.Add(new Staff
                    {
                        EmployeeId = Convert.ToInt32(r["employee_id"]),
                        Name = $"{r["full_name"]} {r["last_name"]}"
                    });
                }
            }

            return View(XrayModel);
        }

        // Posting xray values into the table
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XRay(XRayModel XrayPostModel)
        {
            if (XrayPostModel.EmployeeId <= 0)
                ModelState.AddModelError("EmployeeId", "Select employee.");

            if (!ModelState.IsValid)
                return XRay(XrayPostModel.BatchId, XrayPostModel.ProductId);

            
            var packagingJson = TempData["PackagingVM"] as string;
            var processingJson = TempData["ProcessingVM"] as string;

            if (string.IsNullOrWhiteSpace(packagingJson) || string.IsNullOrWhiteSpace(processingJson))
                return Content("Missing previous step data. Please go back and complete steps 2 and 3 again.");

            var packVM = JsonSerializer.Deserialize<BatchPackagingModel>(packagingJson);
            var procVM = JsonSerializer.Deserialize<BatchProcessingModel>(processingJson);

            if (packVM == null || procVM == null)
                return Content("Could not read previous step data.");

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO b_pack
                    (b_pack_id, production_date, start_time_filling, finish_time_filling,
                     hygienic_condition, time_checked, sealing_condition, packaging_type,
                     qty_pouch_made, comments, staff_employee_id, batch_ctrl_batch_id)
                    VALUES
                    (:id, :prod, :startt, :finisht,
                     :hyg, :checked, :seal, :ptype,
                     :qty, :comm, :staff, :batch)";

                cmd.Parameters.Add(new OracleParameter("id", GetNextNumber(conn, "b_pack", "b_pack_id")));
                cmd.Parameters.Add(new OracleParameter("prod", (object?)packVM.ProductionDate ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("startt", (object?)CombineDateAndTime(packVM.ProductionDate, packVM.StartTimeFilling) ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("finisht", (object?)CombineDateAndTime(packVM.ProductionDate, packVM.FinishTimeFilling) ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("hyg", (object?)packVM.HygienicCondition ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("checked", (object?)CombineDateAndTime(packVM.ProductionDate, packVM.TimeChecked) ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("seal", (object?)packVM.SealingCondition ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("ptype", (object?)packVM.PackagingType ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("qty", (object?)packVM.QtyPouchMade ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("comm", (object?)packVM.Comments ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("staff", packVM.EmployeeId));
                cmd.Parameters.Add(new OracleParameter("batch", XrayPostModel.BatchId));

                cmd.ExecuteNonQuery();
            }

            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO b_proces
                    (bl_proces_id, production_date, start_time, finish_time, alarm_autoclave,
                     time_checked, rejected_pouches, comments, reason_comment, action_taken,
                     batch_ctrl_batch_id, staff_employee_id)
                    VALUES
                    (:id, :prod, :startt, :finisht, :alarm,
                     :checked, :rej, :comm, :reason, :action,
                     :batch, :staff)";

                cmd.Parameters.Add(new OracleParameter("id", GetNextNumber(conn, "b_proces", "bl_proces_id")));
                cmd.Parameters.Add(new OracleParameter("prod", (object?)procVM.ProductionDate ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("startt", (object?)CombineDateAndTime(procVM.ProductionDate, procVM.StartTime) ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("finisht", (object?)CombineDateAndTime(procVM.ProductionDate, procVM.FinishTime) ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("alarm", (object?)procVM.AlarmAutoclave ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("checked", (object?)CombineDateAndTime(procVM.ProductionDate, procVM.TimeChecked) ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("rej", DBNull.Value)); // your UI doesn't include this; can add later
                cmd.Parameters.Add(new OracleParameter("comm", (object?)procVM.Comments ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("reason", (object?)procVM.ReasonComment ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("action", (object?)procVM.ActionTakenComment ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("batch", XrayPostModel.BatchId));
                cmd.Parameters.Add(new OracleParameter("staff", procVM.EmployeeId));

                cmd.ExecuteNonQuery();
            }

            //savinf data into the databsa
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO xray_record
                    (xray_id, start_time, ""date"", end_time, correction_action, comments,
                     batch_ctrl_batch_id, staff_employee_id)
                    VALUES
                    (:id, :startt, :datev, :endt, :corr, :comm,
                     :batch, :staff)";

                cmd.Parameters.Add(new OracleParameter("id", GetNextNumber(conn, "xray_record", "xray_id")));
                cmd.Parameters.Add(new OracleParameter("startt", (object?)CombineDateAndTime(XrayPostModel.Date, XrayPostModel.StartTime) ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("datev", (object?)XrayPostModel.Date ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("endt", (object?)CombineDateAndTime(XrayPostModel.Date, XrayPostModel.EndTime) ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("corr", (object?)XrayPostModel.CorrectionAction ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("comm", (object?)XrayPostModel.Comments ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("batch", XrayPostModel.BatchId));
                cmd.Parameters.Add(new OracleParameter("staff", XrayPostModel.EmployeeId));

                cmd.ExecuteNonQuery();
            }

            var ingredientsJson = TempData["BatchIngredients"] as string;

            if (!string.IsNullOrWhiteSpace(ingredientsJson))
            {
                var lines = JsonSerializer.Deserialize<List<IngredientBatchModel>>(ingredientsJson);

                if (lines != null)
                {
                    foreach (var line in lines)
                    {
                        if (line.AmountPerBatchKg <= 0) continue;

                        // INSERT INTO batch_ingredient_usage
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"INSERT INTO batch_ingredient_usage
                                (usage_id, batch_id, ingredient_id, allergens, hygienic_condition,
                                 lot_number, expiry_date, amount_used_kg)
                                VALUES
                                (:id, :batch, :ing, :allg, :hyg, :lot, :exp, :amt)";

                            cmd.Parameters.Add(new OracleParameter("id",
                                GetNextNumber(conn, "batch_ingredient_usage", "usage_id")));

                            cmd.Parameters.Add(new OracleParameter("batch", XrayPostModel.BatchId));
                            cmd.Parameters.Add(new OracleParameter("ing", line.IngredientId));
                            cmd.Parameters.Add(new OracleParameter("allg",
                                (object?)line.Allergens ?? DBNull.Value));
                            cmd.Parameters.Add(new OracleParameter("hyg",
                                (object?)line.HygienicCondition ?? DBNull.Value));
                            cmd.Parameters.Add(new OracleParameter("lot",
                                (object?)line.LotNumber ?? DBNull.Value));
                            cmd.Parameters.Add(new OracleParameter("exp",
                                (object?)line.ExpiryDate ?? DBNull.Value));
                            cmd.Parameters.Add(new OracleParameter("amt",
                                line.AmountPerBatchKg));

                            cmd.ExecuteNonQuery();
                        }

                        // Updating values
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                UPDATE ingredients
                                SET amount_used = NVL(amount_used,0) + :amt,
                                    amount_remain = NVL(amount_remain, quantity) - :amt
                                WHERE ingredient_id = :ing";

                            cmd.Parameters.Add(new OracleParameter("amt", line.AmountPerBatchKg));
                            cmd.Parameters.Add(new OracleParameter("ing", line.IngredientId));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            //return after all forms to the main dashaboard
            return RedirectToAction("Index", "Dashboard");




        }



        // geneting new ID number for the table
        private int GetNextNumber(OracleConnection conn, string table, string column)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT NVL(MAX({column}),0)+1 FROM {table}";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // combine date with a data time string
        private DateTime? CombineDateAndTime(DateTime? date, string? time)
        {
            if (date == null || string.IsNullOrWhiteSpace(time)) return null;

            
            if (!TimeSpan.TryParse(time, out var ts)) return null;

            return date.Value.Date.Add(ts);
        }



    }
}