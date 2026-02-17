using System;
using System.Collections.Generic;

namespace GreenBowlApp.Models.ViewModels
{
    public class ReceivedIngredientModel
    {
        // model for received ingredients
        public int ReceivedId { get; set; }  

        public string? IngredientName { get; set; }
        public string? SupplierName { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public int? Quantity { get; set; }
        public double? UnitKg { get; set; }
        public int? DamagedBags { get; set; }

        public string? ShippingCompany { get; set; }

        public int StaffEmployeeId { get; set; } 
        public DateTime? DateReceived { get; set; } = DateTime.Today;

        public List<Staff> Employees { get; set; } = new();
    }
}