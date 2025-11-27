using Azure;
using Azure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using StudentManagement.Models;
using StudentManagement.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;

namespace StudentManagement.Services
{

    
    public class PoiService : IPOIService
    {
        private readonly string connectionString;
        public PoiService(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DbConnectionString");
        }
     

        public async Task<ServiceResponse<List<POIField>>> GetPoiFieldsAsync(string poiId)
        {
            var response = new ServiceResponse<List<POIField>>();

            // Assumes the configuration is stored in PoiSubmissionDetails for simplicity,
            // or a specialized PoiFieldConfig table. We will fetch the config data here.
            string sql = "SELECT TOP 1 FieldConfiguration FROM PoiSubmissionDetails WHERE PoiId = @PoiId ORDER BY SubmissionTimestamp DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PoiId", poiId);
                    await conn.OpenAsync();

                    string? configJson = (await cmd.ExecuteScalarAsync()) as string;

                    if (string.IsNullOrEmpty(configJson))
                    {
                        response.Success = false;
                        response.Message = "Field configuration not found for this POI.";
                        return response;
                    }

                    // Deserialize the JSON string back into the C# model (requires Newtonsoft.Json or System.Text.Json)
                    // Assuming System.Text.Json for modern Core apps:
                    var fields = System.Text.Json.JsonSerializer.Deserialize<List<POIField>>(configJson);

                    response.Data = fields;
                    response.Message = "Field configuration retrieved successfully.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Database or deserialization error: {ex.Message}";
                response.Data = null;
            }
            return response;
        }


        // Services/PoiService.cs

        public async Task<ServiceResponse<object>> SubmitPoiAsync(SubmitPOIRequest submission)
        {
            var response = new ServiceResponse<object>();

            // Find MerchantId using MobileNo
            int merchantId = await GetMerchantIdByMobileNoAsync(submission.mobileNo);
            if (merchantId == 0)
            {
                response.Success = false;
                response.Message = "Invalid Mobile Number.";
                return response;
            }

            // Serialize FieldData and PhotosData into JSON strings for database storage
            string submittedFieldDataJson = System.Text.Json.JsonSerializer.Serialize(submission.FieldData);

            // Note: If you don't store the configuration in the submission table, 
            // you might need to query it here to check validity before inserting.
            // For this script, we assume FieldConfiguration is NOT required for the INSERT.

            string sql = @"
        INSERT INTO PoiSubmissionDetails (
            PoiId, MerchantId, SubmissionTimestamp, FormVersion, 
            FieldConfiguration, SubmittedFieldData, SubmittedPhotosData, StatusMessage
        )
        VALUES (
            @PoiId, @MerchantId, GETDATE(), @FormVersion, 
            @FieldConfig, @SubmittedFieldData, @SubmittedPhotosData, @StatusMessage
        );";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PoiId", submission.poiId);
                    cmd.Parameters.AddWithValue("@MerchantId", merchantId);
                    cmd.Parameters.AddWithValue("@FormVersion", "1.0"); // Hardcode or derive version
                    cmd.Parameters.AddWithValue("@FieldConfig", "{}"); // Placeholder if not used
                    cmd.Parameters.AddWithValue("@SubmittedFieldData", submittedFieldDataJson);
                    cmd.Parameters.AddWithValue("@SubmittedPhotosData", submission.FieldData!["Photo Requirement"]); // Extract photos if needed
                    cmd.Parameters.AddWithValue("@StatusMessage", "Submit Successful");

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();

                    // Update the POI status in the PointOfInterest table (critical step)
                    await UpdatePoiStatus(submission.poiId, "Submitted");

                    response.Message = "Submit Successful";
                    response.Data = new { message = "Submit Successful", status = "Success" };
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Database error during submission: {ex.Message}";
                response.Data = null;
            }
            return response;
        }


        public async Task<ServiceResponse<object>> InsertAgentAsync(string payload, int poid, int agentid)
        {
            int insertedId;
            var response = new ServiceResponse<object>();


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    string sql = "INSERT INTO AgentPoi (PoiRequest,POIid,AgentId) OUTPUT INSERTED.Id VALUES (@payload,@poid,@agentid)";
                    
                    SqlCommand cmd = new SqlCommand(sql, conn, transaction);
                    cmd.Parameters.AddWithValue("@payload", payload);
                    cmd.Parameters.AddWithValue("@poid", poid.ToString());
                    cmd.Parameters.AddWithValue("@agentid", agentid.ToString());

                    
                    
                    insertedId = (int)await cmd.ExecuteScalarAsync();

                    string updateSql =
                        @"UPDATE POIS
                      SET taskStatus = @TaskStatus
                      WHERE poiId = @PoiId AND agentId = @AgentId";

                    SqlCommand updateCmd = new SqlCommand(updateSql, conn, transaction);
                    updateCmd.Parameters.AddWithValue("@TaskStatus", "In Progress");
                    updateCmd.Parameters.AddWithValue("@PoiId", poid.ToString());
                    updateCmd.Parameters.AddWithValue("@AgentId", agentid.ToString());

                    int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        transaction.Rollback();
                        return new ServiceResponse<object> { Data = null, Message = "Failed to add POI Detail", Success = false };
                    }

                    // Commit both insert + update
                    transaction.Commit();

                    return new ServiceResponse<object> { Data = insertedId, Message = "POI Details Stored Successfully", Success = true };

                }
                catch (Exception ex)
                {
                    return new ServiceResponse<object> { Data = null, Message = "Failed to add POI Detail :" + ex.Message.ToString(), Success = false };
                }

            }

        }


        // Placeholder for Merchant lookup (must be implemented)
        private async Task<int> GetMerchantIdByMobileNoAsync(string? mobileNo)
        {
            if (string.IsNullOrEmpty(mobileNo))
            {
                return 0;
            }

            int merchantId = 0;

            // SQL to select the MerchantId
            string sql = "SELECT MerchantId FROM Merchant WHERE MobileNumber = @MobileNo";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // Use AddWithValue for the parameter
                    cmd.Parameters.AddWithValue("@MobileNo", mobileNo);

                    await conn.OpenAsync();

                    // ExecuteScalar retrieves the single value (MerchantId)
                    var result = await cmd.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        // Convert the result to integer
                        merchantId = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (recommended in a real application)
                Console.WriteLine($"Error looking up Merchant ID: {ex.Message}");
                // Return 0 on error
                return 0;
            }

            return merchantId;
        }

  

        // Placeholder for POI Status Update (must be implemented)
        private async Task UpdatePoiStatus(string? poiId, string status)
        {
            if (string.IsNullOrEmpty(poiId) || string.IsNullOrEmpty(status))
            {
                return;
            }

            // SQL UPDATE statement to modify the status based on PoiId
            string sql = @"
        UPDATE PointOfInterest 
        SET 
            Status = @Status, 
            TaskStatus = @Status 
        WHERE PoiId = @PoiId";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // Add parameters for safe execution
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@PoiId", poiId);

                    await conn.OpenAsync();

                    // ExecuteNonQuery is used for INSERT, UPDATE, and DELETE operations
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the exception (recommended in a real application)
                Console.WriteLine($"Error updating POI status for {poiId}: {ex.Message}");
            }
        }

    }
}
