using System;
using System.Collections.Generic;

namespace GreenBowlApp.Models.ViewModels
{
    public class BatchProcessingModel
    {
        //model for process control 
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string BatchId { get; set; } = "";

        public int ProcessId { get; set; }          
        public string EmployeeName { get; set; } = "";

        
        public int EmployeeId { get; set; }
        public DateTime? ProductionDate { get; set; }

        public string? StartTime { get; set; }   
        public string? FinishTime { get; set; }  
        public string? TimeChecked { get; set; } 

        public string? AlarmAutoclave { get; set; }

        public string? ReasonComment { get; set; }
        public string? ActionTakenComment { get; set; }
        public string? Comments { get; set; }

        public List<Staff> Employees { get; set; } = new();
    }
}