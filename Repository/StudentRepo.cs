using StudentManagement.Models;
using System.Collections.Generic;

namespace StudentManagement.Repository
{
    public class StudentRepo
    {
        public static List<Student> students = new List<Student>() 
        { new Student {Id=1,age=22,Name="Yogesh",Department = "CS" },
          new Student {Id=2,age=24,Name="Nikhil",Department = "IT" }
        };
    }
}
