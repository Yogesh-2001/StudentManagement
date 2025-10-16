using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;
using StudentManagement.Repository;
using System.Collections.Generic;
using System.Linq;
namespace StudentManagement.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class StudentController:ControllerBase
    {
        [HttpGet("get-all-students")]
        public ActionResult<List<Student>> GetAllStudents()
        {
            return StudentRepo.students;
        }

        [HttpGet("get-student/{id:int}")]
        public ActionResult<Student> GetAllStudentById(int id)
        {
            var student = StudentRepo.students.Where(n => n.Id == id).FirstOrDefault();
            if (id == 0)
            {
                return BadRequest();
            }
            else if (student == null)
            {
                return NotFound();
            }
            return student;
        }

        [HttpDelete("{id:int}")]
        public ActionResult<bool> DeleteStudentById(int id)
        {
            var student = StudentRepo.students.Where(n => n.Id == id).FirstOrDefault();
            if (student == null)
            {
                return NotFound("student not found");
            }
            StudentRepo.students.Remove(student);
            return Ok(true);
        }

        [HttpPost("create")]
        public ActionResult<Student> CreateStudent(Student model)
        {
            var student = StudentRepo.students.LastOrDefault();
            model.Id  = student.Id +1;
            StudentRepo.students.Add(model);
           
           
            return Ok(model);
        }

        [HttpPut("update")]
        public ActionResult UpdateStudent(Student model)
        {
            var student = StudentRepo.students.Where(n=>n.Id == model.Id).FirstOrDefault();
            if (student == null)
            {
                return BadRequest("student not found");
            }
            student.Name = model.Name;
            student.Department = model.Department;
            student.age  = model.age;
          
            return NoContent();
        }
    }
}
