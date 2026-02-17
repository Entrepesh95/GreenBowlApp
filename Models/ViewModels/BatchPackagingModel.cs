using GreenBowlApp.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace GreenBowlApp.Models.ViewModels
{
    public class BatchPackagingModel
    {
        // packaging model for the controller and the viewe and attributes from the databse
        
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string BatchId { get; set; } = "";

        public int PackId { get; set; }          
        public string EmployeeName { get; set; } = "";  

        
        public int EmployeeId { get; set; }
        public DateTime? ProductionDate { get; set; }

        public string? StartTimeFilling { get; set; }   
        public string? FinishTimeFilling { get; set; }  
        public string? TimeChecked { get; set; }        

        public string? HygienicCondition { get; set; }
        public string? SealingCondition { get; set; }
        public string? PackagingType { get; set; }

        public int? QtyPouchMade { get; set; }
        public string? Comments { get; set; }

        public List<Staff> Employees { get; set; } = new();

        
    }
}