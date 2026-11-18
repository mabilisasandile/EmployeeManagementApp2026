using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementApp.Models
{
    //Exercise 3 - Model
    public class Employee
    {
        public int EmployeeId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Department { get; set; }
    }
}
