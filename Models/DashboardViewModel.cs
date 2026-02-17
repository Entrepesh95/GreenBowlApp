using System.Collections.Generic;

namespace GreenBowlApp.Models

{
    //dashboard model to be able to get values in the controller anddisplay them in the view
    public class DashboardViewModel
    {
        public int TotalIngredients { get; set; }
        public int TotalProducts { get; set; }
        public int TotalBatches { get; set; }
        public int LowStockItems { get; set; }

        // Chart data ion the dashboard
        public List<string> IngredientNames { get; set; } = new();
        public List<double> AvailableAmounts { get; set; } = new(); 
        public List<double> UsedAmounts { get; set; } = new();      
    }
}
