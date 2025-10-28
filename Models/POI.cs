using System;

namespace StudentManagement.Models
{
    public class PoiRecord
    {
        // Primary Identifiers
        public int Id { get; set; } // Added for a primary key, common in EF Core
        public string? PoiId { get; set; }
        public string? AgentId { get; set; }

        // Core Information
        public string? PoiName { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public string? Description { get; set; }
        public string? TargetAudience { get; set; }

        // Location Details
        public string? Address { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }

        // Contact Information
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }

        // Detailed/Complex Data (Often JSON strings or collections in the database)
        public string? Amenities { get; set; } // Could be a List<string> or JSON string
        public string? RoomTypes { get; set; } // Could be a List<string> or JSON string
        public string? MediaPhotos { get; set; } // Could be a List<string> or JSON string
        public string? Media360 { get; set; } // Could be a string URL or List<string>

        // Status and Workflow
        public string? VerificationStatus { get; set; }
        public bool? Status { get; set; }
        public string? Remarks { get; set; }
        public string? Message { get; set; }
        public string? AssignedVerifier { get; set; }
        public string? NextAction { get; set; }
        public string? FormVersion { get; set; }

        // Dates
        public string? SubmittedOn { get; set; }
    }
}
