using GreenBowlApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;


namespace GreenBowlApp.Controllers
{
    // the autorize is used for autorization, so people must be logged in order to see it
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly string _connStr;

        public ProductsController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("OracleDb")!;
        }

        // method to display the products page
        public IActionResult Index(int? selectedId)
        {
            var productView = new Product();

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            // creating a connection to oracle to obatin attributes and SQL commands
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT product_id, product_name, allergens, hygiene_condition, expiry_date, batch_ctrl_batch_id
                    FROM product
                    ORDER BY product_name";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    productView.Products.Add(new Product
                    {
                        ProductId = Convert.ToInt32(reader["product_id"]),
                        ProductName = Convert.ToString(reader["product_name"]) ?? "",
                        Allergens = reader["allergens"] == DBNull.Value ? null : Convert.ToString(reader["allergens"]),
                        HygieneCondition = reader["hygiene_condition"] == DBNull.Value ? null : Convert.ToString(reader["hygiene_condition"]),
                        ExpiryDate = reader["expiry_date"] == DBNull.Value ? null : Convert.ToDateTime(reader["expiry_date"]),
                        BatchCtrlBatchId = Convert.ToString(reader["batch_ctrl_batch_id"]) ?? "",
                        BatchCount = 0, // later
                        PouchCount = 0  // later
                    });
                }
            }

            //this will display the batches made from the products when clicked
            if (productView.Products.Count > 0)
            {
                var chosen = productView.Products[0];

                if (selectedId.HasValue)
                {
                    var found = productView.Products.Find(p => p.ProductId == selectedId.Value);
                    if (found != null) chosen = found;
                }

                productView.SelectedProductId = chosen.ProductId;
                productView.SelectedProductName = chosen.ProductName;
            }

            return View(productView);
        }

        // method to create more products using a form
        public IActionResult Create()
        {
            return Content("");
        }
    }
}