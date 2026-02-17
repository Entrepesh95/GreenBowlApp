using System;
using System.Collections.Generic;

namespace GreenBowlApp.Models.ViewModels
{
    public class PreEquipmentModel
    {
        // the equipment pre model to pass fields to the controller
        
        public string BatchId { get; set; } = "";   
        public int EmployeeId { get; set; }         

        public int EquipId { get; set; }
        public string EmployeeName { get; set; } = "";

        
        public List<BatchOption> Batches { get; set; } = new();
        public List<Staff> Employees { get; set; } = new();

        
        public DateTime? Date { get; set; } = DateTime.Today; 

        public string? StartTime { get; set; }  
        public string? FinishTime { get; set; } 

        public string? EquipmentName { get; set; }
        public string? ConditionOfEquipment { get; set; }
        public string? ActionTaken { get; set; }

        public string GlutenTest { get; set; } = "Pass"; 
        public string? PackagingMachine { get; set; }
        public string? Comments { get; set; }
    }

    public class BatchOption
    {
        public string BatchId { get; set; } = "";
        public string Label { get; set; } = ""; 
    }
}