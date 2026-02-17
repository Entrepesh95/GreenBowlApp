using System;

namespace GreenBowlApp.Models
{

// product model for products controller and product views
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";

        public string? Allergens { get; set; }
        public string? HygieneCondition { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public List<Product> Products { get; set; } = new();
        public int SelectedProductId { get; set; }
        public string SelectedProductName { get; set; } = "";

        public string BatchCtrlBatchId { get; set; } = ""; 

        
        public int BatchCount { get; set; } = 0;
        public int PouchCount { get; set; } = 0;
    }
}