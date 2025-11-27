using System.Collections.Generic;

namespace StudentManagement.Models
{
    public class PoiAssignmentInputDto
    {
        // Agent ID is usually passed in the URL or body, but we'll include it in the body for simplicity
        public string AgentId { get; set; }

        public int PoiId { get; set; } // Assuming the POI exists in a master table, but we insert it here for assignment
        public string PoiName { get; set; }
        public string Category { get; set; }
        public int CategoryId { get; set; }
        public string TaskPriority { get; set; }

        // Initial status for a new assignment
        public string TaskStatus { get; set; } = "Pending";
        public int Progress { get; set; } = 0;

        public string ContactNo { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        // Subtasks to be inserted
        public List<SubtaskInputDto> SubTasks { get; set; }
    }

    public class SubtaskInputDto
    {
        public string IconUrl { get; set; }
        public string Text { get; set; }
    }
}
