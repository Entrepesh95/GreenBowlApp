using System;
using System.Collections.Generic;

namespace GreenBowlApp.Models.ViewModels
{
    // batch model control for the controller and the view.
    public class BatchControlModel
    {
        
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";

        
        public string BatchNo { get; set; } = "";
        public int SupervisorEmployeeId { get; set; }

        public DateTime? ProductionDate { get; set; }
        public DateTime? EffectiveDate { get; set; }

        public string BatchId { get; set; } = "";          
        public string SupervisorName { get; set; } = "";   
        public string? StartTime { get; set; }             
        public string? FinishTime { get; set; }           

        
        public string? StartBatchTime { get; set; }
        public string? FinishBatchTime { get; set; }

        public double? BatchWeight { get; set; }
        public string? Description { get; set; }

        
        public List<Staff> Supervisors { get; set; } = new();

        
        public List<IngredientBatchModel> Ingredients { get; set; } = new();
    }

   

    public class IngredientBatchModel
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = "";

        
        public string? Allergens { get; set; }
        public string? HygienicCondition { get; set; }

        public string? LotNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public double AmountPerBatchKg { get; set; } = 0;
    }
}