using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace StudentManagement.Models
{
    public class Student
    {
        [ValidateNever]
        public int Id { get; set; }
        [Required(ErrorMessage ="Name is required"),StringLength(20)]
        public string Name { get; set; }
        public string Department { get; set; }
        [Range(20,40)]
        public int age { get; set; }
        
    }
    public class Employee
    {
       
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? CityAssigned { get; set; }
        public string? Pincode { get; set; }


        public string? ProfilePhoto { get; set; }
        public string? IdProofType { get; set; }
        public string? IdProofNumber { get; set; }
        public string? RegistrationSource { get; set; }


        public bool? Status { get; set; } 
        public string? TotalPoisAssigned { get; set; }
        public string? TotalPoisCompleted { get; set; }
        public string? TotalPoisApproved { get; set; }
        public string? TotalPoisRejected { get; set; }
        public string? ProgressPercentage { get; set; }

      
        public string? CreatedAt { get; set; } 
    }
}
