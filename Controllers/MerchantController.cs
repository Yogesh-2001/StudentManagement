using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentManagement.Models;
using StudentManagement.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class MerchantController : ControllerBase
    {
        private readonly string connectionString;
        private readonly IDashboardService dashboardService;

        public MerchantController(IConfiguration configuration, IDashboardService _dashboardService)
        {
            connectionString = configuration.GetConnectionString("DbConnectionString");
            dashboardService = _dashboardService;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<ServiceResponse<MerchantDetail>>> GetMerchantDetails(string mobileNo)
        {

            // 1. Input Validation
            if (string.IsNullOrEmpty(mobileNo))
            {
                return BadRequest(new ServiceResponse<MerchantDetail>
                {
                    Success = false,
                    Message = "mobileNo query parameter is required."
                });
            }


            ServiceResponse<MerchantDetail> response = await dashboardService.GetDashboardDetails(mobileNo);

            // 3. Handle response status
            if (!response.Success)
            {
                // If the service failed (DB error, etc.)
                return StatusCode(500, response);
            }

            if (response.Data == null || response.Data.merchantName == null)
            {
                // If the query was successful but returned no merchant data
                response.Success = false;
                response.Message = "Merchant not found.";
                return NotFound(response);
            }

            return Ok(response);
        }


        [HttpGet("dashboard/{agentId}")]
        [ProducesResponseType(typeof(ServiceResponse<DashboardDataDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<DashboardDataDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ServiceResponse<DashboardDataDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboardDetails(string agentId)
        {
            if (string.IsNullOrEmpty(agentId))
            {
                return BadRequest(new ServiceResponse<DashboardDataDto>
                {
                    Success = false,
                    Message = "Agent ID cannot be empty.",
                    Data = null
                });
            }

            try
            {
                // Call the repository to fetch the structured data
                var data = await dashboardService.GetDashboardDataAsync(agentId);

                if (data == null)
                {
                    // Return 404 if the agent is valid but no data (or agent itself) was found
                    return NotFound(new ServiceResponse<DashboardDataDto>
                    {
                        Success = false,
                        Message = $"Dashboard data not found for agent ID: {agentId}",
                        Data = null
                    });
                }

                // Return 200 OK with the structured data
                return Ok(new ServiceResponse<DashboardDataDto>
                {
                    Success = true,
                    Message = "Dashboard data fetched successfully",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                // Log the exception here (e.g., using ILogger)
                Console.WriteLine($"Error fetching dashboard for {agentId}: {ex.Message}");

                // Return 500 Internal Server Error for unhandled exceptions (e.g., database connection issues)
                return StatusCode(StatusCodes.Status500InternalServerError, new ServiceResponse<DashboardDataDto>
                {
                    Success = false,
                    Message = "An internal server error occurred while processing your request.",
                    Data = null
                });
            }
        }

        [HttpPost("create-poi")]
        [ProducesResponseType(typeof(ServiceResponse<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ServiceResponse<int>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServiceResponse<int>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePoiAssignment([FromBody] PoiAssignmentInputDto assignment)
        {
            // Basic validation
            if (!ModelState.IsValid || assignment.PoiId <= 0 || string.IsNullOrEmpty(assignment.AgentId))
            {
                return BadRequest(new ServiceResponse<int>
                {
                    Success = false,
                    Message = "Invalid assignment data provided.",
                    Data = 0
                });
            }

            try
            {
                bool success = await dashboardService.CreatePoiAssignmentAsync(assignment);

                if (success)
                {
                    // HTTP 201 Created is the standard response for successful creation
                    return StatusCode(StatusCodes.Status201Created, new ServiceResponse<int>
                    {
                        Success = true,
                        Message = $"POI assignment {assignment.PoiId} created successfully for agent {assignment.AgentId}.",
                        Data = assignment.PoiId
                    });
                }

                // This path is unlikely due to transaction/exception handling, but included for completeness
                return BadRequest(new ServiceResponse<int>
                {
                    Success = false,
                    Message = "Failed to create POI assignment due to an unknown error.",
                    Data = 0
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Creation Error: {ex.Message}");

                // Return 500 Internal Server Error
                return StatusCode(StatusCodes.Status500InternalServerError, new ServiceResponse<int>
                {
                    Success = false,
                    Message = "An internal server error occurred during assignment creation.",
                    Data = 0
                });
            }
        }
    }
}
