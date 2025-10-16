using System;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Validators
{
    public class DateCheckValidator:ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
           var date = (DateTime)value;
            if(date < DateTime.Now)
            {
                return new ValidationResult("the date must be greater than  today date");
            }

            return ValidationResult.Success;
        }
    }
}
