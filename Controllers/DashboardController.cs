using GreenBowlApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;


namespace GreenBowlApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly string _connStr;

        public DashboardController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("OracleDb")!;
        }

        public IActionResult Index()
        {
            var vm = new DashboardViewModel();

            // connecting to the oracle databse
            using var conn = new OracleConnection(_connStr);
            conn.Open();

            // connection to the database to get the amount of ingrdts
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM ingredients";
                vm.TotalIngredients = System.Convert.ToInt32(cmd.ExecuteScalar());
            }

            // connection to the database to get the amount of prodcs
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM product";
                vm.TotalProducts = System.Convert.ToInt32(cmd.ExecuteScalar());
            }

            // connection to the database to get the amount of batches made
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM batch_ctrl";
                vm.TotalBatches = System.Convert.ToInt32(cmd.ExecuteScalar());
            }

            // calculating amount of ingredients to define low stock
            
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM ingredients WHERE NVL(amount_remain, 0) <= 10";
                vm.LowStockItems = System.Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Ingredient chart data to obatain direct info from databse
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT item_name,
                           NVL(amount_remain, 0) AS amount_remain,
                           NVL(amount_used, 0)   AS amount_used
                    FROM ingredients
                    ORDER BY item_name";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    vm.IngredientNames.Add(reader["item_name"].ToString() ?? "");
                    vm.AvailableAmounts.Add(System.Convert.ToDouble(reader["amount_remain"]));
                    vm.UsedAmounts.Add(System.Convert.ToDouble(reader["amount_used"]));
                }
            }

            return View(vm);
        }
    }
}