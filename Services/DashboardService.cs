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

        // --- Core Repository Method ---
        public async Task<DashboardDataDto> GetDashboardDataAsync(string agentId)
        {
            var data = new DashboardDataDto { AssignedPOIs = new List<PoiDto>() };
            var poiDictionary = new Dictionary<int, PoiDto>();

            // SQL Server T-SQL Batch for multiple result sets
            // NOTE: The table names and column names must match the SQL Server schema
            var sql = @"
            -- 1. Get Agent Header Data
            SELECT AgentId, AgentName, DataCollectionLocation 
            FROM Agents 
            WHERE AgentId = @AgentId;

            -- 2. Get Task Summary Counts
            SELECT TaskStatus AS TaskName, COUNT(*) AS Count 
            FROM POIS 
            WHERE AgentId = @AgentId 
            GROUP BY TaskStatus;

            -- 3. Get POI and Subtask Data (JOINed)
            SELECT
                p.poiId, p.poiName, p.category, p.categoryId, p.taskPriority, p.taskStatus, 
                p.progress, p.revisionRequired, p.contactNo, p.latitude, p.longitude, 
                p.revisionMessage, s.iconUrl, s.text
            FROM POIS p
            LEFT JOIN Subtasks s ON p.PoiId = s.PoiId
            WHERE p.AgentId = @AgentId
            ORDER BY p.PoiId;
        ";

            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(sql, connection);

            // Add parameter for filtering by AgentId
            command.Parameters.AddWithValue("@AgentId", agentId);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                // 1. Read Agent Data (First Result Set)
                if (await reader.ReadAsync())
                {
                    data.AgentId = reader["AgentId"].ToString();
                    data.AgentName = reader["AgentName"].ToString();
                    data.DataCollectionLocation = reader["DataCollectionLocation"].ToString();
                }
                else
                {
                    // Agent not found
                    return null;
                }

                // 2. Read Task Summary (Second Result Set)
                if (await reader.NextResultAsync())
                {
                    data.TaskSummary = new List<TaskSummaryDto>();
                    while (await reader.ReadAsync())
                    {
                        data.TaskSummary.Add(new TaskSummaryDto
                        {
                            TaskName = reader["TaskName"].ToString(),
                            Count = Convert.ToInt32(reader["Count"])
                        });
                    }
                }

                // 3. Read POI and Subtask Data (Third Result Set)
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int poiId = Convert.ToInt32(reader["PoiId"]);

                        // --- Hierarchical Mapping Logic ---

                        // Check if POI is already mapped
                        if (!poiDictionary.TryGetValue(poiId, out var poiEntry))
                        {
                            // Create new POI entry
                            poiEntry = new PoiDto
                            {
                                PoiId = poiId,
                                PoiName = reader["PoiName"].ToString(),
                                Category = reader["Category"].ToString(),
                                CategoryId = Convert.ToInt32(reader["CategoryId"]),
                                TaskPriority = reader["TaskPriority"].ToString(),
                                TaskStatus = reader["TaskStatus"].ToString(),
                                Progress = Convert.ToInt32(reader["Progress"]),
                                RevisionRequired = Convert.ToBoolean(reader["RevisionRequired"]),
                                ContactNo = reader["ContactNo"].ToString(),
                                Latitude = reader["Latitude"].ToString(),
                                Longitude = reader["Longitude"].ToString(),
                                // Handle potential DBNull for RevisionMessage
                                RevisionMessage = reader["RevisionMessage"] as string,
                            };
                            poiDictionary.Add(poiId, poiEntry);
                            data.AssignedPOIs.Add(poiEntry);
                        }

                        // Add subtask if text is not null (meaning a subtask exists for this row)
                        if (reader["Text"] != DBNull.Value)
                        {
                            poiEntry.SubTasks.Add(new SubtaskDto
                            {
                                IconUrl = reader["IconUrl"].ToString(),
                                Text = reader["Text"].ToString()
                            });
                        }
                    }
                }

                return data;
            }
            catch (SqlException ex)
            {
                // Log the error (e.g., using ILogger)
                Console.WriteLine($"SQL Error: {ex.Message}");
                throw; // Re-throw or handle gracefully
            }
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

        public async Task<bool> CreatePoiAssignmentAsync(PoiAssignmentInputDto assignment)
        {
            // 1. POIS Insert SQL
            var poiSql = @"
        INSERT INTO POIS (PoiId, AgentId, PoiName, Category, CategoryId, TaskPriority, 
                          TaskStatus, Progress, RevisionRequired, ContactNo, Latitude, Longitude)
        VALUES (@PoiId, @AgentId, @PoiName, @Category, @CategoryId, @TaskPriority, 
                @TaskStatus, @Progress, 0, @ContactNo, @Latitude, @Longitude);
    ";

            // 2. SUBTASKS Insert SQL
            // Note: We assume SubtaskId is IDENTITY/AUTO_INCREMENT in the database
            var subtaskSql = @"
        INSERT INTO Subtasks (PoiId, IconUrl, Text)
        VALUES (@PoiId, @IconUrl, @Text);
    ";

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Start a transaction to ensure atomicity
            using var transaction = connection.BeginTransaction();

            try
            {
                // --- 1. Insert into POIS table ---
                using (var command = new SqlCommand(poiSql, connection, transaction))
                {
                    // Add parameters for POIS table
                    command.Parameters.AddWithValue("@PoiId", assignment.PoiId);
                    command.Parameters.AddWithValue("@AgentId", assignment.AgentId);
                    command.Parameters.AddWithValue("@PoiName", assignment.PoiName);
                    command.Parameters.AddWithValue("@Category", assignment.Category);
                    command.Parameters.AddWithValue("@CategoryId", assignment.CategoryId);
                    command.Parameters.AddWithValue("@TaskPriority", assignment.TaskPriority);
                    command.Parameters.AddWithValue("@TaskStatus", assignment.TaskStatus);
                    command.Parameters.AddWithValue("@Progress", assignment.Progress);
                    command.Parameters.AddWithValue("@ContactNo", assignment.ContactNo);
                    command.Parameters.AddWithValue("@Latitude", assignment.Latitude);
                    command.Parameters.AddWithValue("@Longitude", assignment.Longitude);

                    await command.ExecuteNonQueryAsync();
                }

                // --- 2. Insert into SUBTASKS table ---
                if (assignment.SubTasks != null && assignment.SubTasks.Any())
                {
                    foreach (var subtask in assignment.SubTasks)
                    {
                        using (var command = new SqlCommand(subtaskSql, connection, transaction))
                        {
                            // Add parameters for Subtasks table
                            command.Parameters.AddWithValue("@PoiId", assignment.PoiId);
                            command.Parameters.AddWithValue("@IconUrl", subtask.IconUrl ?? (object)DBNull.Value); // Handle null
                            command.Parameters.AddWithValue("@Text", subtask.Text);

                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }

                // --- 3. Commit the transaction ---
                transaction.Commit();
                return true;
            }
            catch (SqlException ex)
            {
                // Rollback on failure
                transaction.Rollback();
                // Log the error
                Console.WriteLine($"SQL Error during assignment creation: {ex.Message}");
                throw; // Re-throw or handle as needed
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error during assignment creation: {ex.Message}");
                throw;
            }
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
