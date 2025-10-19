using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentManagement.Models;
using System;
using System.Collections.Generic;
using System.Configuration;



namespace StudentManagement.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly string connectionString = string.Empty;

        public EmployeeController(IConfiguration configuration)
        {
            
            connectionString = configuration.GetConnectionString("DbConnectionString");
        }

        [HttpGet("employees")]
        public ServiceResponse<List<Employee>> GetAllEmployees()
        {
            var employees = new List<Employee>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Employees";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        employees.Add(new Employee
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["name"],
                            Department = (string)reader["Department"],
                            Salary = Convert.ToDecimal(reader["Salary"])
                        });
                    }
                }
                return new ServiceResponse<List<Employee>>
                {
                    Success = true,
                    Message = "All employees fetched.",
                    Data = employees
                };
            }
            catch (Exception ex)
            {

                return new ServiceResponse<List<Employee>>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }


        public ServiceResponse<Employee> AddEmployee(Employee emp)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Employees (Name, Department, Salary) OUTPUT INSERTED.Id VALUES (@Name, @Department, @Salary)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", emp.Name);
                    cmd.Parameters.AddWithValue("@Department", emp.Department);
                    cmd.Parameters.AddWithValue("@Salary", emp.Salary);
                    conn.Open();
                    emp.Id = (int)cmd.ExecuteScalar();
                    return new ServiceResponse<Employee>
                    {
                        Success = true,
                        Message = "Employee added successfully.",
                        Data = emp
                    };

                }
            }
            catch (Exception ex)
            {

                return new ServiceResponse<Employee>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
    }
}
