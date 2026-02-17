using GreenBowlApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;


namespace GreenBowlApp.Controllers
{
    
    [Authorize]
    public class IngredientsController : Controller
    {
        private readonly string _connStr;

        public IngredientsController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("OracleDb")!;
        }

        // getting ingredients attributes from oracle to be able to display them in the view
        public IActionResult Index()
        {
            var list = new List<Ingredient>();

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT ingredient_id, item_name, lot_number, quantity, weight_per_bag,
                       location, company_made, country_origin, expiry_date,
                       amount_used, amount_remain, date_recied
                FROM ingredients
                ORDER BY ingredient_id";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Ingredient
                {
                    IngredientId = Convert.ToInt32(reader["ingredient_id"]),
                    ItemName = Convert.ToString(reader["item_name"]) ?? "",
                    LotNumber = reader["lot_number"] == DBNull.Value ? null : Convert.ToString(reader["lot_number"]),
                    Quantity = reader["quantity"] == DBNull.Value ? null : Convert.ToDouble(reader["quantity"]),
                    WeightPerBag = reader["weight_per_bag"] == DBNull.Value ? null : Convert.ToDouble(reader["weight_per_bag"]),
                    Location = reader["location"] == DBNull.Value ? null : Convert.ToString(reader["location"]),
                    CompanyMade = reader["company_made"] == DBNull.Value ? null : Convert.ToString(reader["company_made"]),
                    CountryOrigin = reader["country_origin"] == DBNull.Value ? null : Convert.ToString(reader["country_origin"]),
                    ExpiryDate = reader["expiry_date"] == DBNull.Value ? null : Convert.ToDateTime(reader["expiry_date"]),
                    AmountUsed = reader["amount_used"] == DBNull.Value ? null : Convert.ToDouble(reader["amount_used"]),
                    AmountRemain = reader["amount_remain"] == DBNull.Value ? null : Convert.ToDouble(reader["amount_remain"]),
                    DateReceived = reader["date_recied"] == DBNull.Value ? null : Convert.ToDateTime(reader["date_recied"]),
                });
            }

            return View(list);
        }

        // this method calls the create view to add a new ingredient
        public IActionResult Create()
        {
            return View(new Ingredient());
        }

        // this will post the new ingredient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Ingredient ing)
        {
            if (string.IsNullOrWhiteSpace(ing.ItemName))
            {
                ModelState.AddModelError("ItemName", "Ingredient missing.");
            }

            if (!ModelState.IsValid)
                return View(ing);

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO ingredients
                (ingredient_id, item_name, lot_number, quantity, weight_per_bag,
                 location, company_made, country_origin, expiry_date,
                 amount_used, amount_remain, date_recied)
                VALUES
                (:ingredient_id, :item_name, :lot_number, :quantity, :weight_per_bag,
                 :location, :company_made, :country_origin, :expiry_date,
                 :amount_used, :amount_remain, :date_recied)";

            cmd.Parameters.Add(new OracleParameter("ingredient_id", ing.IngredientId));
            cmd.Parameters.Add(new OracleParameter("item_name", ing.ItemName));
            cmd.Parameters.Add(new OracleParameter("lot_number", (object?)ing.LotNumber ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("quantity", (object?)ing.Quantity ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("weight_per_bag", (object?)ing.WeightPerBag ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("location", (object?)ing.Location ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("company_made", (object?)ing.CompanyMade ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("country_origin", (object?)ing.CountryOrigin ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("expiry_date", (object?)ing.ExpiryDate ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("amount_used", (object?)ing.AmountUsed ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("amount_remain", (object?)ing.AmountRemain ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("date_recied", (object?)ing.DateReceived ?? DBNull.Value));

            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }

        // this will redirect to the Edit view to edit the ingredient
        public IActionResult Edit(int id)
        {
            var ing = GetIngredientById(id);
            if (ing == null) return NotFound();
            return View(ing);
        }

        // this will send out the edited ingredient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Ingredient ing)
        {
            if (string.IsNullOrWhiteSpace(ing.ItemName))
            {
                ModelState.AddModelError("ItemName", "Ingredient is misiing.");
            }

            if (!ModelState.IsValid)
                return View(ing);

            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE ingredients
                SET item_name = :item_name,
                    lot_number = :lot_number,
                    quantity = :quantity,
                    weight_per_bag = :weight_per_bag,
                    location = :location,
                    company_made = :company_made,
                    country_origin = :country_origin,
                    expiry_date = :expiry_date,
                    amount_used = :amount_used,
                    amount_remain = :amount_remain,
                    date_recied = :date_recied
                WHERE ingredient_id = :ingredient_id";

            cmd.Parameters.Add(new OracleParameter("item_name", ing.ItemName));
            cmd.Parameters.Add(new OracleParameter("lot_number", (object?)ing.LotNumber ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("quantity", (object?)ing.Quantity ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("weight_per_bag", (object?)ing.WeightPerBag ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("location", (object?)ing.Location ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("company_made", (object?)ing.CompanyMade ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("country_origin", (object?)ing.CountryOrigin ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("expiry_date", (object?)ing.ExpiryDate ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("amount_used", (object?)ing.AmountUsed ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("amount_remain", (object?)ing.AmountRemain ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("date_recied", (object?)ing.DateReceived ?? DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("ingredient_id", ing.IngredientId));

            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }

        // this will delete the ingredient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM ingredients WHERE ingredient_id = :id";
            cmd.Parameters.Add(new OracleParameter("id", id));

            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }

        // this will help us to obtain the ingredient by ID 
        private Ingredient? GetIngredientById(int id)
        {
            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT ingredient_id, item_name, lot_number, quantity, weight_per_bag,
                       location, company_made, country_origin, expiry_date,
                       amount_used, amount_remain, date_recied
                FROM ingredients
                WHERE ingredient_id = :id";
            cmd.Parameters.Add(new OracleParameter("id", id));

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new Ingredient
            {
                IngredientId = Convert.ToInt32(reader["ingredient_id"]),
                ItemName = Convert.ToString(reader["item_name"]) ?? "",
                LotNumber = reader["lot_number"] == DBNull.Value ? null : Convert.ToString(reader["lot_number"]),
                Quantity = reader["quantity"] == DBNull.Value ? null : Convert.ToDouble(reader["quantity"]),
                WeightPerBag = reader["weight_per_bag"] == DBNull.Value ? null : Convert.ToDouble(reader["weight_per_bag"]),
                Location = reader["location"] == DBNull.Value ? null : Convert.ToString(reader["location"]),
                CompanyMade = reader["company_made"] == DBNull.Value ? null : Convert.ToString(reader["company_made"]),
                CountryOrigin = reader["country_origin"] == DBNull.Value ? null : Convert.ToString(reader["country_origin"]),
                ExpiryDate = reader["expiry_date"] == DBNull.Value ? null : Convert.ToDateTime(reader["expiry_date"]),
                AmountUsed = reader["amount_used"] == DBNull.Value ? null : Convert.ToDouble(reader["amount_used"]),
                AmountRemain = reader["amount_remain"] == DBNull.Value ? null : Convert.ToDouble(reader["amount_remain"]),
                DateReceived = reader["date_recied"] == DBNull.Value ? null : Convert.ToDateTime(reader["date_recied"]),
            };
        }
    }
}