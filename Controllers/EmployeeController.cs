using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentManagement.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;



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
                    string query = "SELECT * FROM [Employees]";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        employees.Add(new Employee
                        {
                            Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 :reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.IsDBNull(reader.GetOrdinal("name")) ? null : reader.GetString(reader.GetOrdinal("name")),
                            MobileNumber = reader.IsDBNull(reader.GetOrdinal("mobile_number")) ? null : reader.GetString(reader.GetOrdinal("mobile_number")),
                            Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email")),
                            Address = reader.IsDBNull(reader.GetOrdinal("address")) ? null : reader.GetString(reader.GetOrdinal("address")),
                            CityAssigned = reader.IsDBNull(reader.GetOrdinal("city_assigned")) ? null : reader.GetString(reader.GetOrdinal("city_assigned")),
                            Pincode = reader.IsDBNull(reader.GetOrdinal("pincode")) ? null : reader.GetString(reader.GetOrdinal("pincode")),
                            ProfilePhoto = reader.IsDBNull(reader.GetOrdinal("profile_photo")) ? null : reader.GetString(reader.GetOrdinal("profile_photo")),
                            IdProofType = reader.IsDBNull(reader.GetOrdinal("id_proof_type")) ? null : reader.GetString(reader.GetOrdinal("id_proof_type")),
                            IdProofNumber = reader.IsDBNull(reader.GetOrdinal("id_proof_number")) ? null : reader.GetString(reader.GetOrdinal("id_proof_number")),
                            RegistrationSource = reader.IsDBNull(reader.GetOrdinal("registration_source")) ? null : reader.GetString(reader.GetOrdinal("registration_source")),

                            
                            Status = reader.IsDBNull(reader.GetOrdinal("status")) ? false : reader.GetBoolean(reader.GetOrdinal("status")),

                            TotalPoisAssigned = reader.IsDBNull(reader.GetOrdinal("total_pois_assigned")) ? null : reader.GetString(reader.GetOrdinal("total_pois_assigned")),
                            TotalPoisCompleted = reader.IsDBNull(reader.GetOrdinal("total_pois_completed")) ? null : reader.GetString(reader.GetOrdinal("total_pois_completed")),
                            TotalPoisApproved = reader.IsDBNull(reader.GetOrdinal("total_pois_approved")) ? null : reader.GetString(reader.GetOrdinal("total_pois_approved")),
                            TotalPoisRejected = reader.IsDBNull(reader.GetOrdinal("total_pois_rejected")) ? null : reader.GetString(reader.GetOrdinal("total_pois_rejected")),
                            ProgressPercentage = reader.IsDBNull(reader.GetOrdinal("progress_percentage")) ? null : reader.GetString(reader.GetOrdinal("progress_percentage")),
                            CreatedAt = reader.IsDBNull(reader.GetOrdinal("created_at")) ? null : reader.GetString(reader.GetOrdinal("created_at"))
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

        [HttpPost("create")]
        public async Task<ServiceResponse<Employee>> AddEmployee(Employee newEmployee)
        {
            try
            {
                string sql = @"
        INSERT INTO [Employees] (
            [name], mobile_number, email, address, city_assigned, pincode, 
            profile_photo, id_proof_type, id_proof_number, registration_source, 
            status, total_pois_assigned, total_pois_completed, 
            total_pois_approved, total_pois_rejected, progress_percentage, created_at
        )
        VALUES (
            @Name, @MobileNumber, @Email, @Address, @CityAssigned, @Pincode, 
            @ProfilePhoto, @IdProofType, @IdProofNumber, @RegistrationSource, 
            @Status, @TotalPoisAssigned, @TotalPoisCompleted, 
            @TotalPoisApproved, @TotalPoisRejected, @ProgressPercentage, @CreatedAt
        );

    ";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", newEmployee.Name);
                        cmd.Parameters.AddWithValue("@MobileNumber", newEmployee.MobileNumber);
                        cmd.Parameters.AddWithValue("@Email", newEmployee.Email);
                        cmd.Parameters.AddWithValue("@Address", newEmployee.Address);
                        cmd.Parameters.AddWithValue("@CityAssigned", newEmployee.CityAssigned);
                        cmd.Parameters.AddWithValue("@Pincode", newEmployee.Pincode);
                        cmd.Parameters.AddWithValue("@ProfilePhoto", newEmployee.ProfilePhoto);
                        cmd.Parameters.AddWithValue("@IdProofType", newEmployee.IdProofType);
                        cmd.Parameters.AddWithValue("@IdProofNumber", newEmployee.IdProofNumber);
                        cmd.Parameters.AddWithValue("@RegistrationSource", newEmployee.RegistrationSource);

                        // Status is bool (C#) -> BIT (SQL)
                        cmd.Parameters.AddWithValue("@Status", newEmployee.Status);

                        cmd.Parameters.AddWithValue("@TotalPoisAssigned", newEmployee.TotalPoisAssigned);
                        cmd.Parameters.AddWithValue("@TotalPoisCompleted", newEmployee.TotalPoisCompleted);
                        cmd.Parameters.AddWithValue("@TotalPoisApproved", newEmployee.TotalPoisApproved);
                        cmd.Parameters.AddWithValue("@TotalPoisRejected", newEmployee.TotalPoisRejected);
                        cmd.Parameters.AddWithValue("@ProgressPercentage", newEmployee.ProgressPercentage);

                        // Use DBNull.Value if CreatedAt is null, otherwise use the string value
                        cmd.Parameters.AddWithValue("@CreatedAt", newEmployee.CreatedAt ?? (object)DBNull.Value);

                        await conn.OpenAsync();
                        int newId = cmd.ExecuteNonQuery();

                        if (newId <= 0)
                        {
                            return new ServiceResponse<Employee>
                            {
                                Success = false,
                                Message = "Employee creation failed",
                                Data = null
                            };
                        }

                        return new ServiceResponse<Employee>
                        {
                            Success = true,
                            Message = "Employee created successfully",
                            Data = null
                        };
                    }
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
