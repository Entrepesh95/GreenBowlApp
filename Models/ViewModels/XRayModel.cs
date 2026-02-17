using System;
using System.Collections.Generic;

namespace GreenBowlApp.Models.ViewModels
{
    public class XRayModel
    {
        // model for xray controller and views

        public int XrayId { get; set; }                 
        public string EmployeeName { get; set; } = "";  
        public string BatchId { get; set; } = "";
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";

        public int EmployeeId { get; set; }
        public DateTime? Date { get; set; }

        public string? StartTime { get; set; } 
        public string? EndTime { get; set; }   

        public bool Rejected { get; set; }
        public int? RejectedPouches { get; set; }  

        public string? CorrectionAction { get; set; }
        public string? Comments { get; set; }

        public List<Staff> Employees { get; set; } = new();
    }
}