using System;

namespace GreenBowlApp.Models
{
    // ingredient model to pass data to the controllers and the view
    
    public class Ingredient
    {
        public int IngredientId { get; set; }

        public string? ItemName { get; set; }

        public string? LotNumber { get; set; }

        public double? Quantity { get; set; }

        public double? WeightPerBag { get; set; }

        public string? Location { get; set; }

        public string? CompanyMade { get; set; }

        public string? CountryOrigin { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public double? AmountUsed { get; set; }

        public double? AmountRemain { get; set; }

        
        public DateTime? DateReceived { get; set; }
    }
}