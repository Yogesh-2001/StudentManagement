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

        

    }
}
