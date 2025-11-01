using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentManagement.Models;
using StudentManagement.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Services
{
    public class DashboardService : IDashboardService
    {

        private readonly string connectionString;
        public DashboardService(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DbConnectionString");
        }

        public async Task<ServiceResponse<MerchantDetail>> GetDashboardDetails(string mobileNo)
        {
            var response = new ServiceResponse<MerchantDetail>();
            var dashboardData = new MerchantDetail();

            // 1. SQL Query to fetch Merchant info and ALL assigned POIs in one go.
            // Assumes MerchantId is linked to POI table (AssignedMerchantId)
            string sql = @"
            SELECT 
                M.MerchantName, 
                M.DataCollectionLocation,
                P.PoiId, 
                P.PoiName, 
                P.Status, 
                P.Priority,
                P.TaskStatus -- Used for calculating task counts
            FROM Merchant M
            LEFT JOIN PointOfInterest P ON M.MerchantId = P.AssignedMerchantId
            WHERE M.MobileNo = @MobileNo;";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MobileNo", mobileNo);
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        var allAssignedPOIs = new List<AssignedPOI>();
                        var taskStatuses = new List<string>();
                        bool isFirstRow = true;

                        while (await reader.ReadAsync())
                        {
                            if (isFirstRow)
                            {
                                // Populate core merchant details only once
                                dashboardData.merchantName = reader.GetString(reader.GetOrdinal("MerchantName"));
                                dashboardData.dataCollectionLocation = reader.IsDBNull(reader.GetOrdinal("DataCollectionLocation")) ? null : reader.GetString(reader.GetOrdinal("DataCollectionLocation"));
                                isFirstRow = false;
                            }

                            // Add assigned POI data
                            if (!reader.IsDBNull(reader.GetOrdinal("PoiId")))
                            {
                                var poi = new AssignedPOI
                                {
                                    poiId = reader.GetString(reader.GetOrdinal("PoiId")),
                                    poiName = reader.GetString(reader.GetOrdinal("PoiName")),
                                    status = reader.GetString(reader.GetOrdinal("Status")),
                                    priority = reader.IsDBNull(reader.GetOrdinal("Priority")) ? null : reader.GetString(reader.GetOrdinal("Priority"))
                                };
                                allAssignedPOIs.Add(poi);

                                // Collect TaskStatus for later counting
                                if (!reader.IsDBNull(reader.GetOrdinal("TaskStatus")))
                                {
                                    taskStatuses.Add(reader.GetString(reader.GetOrdinal("TaskStatus")));
                                }
                            }
                        }

                        // 2. Calculate Task Counts from the collected statuses
                        dashboardData.taskCounts = CalculateTaskCounts(taskStatuses);
                        dashboardData.assignedPOIs = allAssignedPOIs;
                    }
                }
                response.Data = dashboardData;

                // 3. Handle response status
                if (!response.Success)
                {
                    // If the service failed (DB error, etc.)
                    return response;
                }

                if (response.Data == null || response.Data.merchantName == null)
                {
                    // If the query was successful but returned no merchant data
                    response.Success = false;
                    response.Message = "Merchant not found.";
                    return response;
                }

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Database error: {ex.Message}";
                response.Data = null;
            }
            return response;
        }

        // Helper method to calculate counts
        private TaskCount CalculateTaskCounts(List<string> statuses)
        {
            return new TaskCount
            {
                // Assuming 'pending', 'revision', 'completed' (today), and 'verified' are possible values for TaskStatus
                pending = statuses.Count(s => s.Equals("pending", StringComparison.OrdinalIgnoreCase)),
                revision = statuses.Count(s => s.Equals("revision", StringComparison.OrdinalIgnoreCase)),
                verified = statuses.Count(s => s.Equals("verified", StringComparison.OrdinalIgnoreCase)),
                // Note: 'completedToday' often requires comparing a SubmissionTimestamp to the current date,
                // which this simplified count cannot do. Here it assumes a status exists.
                completedToday = statuses.Count(s => s.Equals("completed", StringComparison.OrdinalIgnoreCase))
            };
        }
    }
}
