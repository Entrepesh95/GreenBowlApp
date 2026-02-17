using System.ComponentModel.DataAnnotations;

namespace GreenBowlApp.Models
{
    public class Staff
    {
        public int EmployeeId { get; set; }

        public string FullName { get; set; } = "";

        public string LastName { get; set; } = "";

        public string Name { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        public string? EmployeeRole { get; set; }

        public string? Error { get; set; }
    }
}