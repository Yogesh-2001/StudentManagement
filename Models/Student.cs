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
     
        public string Name { get; set; }
      
        public string Department { get; set; }
     
        public decimal Salary { get; set; }
    }
}
