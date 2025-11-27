using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StudentManagement.Models;
using StudentManagement.Repository;
using StudentManagement.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudentManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PointOfInterestController : ControllerBase
    {
        private readonly string connectionString;
        private readonly IPOIService poiservice;

        public PointOfInterestController(IConfiguration configuration, IPOIService _poiservice)
        {
            connectionString = configuration.GetConnectionString("DbConnectionString");
            poiservice = _poiservice;
        }
        [HttpPost("create")]
        public async Task<ServiceResponse<string>> CreatePOI([FromBody] PoiRecord newPoiRecord)
        {
            try
            {
                string sql = @"
        INSERT INTO [PoiDetails] (
            agent_id, poi_id, category, poi_name, address, latitude, longitude,
            contact_number, email, website, sub_category, amenities, room_types, 
            target_audience, description, media_photos, media_360, 
            verification_status, form_version, remarks, status, message, 
            submitted_on, assigned_verifier, next_action
        )
        VALUES (
            @AgentId, @PoiId, @Category, @PoiName, @Address, @Latitude, @Longitude,
            @ContactNumber, @Email, @Website, @SubCategory, @Amenities, @RoomTypes, 
            @TargetAudience, @Description, @MediaPhotos, @Media360, 
            @VerificationStatus, @FormVersion, @Remarks, @Status, @Message, 
            @SubmittedOn, @AssignedVerifier, @NextAction
        );
        
    ";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        
                        cmd.Parameters.AddWithValue("@AgentId", newPoiRecord.AgentId);
                        cmd.Parameters.AddWithValue("@PoiId", newPoiRecord.PoiId);
                        cmd.Parameters.AddWithValue("@Category", newPoiRecord.Category);
                        cmd.Parameters.AddWithValue("@PoiName", newPoiRecord.PoiName);
                        cmd.Parameters.AddWithValue("@Address", newPoiRecord.Address);
                        cmd.Parameters.AddWithValue("@Latitude", newPoiRecord.Latitude ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Longitude", newPoiRecord.Longitude ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactNumber", newPoiRecord.ContactNumber);
                        cmd.Parameters.AddWithValue("@Email", newPoiRecord.Email);
                        cmd.Parameters.AddWithValue("@Website", newPoiRecord.Website ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@SubCategory", newPoiRecord.SubCategory);
                        cmd.Parameters.AddWithValue("@Amenities", newPoiRecord.Amenities);
                        cmd.Parameters.AddWithValue("@RoomTypes", (object)newPoiRecord.RoomTypes ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@TargetAudience", newPoiRecord.TargetAudience);
                        cmd.Parameters.AddWithValue("@Description", newPoiRecord.Description);
                        cmd.Parameters.AddWithValue("@MediaPhotos", newPoiRecord.MediaPhotos ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Media360", newPoiRecord.Media360 ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@VerificationStatus", newPoiRecord.VerificationStatus);
                        cmd.Parameters.AddWithValue("@FormVersion", newPoiRecord.FormVersion);
                        cmd.Parameters.AddWithValue("@Remarks", newPoiRecord.Remarks ?? (object)DBNull.Value);

                       
                        cmd.Parameters.AddWithValue("@Status", newPoiRecord.Status);

                        cmd.Parameters.AddWithValue("@Message", newPoiRecord.Message);

                        
                        cmd.Parameters.AddWithValue("@SubmittedOn", newPoiRecord.SubmittedOn ?? (object)DBNull.Value);

                        cmd.Parameters.AddWithValue("@AssignedVerifier", newPoiRecord.AssignedVerifier);
                        cmd.Parameters.AddWithValue("@NextAction", newPoiRecord.NextAction);

                        await conn.OpenAsync();

                        
                        int newId =  cmd.ExecuteNonQuery();

                        if (newId<=0)
                        {
                            return new ServiceResponse<string>
                            {
                                Success = false,
                                Message = "POI created Failed",
                                Data = null
                            };
                        }
                        
                    }
                }
                return new ServiceResponse<string>
                {
                    Success = true,
                    Message = "POI created successfully",
                    Data = null
                };
            }
            catch (Exception ex)
            {

                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = $"POI Creation Failed"
                };
            }
        }

        [HttpGet("get-poi")]
        public async Task<ServiceResponse<List<PoiRecord>>> GetPoiRecordsAsync()
        {
            var records = new List<PoiRecord>();

            try
            {

                string sql = "SELECT * FROM [PoiDetails]";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        await conn.OpenAsync();

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {

                            while (await reader.ReadAsync())
                            {
                                var record = new PoiRecord
                                {

                                   // Id = reader.GetInt32(reader.GetOrdinal("Id")),


                                    AgentId = reader.IsDBNull(reader.GetOrdinal("agent_id")) ? null : reader.GetString(reader.GetOrdinal("agent_id")),
                                    PoiId = reader.IsDBNull(reader.GetOrdinal("poi_id")) ? null : reader.GetString(reader.GetOrdinal("poi_id")),
                                    PoiName = reader.IsDBNull(reader.GetOrdinal("poi_name")) ? null : reader.GetString(reader.GetOrdinal("poi_name")),


                                    Latitude = reader.IsDBNull(reader.GetOrdinal("latitude")) ? null : reader.GetString(reader.GetOrdinal("latitude")),
                                    Longitude = reader.IsDBNull(reader.GetOrdinal("longitude")) ? null : reader.GetString(reader.GetOrdinal("longitude")),

                                    Status = reader.IsDBNull(reader.GetOrdinal("status")) ? false : reader.GetBoolean(reader.GetOrdinal("status")),


                                    SubmittedOn = reader.IsDBNull(reader.GetOrdinal("submitted_on")) ? null : reader.GetString(reader.GetOrdinal("submitted_on")),


                                    Category = reader.IsDBNull(reader.GetOrdinal("category")) ? null : reader.GetString(reader.GetOrdinal("category")),
                                    Address = reader.IsDBNull(reader.GetOrdinal("address")) ? null : reader.GetString(reader.GetOrdinal("address")),
                                    ContactNumber = reader.IsDBNull(reader.GetOrdinal("contact_number")) ? null : reader.GetString(reader.GetOrdinal("contact_number")),
                                    Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email")),
                                    Website = reader.IsDBNull(reader.GetOrdinal("website")) ? null : reader.GetString(reader.GetOrdinal("website")),
                                    SubCategory = reader.IsDBNull(reader.GetOrdinal("sub_category")) ? null : reader.GetString(reader.GetOrdinal("sub_category")),
                                    Amenities = reader.IsDBNull(reader.GetOrdinal("amenities")) ? null : reader.GetString(reader.GetOrdinal("amenities")),
                                    RoomTypes = reader.IsDBNull(reader.GetOrdinal("room_types")) ? null : reader.GetString(reader.GetOrdinal("room_types")),
                                    TargetAudience = reader.IsDBNull(reader.GetOrdinal("target_audience")) ? null : reader.GetString(reader.GetOrdinal("target_audience")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                                    MediaPhotos = reader.IsDBNull(reader.GetOrdinal("media_photos")) ? null : reader.GetString(reader.GetOrdinal("media_photos")),
                                    Media360 = reader.IsDBNull(reader.GetOrdinal("media_360")) ? null : reader.GetString(reader.GetOrdinal("media_360")),
                                    VerificationStatus = reader.IsDBNull(reader.GetOrdinal("verification_status")) ? null : reader.GetString(reader.GetOrdinal("verification_status")),
                                    FormVersion = reader.IsDBNull(reader.GetOrdinal("form_version")) ? null : reader.GetString(reader.GetOrdinal("form_version")),
                                    Remarks = reader.IsDBNull(reader.GetOrdinal("remarks")) ? null : reader.GetString(reader.GetOrdinal("remarks")),
                                    Message = reader.IsDBNull(reader.GetOrdinal("message")) ? null : reader.GetString(reader.GetOrdinal("message")),
                                    AssignedVerifier = reader.IsDBNull(reader.GetOrdinal("assigned_verifier")) ? null : reader.GetString(reader.GetOrdinal("assigned_verifier")),
                                    NextAction = reader.IsDBNull(reader.GetOrdinal("next_action")) ? null : reader.GetString(reader.GetOrdinal("next_action"))
                                };
                                records.Add(record);
                            }
                        }
                    }
                }
                return new ServiceResponse<List<PoiRecord>>
                {
                    Success = true,
                    Message = "POI fetched successfully",
                    Data = records
                };
            }
            catch (Exception ex)
            {

                return new ServiceResponse<List<PoiRecord>>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                  
                };
            }

           
        }

        [HttpGet("poi/fields")]
        public async Task<ActionResult<ServiceResponse<List<POIField>>>> GetFieldDetails(BasePOIRequest request)
        {
            if (string.IsNullOrEmpty(request.poiId))
            {
                return BadRequest(new ServiceResponse<POIField> { Success = false, Message = "poiId is required." });
            }

            // MobileNo is optional, so it is omitted from the required parameters here

            ServiceResponse<List<POIField>> response = await poiservice.GetPoiFieldsAsync(request.poiId);

            if (!response.Success)
            {
                return NotFound(response); // Or StatusCode(500, ...) if it's a server/DB error
            }

            return Ok(response);

        }

        [HttpPost("poi/submit")]
        public async Task<ActionResult<ServiceResponse<object>>> SubmitPOI(SubmitPOIRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.poiId) || string.IsNullOrEmpty(request.mobileNo))
            {
                return BadRequest(new ServiceResponse<object> { Success = false, Message = "POI ID and Mobile Number are required." });
            }

            ServiceResponse<object> response = await poiservice.SubmitPoiAsync(request);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response.Data);

        }

        [HttpPost("agent-details/create")]
        public async Task<IActionResult> Create([FromBody] JsonElement payload)
        {
            try
            {

                if (payload.ValueKind == JsonValueKind.Undefined)
                    return BadRequest("Invalid JSON");

                string jsonString = payload.ToString();
                JsonDocument doc = JsonDocument.Parse(jsonString);
                JsonElement root = doc.RootElement;

                // Simple field
                int poiId = Convert.ToInt32(root.GetProperty("PoiId").ToString());
                int agentID = Convert.ToInt32(root.GetProperty("AgentId").GetString());

                var result =  await poiservice.InsertAgentAsync(jsonString, poiId, agentID);
                return Ok(result);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                // log exception in real app
                return StatusCode(500, "An error occurred while inserting the record: " + ex.Message);
            }
        }

    }
}
